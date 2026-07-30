import type { DisplayContent } from '../displayContent.mjs';

export default function ClassicChalkboardLayout({ content }: { content: DisplayContent }) {
  const items = content.tapItems ?? [];
  const categories = (content.tapCategories ?? []).filter(category => category.isActive);
  const sections = [
    ...categories.map(category => ({
      id: category.id,
      name: category.name,
      price: category.categoryPrice,
      items: items.filter(item => item.tapCategoryId === category.id)
    })),
    {
      id: 'uncategorized',
      name: categories.length ? 'On Tap' : 'Drinks',
      price: null,
      items: items.filter(item => !item.tapCategoryId)
    }
  ].filter(section => section.items.length > 0);

  return <section className="classic-chalkboard">
    <header><p>{content.venueName}</p><h1>Drinks</h1></header>
    <div className="chalkboard-categories">{sections.map(section => <article key={section.id}>
      <div className="chalkboard-category-heading">
        <h2>{section.name}</h2>
        {section.price != null ? <strong>${section.price.toFixed(2)}</strong> : null}
      </div>
      <ul>{section.items.map(item => <li className={item.isAvailable ? '' : 'unavailable'} key={item.id}>
        <span>{item.name}</span>
        {section.price == null ? <small>${item.price.toFixed(2)}</small> : null}
      </li>)}</ul>
    </article>)}</div>
    {sections.length === 0 ? <p className="chalkboard-empty">Tap list coming soon.</p> : null}
  </section>;
}
