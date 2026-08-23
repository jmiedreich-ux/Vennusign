import { useEffect, useRef, useState } from 'react';
import { displayConfig } from './config';
import { readCachedDisplayContent } from './displayCache.mjs';
import { readDisplayDiagnostics, type DisplayDiagnosticsRecord } from './displayDiagnostics.mjs';
import { readDeviceGeometry, describeBoardFit, describeThemeCoverage, type DeviceGeometry, type BoardFit, type ThemeCoverage } from './displayGeometry.mjs';
import { loadServerDiagnostics, type ServerDiagnosticsResult } from './displayDiagnosticsApi.mjs';
import { DisplayLayout } from './layouts/DisplayLayout';
import type { DisplayContent } from './displayContent.mjs';
import './diagnostics.css';

type DiagnosticsPageProps = { screenId: string; platform: string; appVersion: string };

const SERVER_REFRESH_INTERVAL_MS = 5_000;

function formatDate(value: string | number | null | undefined) {
  if (value === null || value === undefined) return 'never';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
}

export default function DiagnosticsPage({ screenId, platform, appVersion }: DiagnosticsPageProps) {
  const [local] = useState<DisplayDiagnosticsRecord>(() => readDisplayDiagnostics(screenId));
  const [geometry] = useState<DeviceGeometry | null>(() => readDeviceGeometry(window));
  const [cachedContent] = useState<DisplayContent | null>(() => readCachedDisplayContent(screenId)?.content ?? null);
  const [server, setServer] = useState<ServerDiagnosticsResult | null>(null);
  const [boardFit, setBoardFit] = useState<BoardFit>({ measured: false });
  const boardPreviewRef = useRef<HTMLDivElement>(null);

  // This page is a probe, not the player: it never sends a heartbeat and never posts a content
  // receipt, so opening it from any device never marks a screen Online or writes a delivery
  // record that the wall never actually received (see DisplayPage.tsx's own observer guard).
  useEffect(() => {
    let disposed = false;

    const refresh = () => {
      void loadServerDiagnostics(displayConfig.apiBaseUrl, screenId).then((result) => {
        if (!disposed) setServer(result);
      });
    };

    refresh();
    const timer = window.setInterval(refresh, SERVER_REFRESH_INTERVAL_MS);
    return () => {
      disposed = true;
      window.clearInterval(timer);
    };
  }, [screenId]);

  useEffect(() => {
    if (!cachedContent || !geometry || !boardPreviewRef.current) return;
    const frame = window.requestAnimationFrame(() => {
      if (!boardPreviewRef.current) return;
      setBoardFit(describeBoardFit(boardPreviewRef.current.scrollHeight, geometry.viewport.height));
    });
    return () => window.cancelAnimationFrame(frame);
  }, [cachedContent, geometry]);

  const themeCoverage: ThemeCoverage | null = cachedContent
    ? describeThemeCoverage(cachedContent.layout, cachedContent.theme ?? null)
    : null;

  return (
    <main className="diagnostics-page">
      <p className="eyebrow">Vennusign display diagnostics</p>
      <h1>Screen {screenId}</h1>
      <p className="caution">This is a probe. It reads what this device already knows and asks the server once every 5 seconds - it never reports this screen online.</p>

      <div className="diagnostics-grid">
        <section className="diagnostics-panel">
          <h2>Build (this device)</h2>
          <dl>
            <dt>API origin compiled in</dt><dd>{displayConfig.apiBaseUrl || '(same origin)'}</dd>
            <dt>Player version</dt><dd>{displayConfig.playerVersion}</dd>
            <dt>Shell version</dt><dd>{appVersion}</dd>
            <dt>Platform</dt><dd>{platform}</dd>
          </dl>
        </section>

        <section className="diagnostics-panel">
          <h2>Geometry (this device)</h2>
          {geometry ? (
            <dl>
              <dt>Viewport</dt><dd>{geometry.viewport.width}×{geometry.viewport.height}</dd>
              <dt>Screen</dt><dd>{geometry.screen ? `${geometry.screen.width}×${geometry.screen.height}` : 'unknown'}</dd>
              <dt>Device pixel ratio</dt><dd>{geometry.devicePixelRatio}</dd>
              <dt>Orientation</dt><dd>{geometry.orientation ?? 'unknown'}</dd>
            </dl>
          ) : <p>Not available.</p>}
        </section>

        <section className="diagnostics-panel">
          <h2>Cached content and board fit (this device)</h2>
          {cachedContent ? (
            <>
              <dl>
                <dt>Cached revision</dt><dd>{cachedContent.contentRevision ?? 'none'}</dd>
                <dt>Layout</dt><dd>{cachedContent.layout}</dd>
                <dt>Sections</dt><dd>{cachedContent.sections?.length ?? 0}</dd>
                <dt>Board fit</dt>
                <dd className={boardFit.measured && !boardFit.fits ? 'flag-warn' : 'flag-ok'}>
                  {boardFit.measured
                    ? (boardFit.fits ? 'Fits the viewport' : `${boardFit.overflowPixels}px taller than the viewport - content below the fold`)
                    : 'Measuring…'}
                </dd>
                {themeCoverage && (
                  <>
                    <dt>Theme fields consumed</dt>
                    <dd className={themeCoverage.known && themeCoverage.themeFieldsConsumed < themeCoverage.themeFieldsServed ? 'flag-warn' : 'flag-ok'}>
                      {themeCoverage.known ? `${themeCoverage.themeFieldsConsumed} of ${themeCoverage.themeFieldsServed}` : 'unknown layout'}
                    </dd>
                  </>
                )}
              </dl>
              <div className="diagnostics-board-preview" style={{ height: '12rem' }}>
                <div
                  ref={boardPreviewRef}
                  style={{
                    width: geometry?.viewport.width ?? 1920,
                    height: geometry?.viewport.height,
                    transform: `scale(${(180 / (geometry?.viewport.width || 1920)).toFixed(4)})`,
                    transformOrigin: 'top left'
                  }}
                >
                  <DisplayLayout content={cachedContent} />
                </div>
              </div>
            </>
          ) : <p>No content has been cached on this device yet.</p>}
        </section>

        <section className="diagnostics-panel">
          <h2>Server</h2>
          {!server && <p>Loading…</p>}
          {server?.kind === 'not-found' && <p>The server does not know this screen id.</p>}
          {server?.kind === 'error' && <p className="flag-warn">{server.message}</p>}
          {server?.kind === 'ok' && (
            <dl>
              <dt>Assigned to a venue</dt><dd className={server.diagnostics.isAssignedToVenue ? 'flag-ok' : 'flag-warn'}>{server.diagnostics.isAssignedToVenue ? 'Yes' : 'No'}</dd>
              <dt>Status</dt><dd>{server.diagnostics.status}</dd>
              <dt>Last seen</dt><dd>{formatDate(server.diagnostics.lastSeenUtc)}</dd>
              <dt>Stale</dt><dd className={server.diagnostics.isStale ? 'flag-warn' : 'flag-ok'}>{server.diagnostics.isStale ? 'Yes' : 'No'}</dd>
              <dt>Platform / version</dt><dd>{server.diagnostics.platform ?? 'unknown'} / {server.diagnostics.appVersion ?? 'unknown'}</dd>
              <dt>Configured size</dt><dd>{server.diagnostics.configuredWidthPixels}×{server.diagnostics.configuredHeightPixels}</dd>
              <dt>Authoritative / applied revision</dt><dd>{server.diagnostics.authoritativeRevision ?? 'none'} / {server.diagnostics.appliedRevision ?? 'none'}</dd>
              <dt>Delivery state</dt><dd>{server.diagnostics.deliveryState ?? 'none'}</dd>
              <dt>Onboarding first screen</dt><dd>{server.diagnostics.isOnboardingFirstScreen ? `Yes, go-live ${formatDate(server.diagnostics.onboardingGoLiveAchievedUtc)}` : 'No'}</dd>
            </dl>
          )}
        </section>

        <section className="diagnostics-panel">
          <h2>Recent events on this device</h2>
          {local.events.length === 0 && <p>No events recorded on this device yet.</p>}
          {local.events.length > 0 && (
            <ul className="diagnostics-timeline">
              {[...local.events].reverse().map((event, index) => (
                <li key={`${event.kind}-${event.at}-${index}`}>
                  <strong>{event.kind}</strong> · {formatDate(event.at)} · {JSON.stringify(event.detail)}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </main>
  );
}
