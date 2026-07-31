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
import { applyRealtimeEvent, requiresContentReload } from './displayRealtime.mjs';
import {
  connectDisplayRealtime,
  type DisplayConnectionState,
  type DisplayRealtimeConnection
} from './signalRClient';
import { DisplayLayout } from './layouts/DisplayLayout';
import PlaylistRotation from './PlaylistRotation';
import EmergencyBroadcastOverlay from './EmergencyBroadcastOverlay';

type DisplayPageProps = {
  screenId: string;
  platform: string;
  appVersion: string;
};

type DisplayState =
  | { kind: 'loading' }
  | { kind: 'ready'; content: DisplayContent; source: 'network' | 'cache' }
  | { kind: 'not-found'; message: string }
  | { kind: 'api-error'; message: string };

export default function DisplayPage({ screenId, platform, appVersion }: DisplayPageProps) {
  const [state, setState] = useState<DisplayState>({ kind: 'loading' });
  const [connectionState, setConnectionState] = useState<DisplayConnectionState>('connecting');

  useEffect(() => {
    const preview = new URLSearchParams(window.location.search);
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
    let disposed = false;
    let liveServicesStarted = false;

    setState({ kind: 'loading' });
    setConnectionState('connecting');

    const startLiveServices = async () => {
      if (liveServicesStarted || disposed || previewTheme) {
        return;
      }

      liveServicesStarted = true;
      heartbeat = startDisplayHeartbeat(displayConfig.apiBaseUrl, screenId, { platform, appVersion });
      realtimeConnection = await connectDisplayRealtime(
        displayConfig.apiBaseUrl,
        screenId,
        {
          onConnectionStateChanged: (nextConnectionState) => {
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
              return { kind: 'ready', content, source: 'network' };
            });
          }
        }
      );
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

      const { content, source } = result;
      const themedContent = previewTheme ? { ...content, theme: previewTheme } : content;
      setState({ kind: 'ready', content: themedContent, source });

      if (previewTheme) {
        setConnectionState('connected');
      } else if (result.source === 'network') {
        await startLiveServices();
      } else {
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
      void realtimeConnection?.stop();
    };
  }, [appVersion, platform, screenId]);

  if (state.kind === 'loading') {
    return (
      <main aria-busy="true" aria-live="polite">
        <h1>Vennu Display</h1>
        <p>Loading display…</p>
      </main>
    );
  }

  if (state.kind === 'not-found') {
    return (
      <main role="alert">
        <h1>Display not found</h1>
        <p>{state.message}</p>
      </main>
    );
  }

  if (state.kind === 'api-error') {
    return (
      <main role="alert">
        <h1>Display unavailable</h1>
        <p>{state.message}</p>
      </main>
    );
  }

  const { content } = state;

  return (
    <>
      <p aria-live="polite">Real-time connection: {connectionState}</p>
      {state.source === 'cache' && (
        <p aria-live="polite">Offline — showing the last saved menu.</p>
      )}
      <EmergencyBroadcastOverlay content={content}>
        <PlaylistRotation content={content}><DisplayLayout content={content} /></PlaylistRotation>
      </EmergencyBroadcastOverlay>
    </>
  );
}
