# Android TV distribution readiness

## Automated in this repository

- Assemble `googleTvDebug` and `fireTvDebug` from JDK 17 / Gradle 8.9.
- Verify distinct application IDs and platform bridge identities.
- Source version code, version name, and hosted-player URL from documented build inputs.
- Exclude APKs, app bundles, keystores, passwords, and store credentials from source control.

## Release-operator checklist

For each channel:

- Confirm production hosted-player URL and monotonically increasing version code.
- Supply signing material only through the approved release environment.
- Replace provisional listing artwork, screenshots, privacy URL, support URL, and release notes.
- Run accessibility, remote-control, network-loss, reboot, pairing, kiosk-exit, and upgrade checks on supported physical devices.
- Complete store enrollment, content ratings, data-safety declarations, signing, upload, review, and certification outside this repository.

No item in this checklist is evidence that a store has accepted or certified the application.
