import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import { claimPreRegisteredScreen } from './provisioning.mjs';
import PlayerStateScreen from './PlayerStateScreen';
import { getDisplayStatePresentation } from './displayPresentation.mjs';

type Props = { token: string; platform: string; appVersion: string };

export default function ProvisioningPage({ token, platform, appVersion }: Props) {
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let disposed = false;
    claimPreRegisteredScreen(displayConfig.apiBaseUrl, token, platform, appVersion)
      .then(result => {
        if (!disposed) window.location.replace(result.displayPath);
      })
      .catch(() => {
        if (!disposed) setFailed(true);
      });
    return () => { disposed = true; };
  }, [appVersion, platform, token]);

  return failed
    ? <PlayerStateScreen {...getDisplayStatePresentation('provisioning-error')} />
    : <PlayerStateScreen {...getDisplayStatePresentation('provisioning')} />;
}
