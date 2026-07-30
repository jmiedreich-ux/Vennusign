import ReactDOM from 'react-dom/client';
import App from './App';
import ErrorBoundary from './ErrorBoundary';
import { registerDisplayMediaCache } from './mediaCache.mjs';

if (import.meta.env.PROD) {
  void registerDisplayMediaCache();
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <ErrorBoundary>
    <App />
  </ErrorBoundary>
);
