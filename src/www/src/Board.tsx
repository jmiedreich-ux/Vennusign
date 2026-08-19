import { useEffect, useState } from "react";
import { boardTagLabel, type BoardItem, type BoardPeriod } from "./boardExamples";

// The real Photo Grid layout shows a shimmer-loading placeholder when an item has no
// photo yet - this reuses that same idea (a neutral shimmer, not a fake "photo") with a
// thin accent stripe cycling through a small curated palette, instead of a raw per-item
// hue rotation.
const PHOTO_ACCENT_PALETTE = ["#d9a15b", "#8fae7c", "#c97b63", "#8ba7c9", "#c9a4d4", "#7ec9be"];

function ItemTag({ item }: { item: BoardItem }) {
  if (!item.tag) return null;
  return <em className={`board-tag board-tag--${item.tag}`}>{boardTagLabel[item.tag]}</em>;
}

function HappyHourBanner({ endsLabel }: { endsLabel: string }) {
  const [secondsLeft, setSecondsLeft] = useState(9 * 60 + 42);
  useEffect(() => {
    const id = setInterval(() => setSecondsLeft(s => (s <= 0 ? 12 * 60 : s - 1)), 1000);
    return () => clearInterval(id);
  }, []);
  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;
  return <div className="happy-hour-banner">
    <strong>Happy hour</strong>
    <span>{minutes}:{seconds.toString().padStart(2, "0")} left · {endsLabel}</span>
  </div>;
}

