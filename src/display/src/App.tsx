import DisplayPage from './DisplayPage';
import PairingPage from './PairingPage';
import { resolveDisplayRoute } from './routing';

export default function App() {
  const route = resolveDisplayRoute(window.location.pathname);

  if (route.kind === 'pair') {
    return <PairingPage />;
  }

  if (route.kind === 'not-found') {
    return (
      <main>
        <h1>Display not found</h1>
        <p>Use a display URL with a screen identifier.</p>
      </main>
    );
  }

  return <DisplayPage screenId={route.screenId} />;
}
