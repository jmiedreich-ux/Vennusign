import type { DisplayLayoutProps } from './DisplayLayout';

export default function ClassicDinerLayout({ content }: DisplayLayoutProps) {
  const sections = content.sections ?? [];

  return (
    <div className="classic-diner">
      <header className="classic-diner__header">
        <p>{content.venueName}</p>
        <h1>{content.menuName}</h1>
      </header>
      <div className="classic-diner__columns">
        {sections.map((section) => (
          <section className="classic-diner__section" key={section.id}>
            <h2>{section.name}</h2>
            <ul>
              {section.items.map((item) => (
                <li className={!item.isAvailable || item.quantityAvailable === 0 ? 'is-sold-out' : ''} key={item.id}>
                  <h3>{item.name}</h3>
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
