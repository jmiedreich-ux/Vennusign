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
              <article className="photo-grid__card" key={item.id}>
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
                    <data value={item.price}>{formatPrice(item.price)}</data>
                  </div>
                  {item.description && <p>{item.description}</p>}
                </div>
              </article>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price);
}
