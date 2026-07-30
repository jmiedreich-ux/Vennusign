import { useEffect, useState } from 'react';
import type { DisplayContent } from '../displayContent.mjs';

export default function HappyHourBanner({ content }: { content: DisplayContent }) {
  const [, tick] = useState(0);
  useEffect(() => {
    if (!content.isHappyHour || !content.happyHourEndsAtUtc) return;
    const timer = window.setInterval(() => tick(value => value + 1), 1000);
    return () => window.clearInterval(timer);
  }, [content.happyHourEndsAtUtc, content.isHappyHour]);

  if (!content.isHappyHour) return null;
  const remaining = content.happyHourEndsAtUtc
    ? Math.max(0, new Date(content.happyHourEndsAtUtc).getTime() - Date.now())
    : null;
  return <aside className="happy-hour-banner" aria-live="polite">
    <strong>Happy Hour</strong>
    {remaining === null ? <span>Special pricing is active</span> : <span>{formatRemaining(remaining)} remaining</span>}
  </aside>;
}

function formatRemaining(milliseconds: number) {
  const totalSeconds = Math.ceil(milliseconds / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  return hours > 0
    ? `${hours}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
    : `${minutes}:${String(seconds).padStart(2, '0')}`;
}
