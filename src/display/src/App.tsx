import DisplayPage from './DisplayPage';
import PairingPage from './PairingPage';
import ProvisioningPage from './ProvisioningPage';
import { readPlatformBootstrap, resolvePlatformLaunch } from './platformLaunch.mjs';
import { resolveDisplayRoute } from './routing';
import PlayerStateScreen from './PlayerStateScreen';
import { getDisplayStatePresentation } from './displayPresentation.mjs';

export default function App() {
  const resetPairing = new URLSearchParams(window.location.search).get('vennuReset') === '1';
  const bridge = window.__VENNU_PLATFORM__ ?? readPlatformBootstrap(window.location.search);
  const launch = resolvePlatformLaunch(window.location.pathname, bridge, resetPairing);
  const route = resolveDisplayRoute(launch.pathname);

  if (route.kind === 'pair') {
    return <PairingPage platform={launch.platform} appVersion={launch.appVersion} />;
  }

  if (route.kind === 'provision' && launch.provisioningToken) {
    return <ProvisioningPage token={launch.provisioningToken} platform={launch.platform} appVersion={launch.appVersion} />;
  }

  if (route.kind === 'not-found' || route.kind === 'provision') {
    return <PlayerStateScreen {...getDisplayStatePresentation('route-not-found')} />;
  }

  return <DisplayPage screenId={route.screenId} platform={launch.platform} appVersion={launch.appVersion} />;
}
