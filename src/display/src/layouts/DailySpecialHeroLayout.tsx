import type { DisplayMenuItem } from '../displayContent.mjs';
import type { DisplayLayoutProps } from './DisplayLayout';

export default function DailySpecialHeroLayout({ content }: DisplayLayoutProps) {
  const items = (content.sections ?? []).flatMap(section => section.items);
  const featured = selectFeaturedItem(content.dailySpecial, items);
  const secondary = selectSecondaryItems(items, featured?.id);

  return (
    <div className="daily-special-hero">
      <div className="daily-special-hero__media">
        {featured?.imageUrl
          ? <img alt="" src={featured.imageUrl} />
          : <div aria-hidden="true" className="daily-special-hero__fallback" />}
      </div>
      <div className="daily-special-hero__scrim" />
      <header>
        <p>{content.venueName}</p>
        <strong>{content.menuName}</strong>
      </header>
      <section className="daily-special-hero__featured">
        <span className="daily-special-hero__badge">Today Only</span>
        <h1>{featured?.name ?? content.dailySpecial ?? 'Today’s special'}</h1>
        {featured?.description ? <p>{featured.description}</p> : null}
        {featured ? <data value={activePrice(content.isHappyHour, featured)}>
          {formatPrice(activePrice(content.isHappyHour, featured))}
        </data> : null}
      </section>
      {secondary.length ? <aside className="daily-special-hero__secondary" aria-label="More featured items">
        {secondary.map(item => <article key={item.id}>
          <div>
            <strong>{item.name}</strong>
            {item.description ? <p>{item.description}</p> : null}
          </div>
          <data value={activePrice(content.isHappyHour, item)}>
            {formatPrice(activePrice(content.isHappyHour, item))}
          </data>
        </article>)}
      </aside> : null}
    </div>
  );
}

export function selectFeaturedItem(dailySpecial: string | null | undefined, items: DisplayMenuItem[]) {
  const available = items.filter(isAvailable);
  const requestedName = dailySpecial?.trim().toLocaleLowerCase();

  return (requestedName
    ? available.find(item => item.name.trim().toLocaleLowerCase() === requestedName)
    : undefined)
    ?? available.find(item => item.isPopular && Boolean(item.imageUrl))
    ?? available.find(item => Boolean(item.imageUrl))
    ?? available[0];
}

export function selectSecondaryItems(items: DisplayMenuItem[], featuredId: string | undefined) {
  return items.filter(item => isAvailable(item) && item.id !== featuredId).slice(0, 3);
}

function isAvailable(item: DisplayMenuItem) {
  return item.isAvailable && item.quantityAvailable !== 0;
}

function activePrice(isHappyHour: boolean | undefined, item: DisplayMenuItem) {
  return isHappyHour && item.happyHourPrice !== null ? item.happyHourPrice : item.price;
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
}
