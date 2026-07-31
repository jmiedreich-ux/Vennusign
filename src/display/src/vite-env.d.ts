/// <reference types="vite/client" />

interface Window {
  __VENNU_PLATFORM__?: {
    platform?: string;
    appVersion?: string;
    screenId?: string;
  };
}
