import type { DisplayLayoutProps } from './DisplayLayout';

export default function PhotoGridLayout({ content }: DisplayLayoutProps) {
  const sections = content.sections ?? [];

  return (
    <div className="photo-grid">
      <header className="photo-grid__header">
        <p className="photo-grid__venue">{content.venueName}</p>
        <h1>{content.menuName}</h1>
      </header>
      {sections.map((section) => (
        <section className="photo-grid__section" key={section.id}>
          <h2>{section.name}</h2>
          <div className="photo-grid__cards">
            {section.items.map((item) => (
              <article
                aria-label={isSoldOut(item) ? `${item.name}, sold out` : item.name}
                className={`photo-grid__card${isSoldOut(item) ? ' photo-grid__card--sold-out' : ''}`}
                key={item.id}
              >
                {item.isPopular && <span className="photo-grid__popular">★ Popular</span>}
                <div className="photo-grid__media">
                  {item.imageUrl ? (
                    <img alt="" loading="lazy" src={item.imageUrl} />
                  ) : (
                    <div aria-hidden="true" className="photo-grid__placeholder" />
                  )}
                </div>
                <div className="photo-grid__copy">
                  <div className="photo-grid__title-row">
                    <h3>{item.name}</h3>
                    {content.isHappyHour && item.happyHourPrice !== null ? (
                      <span className="photo-grid__happy-hour-price">
                        <data value={item.happyHourPrice}>{formatPrice(item.happyHourPrice)}</data>
                        <s>{formatPrice(item.price)}</s>
                      </span>
                    ) : (
                      <data value={item.price}>{formatPrice(item.price)}</data>
                    )}
                  </div>
                  {item.description && <p>{item.description}</p>}
                  <div className="photo-grid__badges">
                    {item.tags.map((tag) => (
                      <span className="photo-grid__badge" key={tag}>{tag}</span>
                    ))}
                    {isLimited(item) && (
                      <span className="photo-grid__badge photo-grid__badge--quantity">
                        Only {item.quantityAvailable} left
                      </span>
                    )}
                  </div>
                </div>
                {isSoldOut(item) && <div className="photo-grid__sold-out">Sold out</div>}
              </article>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

type PhotoGridItem = NonNullable<DisplayLayoutProps['content']['sections']>[number]['items'][number];

function isSoldOut(item: PhotoGridItem) {
  return !item.isAvailable || item.quantityAvailable === 0;
}

function isLimited(item: PhotoGridItem) {
  return item.isAvailable && item.quantityAvailable !== null && item.quantityAvailable > 0;
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price);
}
