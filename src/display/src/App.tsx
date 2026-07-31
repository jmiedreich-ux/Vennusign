import DisplayPage from './DisplayPage';
import PairingPage from './PairingPage';
import { readPlatformBootstrap, resolvePlatformLaunch } from './platformLaunch.mjs';
import { resolveDisplayRoute } from './routing';

export default function App() {
  const resetPairing = new URLSearchParams(window.location.search).get('vennuReset') === '1';
  const bridge = window.__VENNU_PLATFORM__ ?? readPlatformBootstrap(window.location.search);
  const launch = resolvePlatformLaunch(window.location.pathname, bridge, resetPairing);
  const route = resolveDisplayRoute(launch.pathname);

  if (route.kind === 'pair') {
    return <PairingPage platform={launch.platform} appVersion={launch.appVersion} />;
  }

  if (route.kind === 'not-found') {
    return (
      <main>
        <h1>Display not found</h1>
        <p>Use a display URL with a screen identifier.</p>
      </main>
    );
  }

  return <DisplayPage screenId={route.screenId} />;
}
