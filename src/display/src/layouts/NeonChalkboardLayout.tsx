import type { CSSProperties } from 'react';
import type { DisplayMenuItem } from '../displayContent.mjs';
import type { DisplayLayoutProps } from './DisplayLayout';

export default function NeonChalkboardLayout({ content }: DisplayLayoutProps) {
  const sections = content.sections ?? [];

  return (
    <div className="neon-chalkboard">
      <div aria-hidden="true" className="neon-chalkboard__frame" />
      <header className="neon-chalkboard__header">
        <p>{content.venueName}</p>
        <h1>{content.menuName}</h1>
      </header>
      <div className="neon-chalkboard__columns">
        {sections.map((section, index) => (
          <section
            className="neon-chalkboard__section"
            key={section.id}
            style={{
              '--neon-section-color': `var(--vennu-section-color-${index % 4 + 1})`,
              '--neon-section-index': index
            } as CSSProperties}
          >
            <h2>{section.name}</h2>
            <ul>
              {section.items.map((item) => (
                <li className={!item.isAvailable || item.quantityAvailable === 0 ? 'is-sold-out' : ''} key={item.id}>
                  <div>
                    <h3>{item.name}</h3>
                    <span aria-hidden="true" />
                    <data value={activePrice(content.isHappyHour, item)}>
                      {formatPrice(activePrice(content.isHappyHour, item))}
                    </data>
                  </div>
                  {item.description ? <p>{item.description}</p> : null}
                  {!item.isAvailable || item.quantityAvailable === 0 ? <strong>Sold out</strong> : null}
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </div>
  );
}

function activePrice(isHappyHour: boolean | undefined, item: DisplayMenuItem) {
  return isHappyHour && item.happyHourPrice !== null ? item.happyHourPrice : item.price;
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price);
}
