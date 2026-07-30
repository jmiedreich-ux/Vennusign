import { useEffect, useState } from 'react';
import type { DisplayMenuItem } from '../displayContent.mjs';
import type { DisplayLayoutProps } from './DisplayLayout';

export default function DailySpecialHeroLayout({ content }: DisplayLayoutProps) {
  const items = (content.sections ?? []).flatMap(section => section.items);
  const featured = selectFeaturedItem(content.dailySpecial, items);
  const rotationItems = featured
    ? [featured, ...selectSecondaryItems(items, featured.id)]
    : [];
  const rotationKey = rotationItems.map(item => item.id).join('|');
  const [activeId, setActiveId] = useState(featured?.id);
  const [reduceMotion, setReduceMotion] = useState(false);
  const active = rotationItems.find(item => item.id === activeId) ?? featured;
  const secondary = rotationItems.filter(item => item.id !== active?.id).slice(0, 3);
  const dwellMilliseconds = (content.heroDwellSeconds ?? 8) * 1000;

  useEffect(() => {
    const query = window.matchMedia('(prefers-reduced-motion: reduce)');
    const update = () => setReduceMotion(query.matches);
    update();
    query.addEventListener('change', update);
    return () => query.removeEventListener('change', update);
  }, []);

  useEffect(() => {
    setActiveId(current => rotationItems.some(item => item.id === current)
      ? current
      : featured?.id);
  }, [featured?.id, rotationKey]);

  useEffect(() => {
    if (reduceMotion || rotationItems.length < 2) {
      return;
    }
    const timer = window.setInterval(() => {
      setActiveId(current => {
        const index = rotationItems.findIndex(item => item.id === current);
        return rotationItems[(index + 1) % rotationItems.length].id;
      });
    }, dwellMilliseconds);
    return () => window.clearInterval(timer);
  }, [dwellMilliseconds, reduceMotion, rotationKey]);

  return (
    <div className="daily-special-hero">
      <div className="daily-special-hero__media">
        {active?.imageUrl
          ? <img alt="" key={active.id} src={active.imageUrl} />
          : <div aria-hidden="true" className="daily-special-hero__fallback" />}
      </div>
      <div className="daily-special-hero__scrim" />
      <header>
        <p>{content.venueName}</p>
        <strong>{content.menuName}</strong>
      </header>
      <section className="daily-special-hero__featured">
        <span className="daily-special-hero__badge">Today Only</span>
        <h1>{active?.name ?? content.dailySpecial ?? 'Today’s special'}</h1>
        {active?.description ? <p>{active.description}</p> : null}
        {active ? <data value={activePrice(content.isHappyHour, active)}>
          {formatPrice(activePrice(content.isHappyHour, active))}
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
