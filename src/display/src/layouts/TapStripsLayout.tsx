import type { CSSProperties } from 'react';
import type { DisplayContent } from '../displayContent.mjs';

const fonts = ['Caveat', 'Kalam', 'Patrick Hand'];

export default function TapStripsLayout({ content }: { content: DisplayContent }) {
  const items = [...(content.tapItems ?? [])].sort((left, right) => left.sortOrder - right.sortOrder);

  return <section className="tap-strips">
    <header><p>{content.venueName}</p><h1>On Tap</h1></header>
    <ol>{items.map((item, index) => <li className={[
      item.isAvailable ? '' : 'unavailable',
      item.isComingSoon ? 'coming-soon' : ''
    ].filter(Boolean).join(' ')} key={item.id} style={{ animationDelay: `${index * 70}ms` } as CSSProperties}>
      <strong className="tap-number">{index + 1}</strong>
      <div>
        <h2 style={{
          '--tap-name-color': item.nameColor ?? '#FFD700',
          fontFamily: `${fonts[index % fonts.length]}, cursive`
        } as CSSProperties}>{item.name}</h2>
        <p>{item.style || 'House selection'}</p>
        <small>{item.abv == null ? null : `${item.abv.toFixed(1)}% ABV`}</small>
      </div>
      <strong className="tap-price">${item.price.toFixed(2)}</strong>
      {!item.isAvailable ? <span className="tap-state">Unavailable</span> : null}
      {item.isComingSoon ? <span className="tap-state">Now brewing</span> : null}
    </li>)}</ol>
    {items.length === 0 ? <p className="tap-strips-empty">The next pour is coming soon.</p> : null}
  </section>;
}
