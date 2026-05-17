import { useState, useEffect, useRef } from "react";

/* ── Fonts & Global Styles ─────────────────────────────────────────────── */
const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Pacifico&family=Caveat:wght@400;500;600;700&family=Permanent+Marker&display=swap');
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  body { background: #000; overflow: hidden; }

  /* ── Neon flicker on venue title ── */
  @keyframes flicker {
    0%,18%,22%,25%,53%,57%,100% { opacity: 1; }
    20%,24%,55% { opacity: 0.85; }
  }
  @keyframes hh-pulse {
    0%,100% { opacity: 1; transform: scale(1); }
    50%  { opacity: 0.88; transform: scale(1.015); }
  }
  @keyframes scanline {
    0%   { transform: translateY(-100%); }
    100% { transform: translateY(100vh); }
  }
  @keyframes chalk-draw {
    from { clip-path: inset(0 100% 0 0); }
    to   { clip-path: inset(0 0% 0 0); }
  }
  @keyframes float {
    0%,100% { transform: translateY(0px); }
    50%     { transform: translateY(-5px); }
  }
  @keyframes glow-breathe {
    0%,100% { filter: brightness(1); }
    50%     { filter: brightness(1.25); }
  }
  .venue-title   { animation: flicker 4s infinite, glow-breathe 3s ease-in-out infinite; }
  .hh-badge      { animation: hh-pulse 2s ease-in-out infinite; }
  .item-row      { animation: chalk-draw 0.5s ease forwards; }
  .price-tag     { animation: glow-breathe 2.5s ease-in-out infinite; }
  .deco-float    { animation: float 4s ease-in-out infinite; }

  /* scanline overlay */
  .scanline::after {
    content: '';
    position: absolute;
    inset: 0;
    background: repeating-linear-gradient(
      to bottom,
      transparent 0px,
      transparent 3px,
      rgba(0,0,0,0.08) 3px,
      rgba(0,0,0,0.08) 4px
    );
    pointer-events: none;
    z-index: 10;
  }
`;

/* ── Neon color themes ─────────────────────────────────────────────────── */
const THEMES = {
  "Bar Classic": {
    bg: "#080f08",
    board: "#0b160b",
    title: "#ffd700",
    titleGlow: "#ff8c00",
    sectionColors: ["#00ffff", "#ff6ec7", "#a8ff3e"],
    priceColor: "#ffffff",
    priceGlow: "#00ffff",
    divider: "#00ff8820",
    hhColor: "#ff6ec7",
    hhGlow: "#ff00aa",
    tagColor: "#ffd700",
  },
  "Violet Lounge": {
    bg: "#09060f",
    board: "#0f0a1a",
    title: "#bf5fff",
    titleGlow: "#7700ff",
    sectionColors: ["#bf5fff", "#00cfff", "#ff6ec7"],
    priceColor: "#ffffff",
    priceGlow: "#bf5fff",
    divider: "#bf5fff20",
    hhColor: "#00cfff",
    hhGlow: "#00aaff",
    tagColor: "#ffd700",
  },
  "Hot Summer": {
    bg: "#0f0700",
    board: "#160b00",
    title: "#ff4500",
    titleGlow: "#ff2200",
    sectionColors: ["#ff6348", "#ffd700", "#ff9f43"],
    priceColor: "#ffffff",
    priceGlow: "#ffd700",
    divider: "#ff450020",
    hhColor: "#ffd700",
    hhGlow: "#ffaa00",
    tagColor: "#ff6348",
  },
  "Ocean Dive": {
    bg: "#00080f",
    board: "#000d18",
    title: "#00cfff",
    titleGlow: "#0077ff",
    sectionColors: ["#00cfff", "#7df9ff", "#00ff88"],
    priceColor: "#ffffff",
    priceGlow: "#00cfff",
    divider: "#00cfff20",
    hhColor: "#00ff88",
    hhGlow: "#00cc66",
    tagColor: "#7df9ff",
  },
};

/* ── Data ──────────────────────────────────────────────────────────────── */
const MENU = [
  {
    id: "s1", title: "Craft Beers", emoji: "🍺",
    items: [
      { id: "i1", name: "Hazy IPA",    desc: "tropical & citrusy",     price: 8.00, hhPrice: 5.00 },
      { id: "i2", name: "Dark Porter", desc: "rich chocolate notes",    price: 7.00, hhPrice: 4.50 },
      { id: "i3", name: "Wheat Ale",   desc: "light & refreshing",      price: 6.50, hhPrice: 4.00 },
      { id: "i4", name: "Pilsner",     desc: "crisp czech-style",       price: 6.00, hhPrice: 4.00 },
    ]
  },
  {
    id: "s2", title: "Cocktails", emoji: "🍸",
    items: [
      { id: "i5", name: "Old Fashioned",  desc: "bourbon · bitters · orange", price: 13.00, hhPrice: 9.00 },
      { id: "i6", name: "Aperol Spritz",  desc: "aperol · prosecco · soda",   price: 12.00, hhPrice: 8.00 },
      { id: "i7", name: "Mezcal Negroni", desc: "smoky · sweet · complex",    price: 14.00, hhPrice: 10.00 },
      { id: "i8", name: "Paloma",         desc: "tequila · grapefruit · lime", price: 12.00, hhPrice: 8.00 },
    ]
  },
  {
    id: "s3", title: "Bar Bites", emoji: "🍟",
    items: [
      { id: "i9",  name: "Truffle Fries",  desc: "parmesan & herbs",       price: 9.00,  hhPrice: 6.00 },
      { id: "i10", name: "Chicken Wings",  desc: "3 sauce options",         price: 14.00, hhPrice: 10.00 },
      { id: "i11", name: "Nachos",         desc: "queso · jalapeños",       price: 12.00, hhPrice: 8.00 },
    ]
  },
];

/* ── Neon text-shadow helpers ──────────────────────────────────────────── */
const neon = (color, intensity = 1) => ({
  color: "#fff",
  textShadow: [
    `0 0 ${4 * intensity}px #fff`,
    `0 0 ${11 * intensity}px #fff`,
    `0 0 ${19 * intensity}px #fff`,
    `0 0 ${40 * intensity}px ${color}`,
    `0 0 ${80 * intensity}px ${color}`,
    `0 0 ${90 * intensity}px ${color}`,
    `0 0 ${100 * intensity}px ${color}`,
    `0 0 ${150 * intensity}px ${color}`,
  ].join(", ")
});

