import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import {
  PAIRING_POLL_INTERVAL_MS,
  PAIRING_SCREEN_STORAGE_KEY,
  displayPath,
  loadPairingStatus,
  preparePairingScreen
} from './pairing.mjs';
import './pairing.css';

type PairingState =
  | { kind: 'loading' }
  | { kind: 'ready'; code: string; expiresAt: string }
  | { kind: 'error' };

type PairingPageProps = { platform: string; appVersion: string };

export default function PairingPage({ platform, appVersion }: PairingPageProps) {
  const [state, setState] = useState<PairingState>({ kind: 'loading' });

  useEffect(() => {
    let disposed = false;
    let pollHandle: number | undefined;
    let expiryHandle: number | undefined;
    let activeCode = '';
    let screenId = window.localStorage.getItem(PAIRING_SCREEN_STORAGE_KEY) ?? '';

    const regenerate = async () => {
      const pairing = await preparePairingScreen(
        displayConfig.apiBaseUrl,
        screenId,
        platform,
        appVersion
      );
      if (pairing.screenId !== screenId) {
        screenId = pairing.screenId;
        window.localStorage.setItem(PAIRING_SCREEN_STORAGE_KEY, screenId);
      }
      if (disposed) return;
      activeCode = pairing.code;
      setState({ kind: 'ready', code: pairing.code, expiresAt: pairing.expiresAt });
      window.clearTimeout(expiryHandle);
      expiryHandle = window.setTimeout(
        () => void regenerate().catch(() => setState({ kind: 'error' })),
        Math.max(0, Date.parse(pairing.expiresAt) - Date.now())
      );
    };

    const poll = async () => {
      if (!activeCode || disposed) return;
      try {
        const status = await loadPairingStatus(displayConfig.apiBaseUrl, activeCode);
        if (status.linked && status.screenId) {
          window.location.replace(displayPath(status.screenId));
        }
      } catch (error) {
        if (error instanceof Error && 'status' in error && error.status === 410) {
          await regenerate();
        }
      }
    };

    regenerate()
      .then(() => {
        pollHandle = window.setInterval(() => void poll(), PAIRING_POLL_INTERVAL_MS);
      })
      .catch(() => {
        if (!disposed) setState({ kind: 'error' });
      });

    return () => {
      disposed = true;
      window.clearInterval(pollHandle);
      window.clearTimeout(expiryHandle);
    };
  }, [appVersion, platform]);

  if (state.kind === 'loading') {
    return <main className="pairing-page" aria-busy="true"><p>Preparing this TV…</p></main>;
  }

  if (state.kind === 'error') {
    return <main className="pairing-page" role="alert"><h1>Pairing unavailable</h1><p>Check the network connection and reload this screen.</p></main>;
  }

  return <main className="pairing-page">
    <p className="eyebrow">Vennu TV setup</p>
    <h1>Pair this screen</h1>
    <p>In Vennu Admin, open the venue’s Screens section and enter:</p>
    <strong className="pairing-code" aria-label={`Pairing code ${state.code.split('').join(' ')}`}>{state.code}</strong>
    <p className="pairing-expiry">This code refreshes automatically every 10 minutes.</p>
  </main>;
}
