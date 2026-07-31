import { useEffect, useState } from 'react';
import { displayConfig } from './config';
import { claimPreRegisteredScreen } from './provisioning.mjs';

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
    ? <main role="alert"><h1>Provisioning unavailable</h1><p>Contact Vennu support for a new delivery token.</p></main>
    : <main aria-busy="true"><p>Preparing this pre-registered TV…</p></main>;
}
