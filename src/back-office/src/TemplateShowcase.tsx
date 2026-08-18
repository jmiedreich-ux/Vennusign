import { useEffect, useState } from "react";

type Template = {
  id: string;
  label: string;
  caption: string;
  render: () => JSX.Element;
};

const SPECIALS = [
  { name: "Grilled Salmon", note: "Charred lemon, asparagus", price: "$24", hue: "#e8834a" },
  { name: "Wild Mushroom Risotto", note: "Truffle, parmesan crisp", price: "$19", hue: "#8a6a3f" },
  { name: "Short Rib Tacos", note: "Pickled onion, cotija", price: "$16", hue: "#c6482f" }
];

const MENU_COLUMNS = [
  { title: "Starters", rows: [["Burrata & Peach", "$14"], ["Charred Octopus", "$18"], ["Soup du Jour", "$9"]] },
  { title: "Mains", rows: [["Short Rib", "$29"], ["Half Chicken", "$22"], ["Mushroom Pasta", "$21"]] },
  { title: "Desserts", rows: [["Basque Cheesecake", "$11"], ["Olive Oil Cake", "$10"], ["Affogato", "$8"]] }
];

const TEMPLATES: Template[] = [
  {
    id: "daily-specials",
    label: "Daily Specials",
    caption: "Rotate today's specials without touching a printer.",
    render: () => <div className="template-mock template-mock--specials">
      <div className="template-mock__specials-scrim" />
      <span className="template-mock__eyebrow">Today's Specials</span>
      {SPECIALS.map(item => (
        <div className="template-mock__specials-row" key={item.name}>
          <span className="template-mock__thumb" style={{ background: `radial-gradient(circle at 32% 30%, ${item.hue}, #1c0f08 78%)` }} aria-hidden="true" />
          <span className="template-mock__specials-text">
            <span className="template-mock__specials-name">{item.name}</span>
            <span className="template-mock__specials-note">{item.note}</span>
          </span>
          <span className="template-mock__leader" aria-hidden="true" />
          <span className="template-mock__specials-price">{item.price}</span>
        </div>
      ))}
    </div>
  },
  {
    id: "happy-hour",
    label: "Happy Hour",
    caption: "Time-boxed promotions that start and stop on schedule.",
    render: () => <div className="template-mock template-mock--promo">
      <span className="template-mock__glow template-mock__glow--a" aria-hidden="true" />
      <span className="template-mock__glow template-mock__glow--b" aria-hidden="true" />
      <span className="template-mock__badge">4 – 6 PM DAILY</span>
      <strong className="template-mock__headline">Happy Hour</strong>
      <span className="template-mock__sub">Half off drafts &amp; well drinks</span>
      <div className="template-mock__promo-prices">
        <span><b>$5</b> Wells</span>
        <span><b>$4</b> Drafts</span>
        <span><b>$8</b> Small Plates</span>
      </div>
    </div>
  },
  {
    id: "full-menu",
    label: "Full Menu Board",
    caption: "A complete menu, organized into clean columns.",
    render: () => <div className="template-mock template-mock--grid">
      {MENU_COLUMNS.map((section, colIndex) => (
        <div className="template-mock__col" key={section.title}>
          <span className={`template-mock__col-chip template-mock__col-chip--${colIndex}`} aria-hidden="true" />
          <span className="template-mock__col-title">{section.title}</span>
          {section.rows.map(([name, price]) => (
            <div className="template-mock__col-row" key={name}>
              <span>{name}</span>
              <span className="template-mock__leader template-mock__leader--dark" aria-hidden="true" />
              <span>{price}</span>
            </div>
          ))}
        </div>
      ))}
    </div>
  },
  {
    id: "table-menu",
    label: "Table QR Menu",
    caption: "A scannable menu for tables that never needs reprinting.",
    render: () => <div className="template-mock template-mock--qr">
      <div className="template-mock__phone">
        <div className="template-mock__phone-notch" aria-hidden="true" />
        <span className="template-mock__phone-brand">Vennusign</span>
        <div className="template-mock__qr" aria-hidden="true">
          <span className="template-mock__qr-finder template-mock__qr-finder--tl" />
          <span className="template-mock__qr-finder template-mock__qr-finder--tr" />
          <span className="template-mock__qr-finder template-mock__qr-finder--bl" />
          <span className="template-mock__qr-noise" />
        </div>
        <span className="template-mock__phone-cta">View Full Menu</span>
      </div>
      <span className="template-mock__qr-label">Scan the code at your table</span>
    </div>
  }
];

const ROTATE_MS = 4500;

export default function TemplateShowcase() {
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    const timer = window.setInterval(() => setIndex(i => (i + 1) % TEMPLATES.length), ROTATE_MS);
    return () => window.clearInterval(timer);
  }, []);

  const current = TEMPLATES[index];

  return <aside className="template-showcase" aria-label="Examples of screen templates">
    <span className="template-showcase__eyebrow">What you can put on a screen</span>
    <div className="template-showcase__frame">
      <div className="template-showcase__bezel">
        <div className="template-showcase__screen" key={current.id}>{current.render()}</div>
      </div>
    </div>
    <div className="template-showcase__caption">
      <strong>{current.label}</strong>
      <span>{current.caption}</span>
    </div>
    <div className="template-showcase__dots" role="tablist" aria-label="Template examples">
      {TEMPLATES.map((template, i) => (
        <button
          key={template.id}
          type="button"
          role="tab"
          aria-selected={i === index}
          aria-label={template.label}
          className={i === index ? "active" : ""}
          onClick={() => setIndex(i)}
        />
      ))}
    </div>
  </aside>;
}
