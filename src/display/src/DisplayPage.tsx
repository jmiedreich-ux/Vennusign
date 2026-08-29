import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import {
  DisplayContentError,
  type DisplayContent
} from './displayContent.mjs';
import {
  cacheDisplayContent,
  loadDisplayContentResilient
} from './displayCache.mjs';
import { startDisplayHeartbeat } from './displayHeartbeat.mjs';
import { reportContentReceipt, describeReceiptSkipReason } from './displayReceipts.mjs';
import { recordDisplayDiagnosticEvent } from './displayDiagnostics.mjs';
import { applyRealtimeEvent, requiresContentReload } from './displayRealtime.mjs';
import {
  connectDisplayRealtime,
  type DisplayConnectionState,
  type DisplayRealtimeConnection
} from './signalRClient';
import { DisplayLayout } from './layouts/DisplayLayout';
import PlaylistRotation from './PlaylistRotation';
import EmergencyBroadcastOverlay from './EmergencyBroadcastOverlay';
import PlayerStateScreen from './PlayerStateScreen';
import {
import { useRotatedContent } from './usePageRotation';
  describeCachedContent,
  getConnectionPresentation,
  getDisplayStatePresentation
} from './displayPresentation.mjs';

type DisplayPageProps = {
  screenId: string;
  platform: string;
  appVersion: string;
};

type DisplayState =
  | { kind: 'loading' }
  | { kind: 'ready'; content: DisplayContent; source: 'network' | 'cache'; cachedAt: number }
  | { kind: 'not-found'; message: string }
  | { kind: 'api-error'; message: string };

export const DISPLAY_CONTENT_RECOVERY_INTERVAL_MS = 60_000;