const softNeon = (color) => ({
  color,
  textShadow: `0 0 6px ${color}cc, 0 0 18px ${color}88, 0 0 40px ${color}55`
});

const edgeGlow = (color) => ({
  boxShadow: `0 0 8px ${color}88, 0 0 20px ${color}44, inset 0 0 8px ${color}11`
});

const fmt = (n) => `$${Number(n).toFixed(2)}`;

/* ── Chalk noise texture as inline SVG data URI ─────────────────────────── */
const CHALK_TEXTURE = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3CfeColorMatrix type='saturate' values='0'/%3E%3C/filter%3E%3Crect width='300' height='300' filter='url(%23noise)' opacity='0.04'/%3E%3C/svg%3E")`;

/* ── Decorative neon divider line ─────────────────────────────────────── */
function NeonDivider({ color }) {
  return (
    <div style={{ margin: "10px 0 14px", position: "relative", height: 2 }}>
      <div style={{ height: 1, background: color, opacity: 0.6,
        boxShadow: `0 0 6px ${color}, 0 0 14px ${color}88, 0 0 30px ${color}44` }} />
    </div>
  );
}

/* ── Section column ───────────────────────────────────────────────────── */
function Section({ section, color, isHH, delay }) {
  const [visible, setVisible] = useState(false);
  useEffect(() => { const t = setTimeout(() => setVisible(true), delay); return () => clearTimeout(t); }, []);

  return (
    <div style={{ flex: 1, opacity: visible ? 1 : 0, transition: "opacity 0.5s ease",
      padding: "0 20px", borderRight: "1px solid #ffffff08" }}>
      {/* Section header */}
      <div style={{ textAlign: "center", marginBottom: 4 }}>
        <div style={{ fontFamily: "'Permanent Marker', cursive", fontSize: "clamp(20px, 2.5vw, 32px)",
          letterSpacing: "0.04em", ...softNeon(color) }}>
          {section.title}
        </div>
      </div>

      <NeonDivider color={color} />

      {/* Items */}
      <div style={{ display: "flex", flexDirection: "column", gap: 14 }}>
        {section.items.map((item, i) => (
          <div key={item.id} className="item-row"
            style={{ animationDelay: `${delay + i * 120}ms`, display: "flex",
              alignItems: "flex-start", gap: 8 }}>
            {/* Bullet dot */}
            <div style={{ width: 5, height: 5, borderRadius: "50%", marginTop: 10, flexShrink: 0,
              background: color, boxShadow: `0 0 6px ${color}, 0 0 12px ${color}` }} />
            {/* Name + desc */}
            <div style={{ flex: 1 }}>
              <div style={{ fontFamily: "'Caveat', cursive", fontSize: "clamp(16px, 1.8vw, 24px)",
                fontWeight: 700, color: "#f0ece0",
                textShadow: "0 0 8px rgba(240,236,224,0.3), 0 1px 2px rgba(0,0,0,0.8)" }}>
                {item.name}
              </div>
              <div style={{ fontFamily: "'Caveat', cursive", fontSize: "clamp(11px, 1.1vw, 15px)",
                color: "#ffffff55", fontStyle: "italic", marginTop: -2 }}>
                {item.desc}
              </div>
            </div>
            {/* Price */}
            <div className="price-tag" style={{ textAlign: "right", flexShrink: 0 }}>
              {isHH ? (
                <>
                  <div style={{ fontFamily: "'Caveat', cursive", fontSize: "clamp(17px, 2vw, 26px)",
                    fontWeight: 700, ...neon(color, 0.6) }}>
                    {fmt(item.hhPrice)}
                  </div>
                  <div style={{ fontFamily: "'Caveat', cursive", fontSize: "clamp(11px, 1vw, 14px)",
                    color: "#ffffff30", textDecoration: "line-through" }}>
                    {fmt(item.price)}
                  </div>
                </>
              ) : (
                <div style={{ fontFamily: "'Caveat', cursive", fontSize: "clamp(17px, 2vw, 26px)",
                  fontWeight: 700, ...neon(color, 0.5) }}>
                  {fmt(item.price)}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ── Theme Picker (overlay control) ──────────────────────────────────────── */
function ThemePicker({ current, onChange }) {
  const [open, setOpen] = useState(false);
  return (
    <div style={{ position: "absolute", top: 16, right: 16, zIndex: 50 }}>
      <button onClick={() => setOpen(o => !o)}
        style={{ background: "#ffffff10", border: "1px solid #ffffff20", color: "#ffffff80",
          padding: "6px 14px", borderRadius: 8, cursor: "pointer", fontSize: 12,
          fontFamily: "'Caveat', cursive", backdropFilter: "blur(8px)" }}>
        🎨 Theme
      </button>
      {open && (
        <div style={{ position: "absolute", right: 0, top: 36, background: "#0d0d0dee",
          border: "1px solid #ffffff15", borderRadius: 10, padding: 8, backdropFilter: "blur(12px)",
          display: "flex", flexDirection: "column", gap: 4, minWidth: 150 }}>
          {Object.keys(THEMES).map(name => (
            <button key={name} onClick={() => { onChange(name); setOpen(false); }}
              style={{ background: name === current ? "#ffffff15" : "transparent",
                border: "none", color: name === current ? "#fff" : "#ffffff70",
                padding: "7px 14px", borderRadius: 6, cursor: "pointer", textAlign: "left",
                fontFamily: "'Caveat', cursive", fontSize: 15 }}>
              {name === current ? "✓ " : "  "}{name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

/* ── Main Chalkboard Display ─────────────────────────────────────────────── */
export default function NeonChalkboard() {
  const [themeName, setThemeName] = useState("Bar Classic");
  const [isHH, setIsHH] = useState(false);
  const [time, setTime] = useState(new Date());
  const theme = THEMES[themeName];

  useEffect(() => {
    const t = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(t);
  }, []);

  const timeStr = time.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  const dayStr  = time.toLocaleDateString([], { weekday: "long", month: "short", day: "numeric" });

  return (
    <>
      <style>{STYLES}</style>

      <div className="scanline" style={{
        width: "100vw", height: "100vh", overflow: "hidden", position: "relative",
        background: theme.bg,
        backgroundImage: `${CHALK_TEXTURE}, radial-gradient(ellipse at 30% 40%, ${theme.title}08 0%, transparent 55%), radial-gradient(ellipse at 70% 60%, ${theme.sectionColors[1]}06 0%, transparent 50%)`,
      }}>

        {/* Outer neon frame */}
        <div style={{
          position: "absolute", inset: 12, borderRadius: 8, pointerEvents: "none",
          border: `1px solid ${theme.title}40`,
          boxShadow: `0 0 12px ${theme.title}30, 0 0 30px ${theme.title}15, inset 0 0 20px ${theme.title}08`
        }} />

        {/* Corner decorations */}
        {[["4px","4px"], ["4px","auto"], ["auto","4px"], ["auto","auto"]].map(([t, r], i) => (
          <div key={i} style={{
            position: "absolute", top: t === "4px" ? 20 : "auto", bottom: t !== "4px" ? 20 : "auto",
            left: r !== "auto" ? 20 : "auto", right: r === "auto" ? 20 : "auto",
            width: 24, height: 24, pointerEvents: "none",
            borderTop: t === "4px" ? `2px solid ${theme.title}80` : "none",
            borderBottom: t !== "4px" ? `2px solid ${theme.title}80` : "none",
            borderLeft: r !== "auto" ? `2px solid ${theme.title}80` : "none",
            borderRight: r === "auto" ? `2px solid ${theme.title}80` : "none",
            boxShadow: `0 0 8px ${theme.title}60`,
          }} />
        ))}

        {/* Theme picker */}
        <ThemePicker current={themeName} onChange={setThemeName} />

        {/* HH toggle (demo control) */}
        <div style={{ position: "absolute", top: 16, left: 16, zIndex: 50, display: "flex",
          alignItems: "center", gap: 8 }}>
          <div onClick={() => setIsHH(h => !h)}
            style={{ width: 42, height: 22, borderRadius: 11, cursor: "pointer", transition: "all 0.2s",
              background: isHH ? theme.hhColor : "#ffffff15",
              boxShadow: isHH ? `0 0 10px ${theme.hhColor}` : "none" }}>
            <div style={{ width: 16, height: 16, borderRadius: "50%", background: "#fff",
              margin: "3px", marginLeft: isHH ? 22 : 3, transition: "margin-left 0.2s" }} />
          </div>
          <span style={{ fontFamily: "'Caveat'", fontSize: 14, color: "#ffffff50" }}>Happy Hour</span>
        </div>

        {/* ── HEADER ── */}
        <div style={{ textAlign: "center", padding: "28px 60px 0" }}>
          <h1 className="venue-title" style={{
            fontFamily: "'Pacifico', cursive",
            fontSize: "clamp(36px, 6vw, 80px)",
            letterSpacing: "0.03em",
            lineHeight: 1.1,
            ...neon(theme.titleGlow, 1.1),
            color: theme.title,
          }}>
            The Copper Still
          </h1>
          <p style={{
            fontFamily: "'Caveat', cursive",
            fontSize: "clamp(13px, 1.6vw, 20px)",
            marginTop: 4,
            letterSpacing: "0.25em",
            textTransform: "uppercase",
            ...softNeon(theme.title),
            opacity: 0.6,
          }}>
            ✦ Craft Cocktails & Local Beer ✦
          </p>

          {/* Neon rule under title */}
          <div style={{ margin: "14px auto 0", width: "60%", height: 1,
            background: `linear-gradient(to right, transparent, ${theme.title}cc, transparent)`,
            boxShadow: `0 0 10px ${theme.title}88, 0 0 24px ${theme.title}44` }} />
        </div>

        {/* ── HAPPY HOUR BANNER ── */}
        {isHH && (
          <div className="hh-badge" style={{
            margin: "12px auto 0", width: "fit-content",
            padding: "8px 36px", borderRadius: 6,
            border: `1px solid ${theme.hhColor}80`,
            ...edgeGlow(theme.hhColor),
            background: `${theme.hhColor}10`,
          }}>
            <span style={{
              fontFamily: "'Permanent Marker', cursive",
              fontSize: "clamp(14px, 2vw, 24px)",
              letterSpacing: "0.12em",
              ...neon(theme.hhGlow, 0.8),
              color: theme.hhColor,
            }}>
              ★ HAPPY HOUR ★ &nbsp; 4PM – 7PM &nbsp; MON – FRI
            </span>
          </div>
        )}

        {/* ── MENU GRID ── */}
        <div style={{
          display: "flex", flex: 1,
          padding: isHH ? "18px 28px 16px" : "24px 28px 16px",
          gap: 0, height: isHH ? "calc(100vh - 220px)" : "calc(100vh - 195px)",
          alignItems: "flex-start",
        }}>
          {MENU.map((section, i) => (
            <Section
              key={section.id}
              section={section}
              color={theme.sectionColors[i]}
              isHH={isHH}
              delay={300 + i * 200}
            />
          ))}
        </div>

        {/* ── FOOTER ── */}
        <div style={{
          position: "absolute", bottom: 18, left: 0, right: 0,
          display: "flex", alignItems: "center", justifyContent: "space-between",
          padding: "0 40px",
        }}>
          <div style={{ fontFamily: "'Caveat'", fontSize: 13, color: "#ffffff25", letterSpacing: "0.08em" }}>
            Powered by TapBoard · Screen sc-main-bar
          </div>
          <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
            <div style={{ display: "flex", gap: 8, fontFamily: "'Caveat'", fontSize: 13 }}>
              {["GF gluten free", "V vegetarian", "🔥 spicy"].map(t => (
                <span key={t} style={{ color: "#ffffff35" }}>· {t}</span>
              ))}
            </div>
          </div>
          <div style={{ textAlign: "right" }}>
            <div style={{ fontFamily: "'Permanent Marker', cursive", fontSize: "clamp(16px, 1.8vw, 22px)",
              ...softNeon(theme.title) }}>
              {timeStr}
            </div>
            <div style={{ fontFamily: "'Caveat'", fontSize: 12, color: "#ffffff35" }}>{dayStr}</div>
          </div>
        </div>

      </div>
    </>
  );
}
