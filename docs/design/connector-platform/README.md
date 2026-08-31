# VennuSign Connector Platform

Technical design package for the reusable cinema-industry integration platform.

## Contents

- `connector-platform-technical-design.html` — complete styled architecture document with rendered Mermaid diagrams.
- `connector-platform-technical-design.md` — repository-friendly Markdown version for reviews and future edits.

## Confirmed scope

- Up to 1,000 customer locations.
- Within-seconds updates for REST pull and push flows.
- REST API pull, inbound REST API push, and SFTP file ingestion.
- Snapshot and incremental updates through a shared mapping and canonical-data layer.
- Operational display data, with customer or order data only when needed.
- No payment-card data.
- Integration-controlled records do not permit manual VennuSign edits.
- Last-known-good data is retained when a connector fails or sends invalid data.
- Standard availability target of 99.9%, with recovery within two hours.

## Viewing the design

Open `connector-platform-technical-design.html` in a browser. The Markdown version also contains all Mermaid source diagrams for repository-native review.

Live reference: <https://vennusign-connector-platform-design.jmiedreich.chatgpt.site>
