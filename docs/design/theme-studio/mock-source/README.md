# Archived Theme Studio mock source

This folder contains the authored source for the published Vennue Theme Studio workflow mock.

The mock is a React/TSX application, so there is no standalone authored HTML file. HTML is produced by React/Vinext from:

- `app/page.tsx` — complete interactive workflow;
- `app/globals.css` — complete visual treatment;
- `app/layout.tsx` — document shell and metadata.

The most important source files are browseable in this folder. The exact full tracked project—including `package-lock.json`, public assets, worker code, configuration, tests, and the locally cached font files—is preserved in:

[`../vennue-theme-studio-source-2026-08-13.zip`](../vennue-theme-studio-source-2026-08-13.zip)

## Source identity

- Live site: https://vennue-theme-studio-draft.jmiedreich.chatgpt.site
- Sites project: `appgprj_6a7d30b04df0819191071e15a087eeec`
- Published version: 13
- Source commit: `35c33d1bb01ae2b5384e72d95adeb8861e43114a`
- Archive SHA-256: `db9f270af144c03f99ae313efe72631ff9d6a321a9a826be389ebf25cb9d6673`

## Restore

1. Download and unzip the archive.
2. Use Node 22.13 or newer.
3. Run `npm ci`.
4. Run `npm test`.
5. Run `npm run dev` for the editable mock.

The archive is the exact recovery artifact. The browseable files are included to make design review possible without downloading it.
