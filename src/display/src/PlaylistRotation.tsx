import { useEffect, useState, type ReactNode } from 'react';
import type { DisplayContent } from './displayContent.mjs';
import './playlist.css';

export default function PlaylistRotation({ content, children }: { content: DisplayContent; children: ReactNode }) {
  const slides = content.playlist ?? [];
  const key = slides.map(slide => `${slide.id}:${slide.dwellSeconds}`).join('|');
  const [activeId, setActiveId] = useState(slides[0]?.id);
  const active = slides.find(slide => slide.id === activeId) ?? slides[0];

  useEffect(() => setActiveId(current => slides.some(slide => slide.id === current) ? current : slides[0]?.id), [key]);
  useEffect(() => {
    if (slides.length < 2 || !active) return;
    const timer = window.setTimeout(() => {
      const index = slides.findIndex(slide => slide.id === active.id);
      setActiveId(slides[(index + 1) % slides.length].id);
    }, active.dwellSeconds * 1000);
    return () => window.clearTimeout(timer);
  }, [active?.id, key]);

  if (!active || active.slideType === 'menu') return children;
  return <main className="playlist-slide" data-slide-id={active.id}>
    {active.slideType === 'image' && active.mediaUrl ? <img alt={active.title ?? ''} src={active.mediaUrl} /> : null}
    {active.slideType === 'message' ? <section><h1>{active.title}</h1>{active.body ? <p>{active.body}</p> : null}</section> : null}
  </main>;
}