export function Board({ venueName, period, compact, walled }: { venueName: string; period: BoardPeriod; compact?: boolean; walled?: boolean }) {
  const [featuredIndex, setFeaturedIndex] = useState(0);
  const isHero = period.style === "daily-special-hero";
  useEffect(() => {
    if (!isHero) return;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduceMotion) return;
    const id = setInterval(() => setFeaturedIndex(i => (i + 1) % period.items.length), 3200);
    return () => clearInterval(id);
  }, [isHero, period.items.length]);

  const glowStyle = period.glow ? ({ "--board-glow": period.glow } as React.CSSProperties) : undefined;

  return <div className={`board board--${period.style}${compact ? " board--compact" : ""}${walled ? " board--walled" : ""}`} style={glowStyle} aria-live="polite">
    {period.happyHourEndsLabel ? <HappyHourBanner endsLabel={period.happyHourEndsLabel} /> : null}
    <div className="board__header">
      <span>{venueName}</span>
      {period.happyHourEndsLabel ? null : <strong>{period.label}</strong>}
    </div>

    {period.style === "daily-special-hero" ? (() => {
      const featured = period.items[featuredIndex];
      const secondary = period.items.filter((_, i) => i !== featuredIndex);
      return <>
        {period.photo ? <img className="board__hero-media" src={period.photo} alt="" aria-hidden="true" /> : null}
        <div className="board__hero-scrim" aria-hidden="true" />
        <div className="board__hero">
        <em className="board__hero-pill">Featured now</em>
        <p className="board__hero-headline">{period.headline}</p>
        <div className="board__hero-featured">
          <span>{featured.name}</span>
          <data value={featured.price}>{featured.price}</data>
        </div>
        <div className="board__hero-secondary">
          {secondary.map(item => <div key={item.name}>
            <span>{item.name}</span><data value={item.price}>{item.price}</data>
          </div>)}
        </div>
        </div>
      </>;
    })() : period.style === "photo-grid" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__photo-grid">
        {period.items.map((item, index) => <div key={item.name} className={`board__photo-card${item.tag === "sold-out" ? " board__photo-card--sold-out" : ""}`}>
          {item.photo
            ? <img className="board__photo-image" src={item.photo} alt={item.name} loading="lazy" />
            : <div className="board__photo-swatch" style={{ "--photo-accent": PHOTO_ACCENT_PALETTE[index % PHOTO_ACCENT_PALETTE.length] } as React.CSSProperties} />}
          <div className="board__photo-copy">
            <span>{item.name}</span>
            <data value={item.price}>{item.price}</data>
          </div>
          <ItemTag item={item} />
        </div>)}
      </div>
    </> : period.style === "movie-poster-board" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__poster-row">
        {period.items.map(item => <div key={item.name} className="board__poster-card">
          {item.photo ? <img className="board__poster-art" src={item.photo} alt={item.name} loading="lazy" /> : null}
          <strong>{item.name}</strong>
          {item.detail ? <small>{item.detail}</small> : null}
          {item.times ? <div className="board__poster-times">
            {item.times.map(time => <span key={time}>{time}</span>)}
          </div> : null}
          <ItemTag item={item} />
        </div>)}
      </div>
    </> : period.style === "flight-board" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__flight-rows">
        <div className="board__flight-row board__flight-row--head" aria-hidden="true">
          <span>Time</span><span>Destination</span><span>Flight</span><span>Gate</span><span>Status</span>
        </div>
        {period.items.map(item => <div key={item.name} className={`board__flight-row board__flight-row--${item.status ?? "on-time"}`}>
          <data value={item.timeLabel}>{item.timeLabel}</data>
          <span className="board__flight-city">{item.name}</span>
          <span>{item.detail}</span>
          <span>{item.price}</span>
          <em>{item.status === "on-time" ? "On time" : item.status === "boarding" ? "Boarding" : item.status === "delayed" ? "Delayed" : "Landed"}</em>
        </div>)}
      </div>
    </> : period.style === "promo-splash" ? (() => {
      const lead = period.items[0];
      const rest = period.items.slice(1);
      return <div className="board__promo">
        <p className="board__promo-headline">{period.headline}</p>
        <div className="board__promo-burst">
          <span>{lead.name}</span>
          <data value={lead.price}>{lead.price}</data>
        </div>
        <div className="board__promo-deals">
          {rest.map(item => <div key={item.name}>
            <span>{item.name}</span><data value={item.price}>{item.price}</data>
          </div>)}
        </div>
      </div>;
    })() : period.style === "photo-tile-board" ? <>
      {walled ? null : <p className="board__headline">{period.headline}</p>}
      <div className="board__tile-grid">
        {period.items.map(item => <div key={item.name} className="board__tile">
          {item.photo ? <div className="board__tile-photo">
            <img src={item.photo} alt={item.name} loading="lazy" />
            <data value={item.price}>{item.price}</data>
          </div> : null}
          <span className="board__tile-name">{item.name}</span>
          {item.detail ? <small>{item.detail}</small> : null}
          <ItemTag item={item} />
        </div>)}
      </div>
    </> : period.style === "letterboard-special" ? <>
      <p className="board__headline">{period.headline}</p>
      <small className="board__letter-sub">Served daily · {period.time}</small>
      <ol className="board__letter-list">
        {period.items.map(item => <li key={item.name}>
          <span>{item.name}</span>
          <data value={item.price}>{item.price}</data>
          <ItemTag item={item} />
        </li>)}
      </ol>
    </> : period.style === "classic-chalkboard" ? <>
      <p className="board__headline">{period.headline}</p>
      {period.categories ? <div className="board__chalk-categories">
        {period.categories.map(category => <div key={category.name} className="board__chalk-category">
          <div className="board__chalk-category-heading">
            <h3>{category.name}</h3>
            {category.price ? <strong>{category.price}</strong> : null}
          </div>
          <ul className="board__list">
            {category.items.map(item => <li key={item.name} className={item.tag === "sold-out" ? "board__item--sold-out" : undefined}>
              <span>{item.name}</span>
              {category.price ? null : <data value={item.price} className="board__item-price">{item.price}</data>}
              <ItemTag item={item} />
            </li>)}
          </ul>
        </div>)}
      </div> : <ul className="board__list">
        {period.items.map(item => <li key={item.name} className={item.tag === "sold-out" ? "board__item--sold-out" : undefined}>
          <span>{item.name}</span>
          <data value={item.price} className="board__item-price">{item.price}</data>
          <ItemTag item={item} />
        </li>)}
      </ul>}
    </> : period.style === "digital-tap-board" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__tap-grid">
        {period.items.map(item => <div key={item.name} className={`board__tap-card${item.tag === "sold-out" ? " board__tap-card--sold-out" : ""}`}>
          <svg width="22" height="26" viewBox="0 0 22 26" aria-hidden="true"><path d="M3 2h16l-2 20a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2L3 2Z" fill="none" stroke="currentColor" strokeWidth="1.6" /><path d="M3 8h16" stroke="currentColor" strokeWidth="1.2" opacity=".5" /></svg>
          <div className="board__tap-copy">
            <span>{item.name}</span>
            {item.detail ? <small>{item.detail}</small> : null}
          </div>
          <data value={item.price}>{item.price}</data>
          {item.tag === "new" ? <em className="board__tap-brewing">Now brewing</em> : <ItemTag item={item} />}
        </div>)}
      </div>
    </> : <>
      <p className="board__headline">{period.headline}</p>
      <ul className="board__list">
        {period.items.map(item => <li key={item.name} className={item.tag === "sold-out" ? "board__item--sold-out" : undefined}>
          <span>{item.name}</span>
          <data value={item.price} className="board__item-price">{item.price}</data>
          <ItemTag item={item} />
        </li>)}
      </ul>
    </>}
    <small className="board__footnote">Preview only · no venue data is changed</small>
  </div>;
}

// Many real venues - the reference case is Chinese takeout counters - don't run one
// screen, they run two or three mounted side by side, each showing something different.
// This proves that arrangement instead of describing it: a shared physical frame around
// several independent boards, exactly like the photos of real backlit menu walls.
export function ScreenWall({ periods }: { periods: { venue: { venueName: string }; period: BoardPeriod }[] }) {
  return <div className="board-wall">
    {periods.map(entry => <Board key={entry.period.id} venueName={entry.venue.venueName} period={entry.period} compact walled />)}
  </div>;
}
