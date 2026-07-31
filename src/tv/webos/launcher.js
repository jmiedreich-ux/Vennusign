(function launchVennuWebOs() {
  'use strict';

  const config = window.VENNU_WEBOS_CONFIG;
  const origin = new URL(config.playerOrigin);
  if (origin.protocol !== 'https:') throw new Error('Vennu player origin must use HTTPS.');

  let launchStarted = false;

  function exitApplication() {
    if (window.webOS && typeof window.webOS.platformBack === 'function') {
      window.webOS.platformBack();
      return;
    }

    window.close();
  }

  function launchPlayer() {
    if (launchStarted) return;
    launchStarted = true;

    const target = new URL('/pair', origin);
    target.searchParams.set('vennuPlatform', 'webos');
    target.searchParams.set('vennuVersion', String(config.appVersion).trim().slice(0, 50));
    window.location.replace(target.toString());
  }

  window.addEventListener('keydown', (event) => {
    if (event.keyCode === 461) exitApplication();
  });

  document.addEventListener('webOSRelaunch', () => {
    launchStarted = false;
    launchPlayer();
  });

  document.addEventListener('visibilitychange', () => {
    if (!document.hidden) launchPlayer();
  });

  launchPlayer();
})();
