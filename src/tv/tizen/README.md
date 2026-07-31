# Samsung Tizen launcher

This credential-free Tizen web package launches the authoritative hosted Vennu player at `/pair` with the approved `tizen` platform and bounded app-version metadata.

- Run `node scripts/validate.mjs` for repository validation.
- Run `scripts/build-unsigned.sh` from an environment with Tizen Studio CLI for an unsigned web build.
- Certificate profiles, signing, WGT packaging, simulator/device install, and Samsung Seller Office submission are release operations and are not stored here.

The launcher allows only the configured HTTPS player origin. The shared React player remains responsible for pairing, layouts, realtime updates, scheduling, and offline behavior.
