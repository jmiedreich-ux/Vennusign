# LG webOS launcher

This credential-free webOS TV package launches the authoritative hosted Vennu player at `/pair` with approved `webos` platform and bounded app-version metadata.

- Run `node scripts/validate.mjs` for repository validation.
- Run `bash scripts/build-unsigned.sh` from an environment with the LG webOS TV CLI for an unsigned IPK.
- Developer Mode, signing, simulator/device installation, LG Seller Lounge submission, and store certification are release operations and are not stored here.

The launcher permits only the configured HTTPS player origin. It handles webOS relaunch, foreground restoration, and remote Back while the shared React player remains responsible for pairing, layouts, realtime updates, scheduling, and offline behavior.
