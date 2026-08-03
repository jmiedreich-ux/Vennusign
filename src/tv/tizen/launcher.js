(function launchVennuTizen() {
  'use strict';

  const config = window.VENNU_TIZEN_CONFIG;
  const origin = new URL(config.playerOrigin);
  if (origin.protocol !== 'https:') throw new Error('Vennusign player origin must use HTTPS.');

  try {
    window.tizen?.tvinputdevice?.registerKeyBatch(['MediaPlayPause', 'MediaStop']);
  } catch {
    // The shared player remains remote-safe when optional media keys are unavailable.
  }

  window.addEventListener('keydown', (event) => {
    if (event.keyCode === 10009) {
      window.tizen?.application?.getCurrentApplication()?.exit();
    }
  });

  const target = new URL('/pair', origin);
  target.searchParams.set('vennuPlatform', 'tizen');
  target.searchParams.set('vennuVersion', String(config.appVersion).trim().slice(0, 50));
  window.location.replace(target.toString());
})();
