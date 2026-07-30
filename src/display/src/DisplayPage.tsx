import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import {
  DisplayContentError,
  loadDisplayContent,
  type DisplayContent
} from './displayContent.mjs';
import { startDisplayHeartbeat } from './displayHeartbeat.mjs';
import { applyRealtimeEvent } from './displayRealtime.mjs';
import {
  connectDisplayRealtime,
  type DisplayConnectionState,
  type DisplayRealtimeConnection
} from './signalRClient';
import { DisplayLayout } from './layouts/DisplayLayout';

type DisplayPageProps = {
  screenId: string;
};

type DisplayState =
  | { kind: 'loading' }
  | { kind: 'ready'; content: DisplayContent }
  | { kind: 'not-found'; message: string }
  | { kind: 'api-error'; message: string };

export default function DisplayPage({ screenId }: DisplayPageProps) {
  const [state, setState] = useState<DisplayState>({ kind: 'loading' });
  const [connectionState, setConnectionState] = useState<DisplayConnectionState>('connecting');

  useEffect(() => {
    const abortController = new AbortController();
    let realtimeConnection: DisplayRealtimeConnection | undefined;
    let heartbeat: { stop: () => void } | undefined;
    let disposed = false;

    setState({ kind: 'loading' });
    setConnectionState('connecting');

    loadDisplayContent(
      displayConfig.apiBaseUrl,
      screenId,
      (input, init) => fetch(input, { ...init, signal: abortController.signal })
    )
      .then(async (content) => {
        if (disposed) {
          return;
        }

        setState({ kind: 'ready', content });
        heartbeat = startDisplayHeartbeat(displayConfig.apiBaseUrl, screenId);

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

              setState((currentState) =>
                currentState.kind === 'ready'
                  ? {
                      kind: 'ready',
                      content: applyRealtimeEvent(currentState.content, eventName, ...args)
                    }
                  : currentState
              );
            }
          }
        );
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
      abortController.abort();
      heartbeat?.stop();
      void realtimeConnection?.stop();
    };
  }, [screenId]);

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
      <DisplayLayout content={content} />
    </>
  );
}
