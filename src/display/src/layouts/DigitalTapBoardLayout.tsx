import { useEffect, useState } from 'react';
import type { DisplayContent } from '../displayContent.mjs';

function BeerGlass({ color }: { color: string }) {
  return <svg aria-hidden="true" className="digital-beer-glass" viewBox="0 0 72 96">
    <path d="M13 10h38l-4 70a8 8 0 0 1-8 7H25a8 8 0 0 1-8-7z" fill={color} />
    <path d="M13 10h38M18 26h31" fill="none" stroke="currentColor" strokeWidth="5" />
    <path d="M51 30h7a10 10 0 0 1 0 20h-9" fill="none" stroke="currentColor" strokeWidth="5" />
    <path d="M17 18c7-8 13 4 20-3 5-5 10 1 13 3" fill="none" stroke="#fff7df" strokeWidth="7" />
  </svg>;
}

export default function DigitalTapBoardLayout({ content }: { content: DisplayContent }) {
  const ordered = [...(content.tapItems ?? [])].sort((left, right) => left.sortOrder - right.sortOrder);
  const pages = Array.from({ length: Math.ceil(ordered.length / 6) }, (_, index) =>
    ordered.slice(index * 6, index * 6 + 6));
  const pageKey = ordered.map(item => item.id).join('|');
  const [pageIndex, setPageIndex] = useState(0);
  const [reduceMotion, setReduceMotion] = useState(false);
  const items = pages[pageIndex] ?? pages[0] ?? [];

  useEffect(() => {
    const query = window.matchMedia('(prefers-reduced-motion: reduce)');
    const update = () => setReduceMotion(query.matches);
    update();
    query.addEventListener('change', update);
    return () => query.removeEventListener('change', update);
  }, []);

  useEffect(() => {
    setPageIndex(current => current < pages.length ? current : 0);
  }, [pageKey, pages.length]);

  useEffect(() => {
    if (reduceMotion || pages.length < 2) return;
    const timer = window.setInterval(() =>
      setPageIndex(current => (current + 1) % pages.length), 10000);
    return () => window.clearInterval(timer);
  }, [pageKey, pages.length, reduceMotion]);

  return <section className="digital-tap-board" data-page-count={pages.length}>
    <header><div><p>{content.venueName}</p><h1>Tap List</h1></div><strong>{ordered.length} pours</strong></header>
    <ol>{items.map((item, index) => <li className={[
      item.isAvailable ? '' : 'unavailable',
      item.isComingSoon ? 'coming-soon' : ''
    ].filter(Boolean).join(' ')} key={item.id}>
      <span className="digital-tap-number">#{pageIndex * 6 + index + 1}</span>
      <BeerGlass color={item.glassColor ?? '#F5C842'} />
      <div className="digital-tap-copy">
        <h2>{item.name}</h2>
        <p>{item.style || 'House selection'}</p>
        <small>{[
          item.abv == null ? null : `${item.abv.toFixed(1)}% ABV`,
          item.ibu == null ? null : `${item.ibu} IBU`
        ].filter(Boolean).join(' · ')}</small>
      </div>
      <strong className="digital-tap-price">${item.price.toFixed(2)}</strong>
      {item.isComingSoon ? <span className="digital-brewing-state">Now Brewing</span> : null}
    </li>)}</ol>
    {pages.length > 1 ? <nav aria-label="Tap pages">{pages.map((_, index) =>
      <span aria-current={index === pageIndex ? 'page' : undefined} key={index} />
    )}</nav> : null}
    {items.length === 0 ? <p className="digital-tap-empty">Fresh taps are coming soon.</p> : null}
  </section>;
}
