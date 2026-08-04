import ReactDOM from 'react-dom/client';
import App from './App';
import ErrorBoundary from './ErrorBoundary';
import { registerDisplayMediaCache } from './mediaCache.mjs';
import { preloadThemeFonts } from './notoFonts.mjs';
import './themeFonts';
import './player.css';

if (import.meta.env.PROD) {
  void registerDisplayMediaCache();
}
void preloadThemeFonts();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <ErrorBoundary>
    <App />
  </ErrorBoundary>
);