export default function DisplayPage({ screenId, platform, appVersion }: DisplayPageProps) {
  const [state, setState] = useState<DisplayState>({ kind: 'loading' });

  /*
   * The page cycle, computed here because a hook cannot live behind the early returns below.
   * It gives back the content unchanged until there is more than one page to turn.
   */
  const rotated = useRotatedContent(state.kind === 'ready' ? state.content : undefined);
  const [connectionState, setConnectionState] = useState<DisplayConnectionState>('connecting');
  const [loadAttempt, setLoadAttempt] = useState(0);

  useEffect(() => {
    const preview = new URLSearchParams(window.location.search);
    // A thumbnail is an observer, not a player. Without this, every embedded preview
    // heartbeats and reports its screen Online, so the Back Office fleet view fakes
    // player presence for screens that were never connected.
    const observerOnly = preview.get('preview') === 'observer';
    const previewTheme = preview.get('preview') === 'theme'
      ? {
          backgroundColor: preview.get('background') ?? '#111315',
          accentColor: preview.get('accent') ?? '#FFB74D',
          fontFamily: preview.get('font') ?? 'Inter',
          presetKey: preview.get('preset') ?? 'custom',
          titleColor: preview.get('title') ?? '#F8F5E9',
          glowColor: preview.get('glow') ?? '#00E5FF',
          boardBackgroundColor: preview.get('board') ?? '#071013',
          sectionColors: (preview.get('sections') ?? '#00E5FF,#FF2BD6,#FFE66D,#7CFF6B').split(','),
          glowIntensity: Number(preview.get('intensity') ?? '1'),
          titleFont: preview.get('titleFont') ?? 'Righteous',
          itemFont: preview.get('itemFont') ?? 'Caveat'
        } as DisplayContent['theme']
      : undefined;
    const abortController = new AbortController();
    let realtimeConnection: DisplayRealtimeConnection | undefined;
    let heartbeat: { stop: () => void } | undefined;
    let recoveryTimer: ReturnType<typeof setInterval> | undefined;
    let disposed = false;
    let liveServicesStarted = false;
    let recoveringFromCache = false;
    const trackDiagnostics = !previewTheme && !observerOnly;

    setState({ kind: 'loading' });
    setConnectionState('connecting');

    const startLiveServices = async () => {
      if (liveServicesStarted || disposed || previewTheme || observerOnly) {
        return;
      }

      liveServicesStarted = true;
      heartbeat = startDisplayHeartbeat(displayConfig.apiBaseUrl, screenId, {
        platform,
        appVersion,
        onResult: (result) => {
          if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'heartbeat', result);
        }
      });
      realtimeConnection = await connectDisplayRealtime(
        displayConfig.apiBaseUrl,
        screenId,
        {
          onConnectionStateChanged: (nextConnectionState) => {
            if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'connection', { state: nextConnectionState });
            if (!disposed) {
              setConnectionState(nextConnectionState);
            }
          },
          onEvent: (eventName, ...args) => {
            if (disposed) {
              return;
            }
            if (requiresContentReload(eventName, args[0])) {
              void loadAndActivate();
              return;
            }

            setState((currentState) => {
              if (currentState.kind !== 'ready') {
                return currentState;
              }

              const content = applyRealtimeEvent(currentState.content, eventName, ...args);
              cacheDisplayContent(screenId, content);
              return { kind: 'ready', content, source: 'network', cachedAt: Date.now() };
            });
          }
        }
      );
      recoveryTimer = window.setInterval(() => {
        if (!disposed) void loadAndActivate();
      }, DISPLAY_CONTENT_RECOVERY_INTERVAL_MS);
    };

    const loadAndActivate = async () => {
      const result = await loadDisplayContentResilient(
        displayConfig.apiBaseUrl,
        screenId,
        {
          fetchImpl: (input, init) => fetch(input, { ...init, signal: abortController.signal })
        }
      );

      if (disposed) {
        return;
      }

      const { content, source, cachedAt } = result;
      const themedContent = previewTheme ? { ...content, theme: previewTheme } : content;

      if (trackDiagnostics) {
        recordDisplayDiagnosticEvent(screenId, 'content-fetch', { source, revision: content.contentRevision ?? null });
      }

      if (!previewTheme && source === 'network') {
        const skipReason = describeReceiptSkipReason(content);
        if (skipReason) {
          if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'receipt', { posted: false, reason: skipReason });
        } else {
          const metadata = { playerVersion: displayConfig.playerVersion, shellVersion: appVersion, platform, recovered: recoveringFromCache };
          await reportContentReceipt(displayConfig.apiBaseUrl, screenId, content, 'Received', metadata)
            .then(() => { if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'receipt', { posted: true, state: 'Received' }); })
            .catch(() => { if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'receipt', { posted: false, reason: 'request-failed' }); });
          window.requestAnimationFrame(() => {
            void reportContentReceipt(displayConfig.apiBaseUrl, screenId, content, 'Applied', metadata)
              .then(() => { if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'receipt', { posted: true, state: 'Applied' }); })
              .catch(() => { if (trackDiagnostics) recordDisplayDiagnosticEvent(screenId, 'receipt', { posted: false, reason: 'request-failed' }); });
          });
        }
      }
      setState({ kind: 'ready', content: themedContent, source, cachedAt });

      if (previewTheme) {
        setConnectionState('connected');
      } else if (result.source === 'network') {
        await startLiveServices();
      } else {
        recoveringFromCache = true;
        setConnectionState('degraded');
      }
    };

    const recoverOnline = () => {
      loadAndActivate().catch(() => {
        if (!disposed) {
          setConnectionState('degraded');
        }
      });
    };

    window.addEventListener('online', recoverOnline);

    loadAndActivate()
      .then(() => {
        if (disposed) {
          return;
        }
      })
      .catch((error: unknown) => {
        if (abortController.signal.aborted || disposed) {
          return;
        }

        if (error instanceof DisplayContentError) {
          setState({ kind: error.kind, message: error.message });
          return;
        }

        setState({ kind: 'api-error', message: 'The display content could not be loaded.' });
      });

    return () => {
      disposed = true;
      window.removeEventListener('online', recoverOnline);
      abortController.abort();
      heartbeat?.stop();
      if (recoveryTimer) window.clearInterval(recoveryTimer);
      void realtimeConnection?.stop();
    };
  }, [appVersion, loadAttempt, platform, screenId]);

  if (state.kind === 'loading') {
    return <PlayerStateScreen {...getDisplayStatePresentation('loading')} />;
  }

  if (state.kind === 'not-found') {
    const presentation = getDisplayStatePresentation('not-found');
    return <PlayerStateScreen {...presentation} message={state.message || presentation.message} onAction={() => setLoadAttempt(value => value + 1)} />;
  }

  if (state.kind === 'api-error') {
    const presentation = getDisplayStatePresentation('api-error');
    return <PlayerStateScreen {...presentation} message={state.message || presentation.message} onAction={() => setLoadAttempt(value => value + 1)} />;
  }

  const { content } = state;
  const connection = getConnectionPresentation(connectionState);

  return (
    <>
      <p className={`player-status${connection.visible && state.source === 'network' ? ' player-status--connection' : ''}`} aria-live="polite">
        <span className={`player-status__heartbeat player-status__heartbeat--${connection.tone}`} aria-hidden="true" />
        {connection.label}
      </p>
      {state.source === 'cache' && (
        <p className="player-status player-status--offline" aria-live="polite">
          <span className="player-status__heartbeat player-status__heartbeat--offline" aria-hidden="true" />
          {describeCachedContent(state.cachedAt)}
        </p>
      )}
      <EmergencyBroadcastOverlay content={content}>
        {/* The pages of this menu turn inside the playlist's own cycle, not instead of it. */}
        <PlaylistRotation content={content}><DisplayLayout content={rotated ?? content} /></PlaylistRotation>
      </EmergencyBroadcastOverlay>
    </>
  );
}
