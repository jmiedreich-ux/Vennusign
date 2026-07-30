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
  const items = ordered.slice(0, 6);

  return <section className="digital-tap-board" data-overflow-count={Math.max(0, ordered.length - items.length)}>
    <header><div><p>{content.venueName}</p><h1>Tap List</h1></div><strong>{items.length} pours</strong></header>
    <ol>{items.map((item, index) => <li className={item.isAvailable ? '' : 'unavailable'} key={item.id}>
      <span className="digital-tap-number">#{index + 1}</span>
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
    </li>)}</ol>
    {items.length === 0 ? <p className="digital-tap-empty">Fresh taps are coming soon.</p> : null}
  </section>;
}
