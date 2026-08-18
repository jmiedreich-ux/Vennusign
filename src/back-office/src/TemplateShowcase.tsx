import { useEffect, useState } from "react";

type Template = {
  id: string;
  label: string;
  caption: string;
  render: () => JSX.Element;
};

const TEMPLATES: Template[] = [
  {
    id: "daily-specials",
    label: "Daily Specials",
    caption: "Rotate today's specials without touching a printer.",
    render: () => <div className="template-mock template-mock--specials">
      <span className="template-mock__eyebrow">Today's Specials</span>
      {[["Grilled Salmon", "$24"], ["Wild Mushroom Risotto", "$19"], ["Short Rib Tacos", "$16"]].map(([name, price]) => (
        <div className="template-mock__row" key={name}><span>{name}</span><span>{price}</span></div>
      ))}
    </div>
  },
  {
    id: "happy-hour",
    label: "Happy Hour",
    caption: "Time-boxed promotions that start and stop on schedule.",
    render: () => <div className="template-mock template-mock--promo">
      <span className="template-mock__badge">4–6 PM</span>
      <strong className="template-mock__headline">Happy Hour</strong>
      <span className="template-mock__sub">Half off drafts &amp; well drinks</span>
    </div>
  },
  {
    id: "full-menu",
    label: "Full Menu Board",
    caption: "A complete menu, organized into clean columns.",
    render: () => <div className="template-mock template-mock--grid">
      {["Starters", "Mains", "Desserts"].map(section => (
        <div className="template-mock__col" key={section}>
          <span className="template-mock__col-title">{section}</span>
          <div className="template-mock__col-line" />
          <div className="template-mock__col-line" />
          <div className="template-mock__col-line" />
        </div>
      ))}
    </div>
  },
  {
    id: "table-menu",
    label: "Table QR Menu",
    caption: "A scannable menu for tables that never needs reprinting.",
    render: () => <div className="template-mock template-mock--qr">
      <div className="template-mock__qr" aria-hidden="true" />
      <span className="template-mock__qr-label">Scan to view menu</span>
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
