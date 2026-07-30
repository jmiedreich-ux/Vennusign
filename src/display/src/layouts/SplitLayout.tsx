import type { DisplayMenuItem } from '../displayContent.mjs';
import type { DisplayLayoutProps } from './DisplayLayout';

export default function SplitLayout({ content }: DisplayLayoutProps) {
  const sections = content.sections ?? [];
  const items = sections.flatMap(section => section.items);
  const hero = selectHero(items);

  return (
    <div className="split-layout" data-ratio={content.splitRatio ?? '40_60'}>
      <section className="split-layout__hero">
        {hero?.imageUrl ? <img alt={hero.name} src={hero.imageUrl} /> : <div aria-hidden="true" className="split-layout__hero-fallback" />}
        <div>
          <span>{content.venueName}</span>
          <h1>{hero?.name ?? content.menuName}</h1>
          {hero?.description ? <p>{hero.description}</p> : null}
          {hero ? <data value={activePrice(content.isHappyHour, hero)}>{formatPrice(activePrice(content.isHappyHour, hero))}</data> : null}
        </div>
      </section>
      <section className="split-layout__menu">
        <header><p>{content.venueName}</p><h2>{content.menuName}</h2></header>
        <div className="split-layout__sections">
          {sections.map(section => (
            <section key={section.id}>
              <h3>{section.name}</h3>
              <ul>{section.items.map(item => (
                <li className={!item.isAvailable || item.quantityAvailable === 0 ? 'is-sold-out' : ''} key={item.id}>
                  <div><strong>{item.name}</strong><span aria-hidden="true" /><data value={activePrice(content.isHappyHour, item)}>{formatPrice(activePrice(content.isHappyHour, item))}</data></div>
                  {item.description ? <p>{item.description}</p> : null}
                  {item.tags.length ? <div className="split-layout__tags">{item.tags.map(tag =>
                    <span key={tag}>{tag}</span>)}</div> : null}
                </li>
              ))}</ul>
            </section>
          ))}
        </div>
      </section>
    </div>
  );
}

export function selectHero(items: DisplayMenuItem[]) {
  const available = (item: DisplayMenuItem) => item.isAvailable && item.quantityAvailable !== 0;
  return items.find(item => available(item) && item.isPopular && Boolean(item.imageUrl))
    ?? items.find(item => available(item) && Boolean(item.imageUrl))
    ?? items.find(available);
}

function activePrice(isHappyHour: boolean | undefined, item: DisplayMenuItem) {
  return isHappyHour && item.happyHourPrice !== null ? item.happyHourPrice : item.price;
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
}
