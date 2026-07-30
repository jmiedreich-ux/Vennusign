import type { DisplayLayoutProps } from './DisplayLayout';

export default function ClassicDinerLayout({ content }: DisplayLayoutProps) {
  const sections = content.sections ?? [];

  return (
    <div className="classic-diner">
      <header className="classic-diner__header">
        <p>{content.venueName}</p>
        <h1>{content.menuName}</h1>
      </header>
      {content.dailySpecial?.trim() ? (
        <aside className="classic-diner__special">
          <span>Today's special</span>
          <strong>{content.dailySpecial}</strong>
        </aside>
      ) : null}
      <div className="classic-diner__columns">
        {sections.map((section) => (
          <section className="classic-diner__section" key={section.id}>
            <h2>{section.name}</h2>
            <ul>
              {section.items.map((item) => (
                <li className={!item.isAvailable || item.quantityAvailable === 0 ? 'is-sold-out' : ''} key={item.id}>
                  <div className="classic-diner__item-line">
                    <h3>{item.name}</h3>
                    <span aria-hidden="true" className="classic-diner__leader" />
                    <data value={activePrice(content.isHappyHour, item.price, item.happyHourPrice)}>
                      {formatPrice(activePrice(content.isHappyHour, item.price, item.happyHourPrice))}
                    </data>
                  </div>
                  {item.description && <p>{item.description}</p>}
                  {!item.isAvailable || item.quantityAvailable === 0 ? <span>Sold out</span> : null}
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </div>
  );
}

function activePrice(isHappyHour: boolean | undefined, price: number, happyHourPrice: number | null) {
  return isHappyHour && happyHourPrice !== null ? happyHourPrice : price;
}

function formatPrice(price: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price);
}
