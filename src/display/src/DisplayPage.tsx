import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import {
  DisplayContentError,
  loadDisplayContent,
  type DisplayContent
} from './displayContent.mjs';

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

  useEffect(() => {
    const abortController = new AbortController();

    setState({ kind: 'loading' });

    loadDisplayContent(
      displayConfig.apiBaseUrl,
      screenId,
      (input, init) => fetch(input, { ...init, signal: abortController.signal })
    )
      .then((content) => setState({ kind: 'ready', content }))
      .catch((error: unknown) => {
        if (abortController.signal.aborted) {
          return;
        }

        if (error instanceof DisplayContentError) {
          setState({ kind: error.kind, message: error.message });
          return;
        }

        setState({ kind: 'api-error', message: 'The display content could not be loaded.' });
      });

    return () => abortController.abort();
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
    <main>
      <header>
        <h1>{content.screenName}</h1>
        <p>{content.status}</p>
      </header>
      <dl>
        <dt>Screen key</dt>
        <dd>{content.screenKey}</dd>
        <dt>Layout</dt>
        <dd>{content.layout}</dd>
        <dt>Last seen</dt>
        <dd>{content.lastSeenUtc ?? 'Not yet reported'}</dd>
      </dl>
    </main>
  );
}
