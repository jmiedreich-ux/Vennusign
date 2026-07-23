# Vennu Display

The display client is a standalone React and Vite application. Run it independently from the .NET solution.

## Requirements

- Node.js 20 or later
- npm

## Setup

```powershell
cd src/display
Copy-Item .env.example .env.local
npm ci
```

Configure `.env.local` when the API is hosted on a different origin:

```text
VITE_API_BASE_URL=https://localhost:7001
VITE_SIGNALR_HUB_URL=https://localhost:7001/hubs/vennu
```

The hub URL defaults to `<VITE_API_BASE_URL>/hubs/vennu` when it is not supplied.

## Commands

```powershell
npm run dev
npm run build
npm run preview
```

Open a display route using:

```text
http://localhost:5173/display/<screenId>
```

WP-02.08 only establishes routing and configuration. Content loading, SignalR group membership, event handling, and heartbeat scheduling are implemented by later work packages.
