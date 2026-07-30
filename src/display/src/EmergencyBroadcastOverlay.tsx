import { useEffect, useState, type ReactNode } from 'react';
import type { DisplayContent } from './displayContent.mjs';
import './emergencyBroadcast.css';

export default function EmergencyBroadcastOverlay({ content, children }: { content: DisplayContent; children: ReactNode }) {
  const broadcast = content.emergencyBroadcast;
  const [expired, setExpired] = useState(false);
  useEffect(() => {
    setExpired(false);
    if (!broadcast) return;
    const remaining = Date.parse(broadcast.expiresUtc) - Date.now();
    if (remaining <= 0) { setExpired(true); return; }
    const timer = window.setTimeout(() => setExpired(true), remaining);
    return () => window.clearTimeout(timer);
  }, [broadcast?.id, broadcast?.expiresUtc]);

  if (!broadcast || expired || Date.parse(broadcast.expiresUtc) <= Date.now()) return children;
  return <main className="emergency-broadcast" role="alert" aria-live="assertive">
    {broadcast.mediaUrl ? <img alt="" src={broadcast.mediaUrl} /> : null}
    <section><p>Emergency notice</p><h1>{broadcast.title}</h1><div>{broadcast.message}</div></section>
  </main>;
}
