import { useState, useEffect, useCallback } from "react";

/* ── Fonts & Animations ─────────────────────────────────────────────────── */
const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Pacifico&family=Lobster&family=Righteous&family=Fredoka+One&family=Bungee&family=Caveat:wght@400;600;700&family=Kalam:wght@400;700&family=Patrick+Hand&family=Permanent+Marker&family=Syne:wght@700;800&family=DM+Sans:wght@300;400;500;600&display=swap');
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  body { background: #0a0c10; font-family: 'DM Sans', sans-serif; color: #e2e8f0; }
  ::-webkit-scrollbar { width: 4px; }
  ::-webkit-scrollbar-track { background: #0d1117; }
  ::-webkit-scrollbar-thumb { background: #30363d; border-radius: 2px; }

  input[type=color] { -webkit-appearance: none; appearance: none; border: none;
    width: 100%; height: 100%; padding: 0; background: none; cursor: pointer; border-radius: 50%; }
  input[type=color]::-webkit-color-swatch-wrapper { padding: 0; border-radius: 50%; }
  input[type=color]::-webkit-color-swatch { border: none; border-radius: 50%; }
  input[type=range] { -webkit-appearance: none; appearance: none; background: transparent; cursor: pointer; width: 100%; }
  input[type=range]::-webkit-slider-runnable-track { background: #21262d; height: 4px; border-radius: 2px; }
  input[type=range]::-webkit-slider-thumb { -webkit-appearance: none; height: 14px; width: 14px;
    border-radius: 50%; background: #f59e0b; margin-top: -5px; }

  @keyframes flicker { 0%,18%,22%,25%,53%,57%,100%{opacity:1}20%,24%,55%{opacity:0.82} }
  @keyframes glow-breathe { 0%,100%{filter:brightness(1)} 50%{filter:brightness(1.3)} }
  @keyframes hh-pulse { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.88;transform:scale(1.01)} }
  @keyframes chalk-in { from{clip-path:inset(0 100% 0 0)} to{clip-path:inset(0 0 0 0)} }
  @keyframes fadeUp { from{opacity:0;transform:translateY(10px)} to{opacity:1;transform:translateY(0)} }
  @keyframes toastIn { from{opacity:0;transform:translateX(40px)} to{opacity:1;transform:translateX(0)} }
  @keyframes spin { to{transform:rotate(360deg)} }

  .venue-neon { animation: flicker 5s infinite, glow-breathe 3s ease-in-out infinite; }
  .hh-banner  { animation: hh-pulse 2s ease-in-out infinite; }
  .chalk-item { animation: chalk-in 0.45s ease forwards; opacity: 0; }
  .fade-up    { animation: fadeUp 0.3s ease forwards; }
  .panel-section { border-bottom: 1px solid #21262d; padding: 18px 0; }
  .panel-section:last-child { border-bottom: none; }
  .swatch-btn { transition: transform 0.15s, box-shadow 0.15s; cursor: pointer; }
  .swatch-btn:hover { transform: scale(1.12); }
`;

/* ── Data ───────────────────────────────────────────────────────────────── */
const MENU_DATA = [
  { id:"s1", title:"Craft Beers", emoji:"🍺",
    items:[
      { id:"i1", name:"Hazy IPA",    desc:"tropical & citrusy",      price:8.00,  hhPrice:5.00 },
      { id:"i2", name:"Dark Porter", desc:"rich chocolate notes",     price:7.00,  hhPrice:4.50 },
      { id:"i3", name:"Wheat Ale",   desc:"light & refreshing",       price:6.50,  hhPrice:4.00 },
      { id:"i4", name:"Pilsner",     desc:"crisp czech-style",        price:6.00,  hhPrice:4.00 },
    ]
  },
  { id:"s2", title:"Cocktails", emoji:"🍸",
    items:[
      { id:"i5", name:"Old Fashioned",  desc:"bourbon · bitters · orange",  price:13.00, hhPrice:9.00 },
      { id:"i6", name:"Aperol Spritz",  desc:"aperol · prosecco · soda",    price:12.00, hhPrice:8.00 },
      { id:"i7", name:"Mezcal Negroni", desc:"smoky · sweet · complex",     price:14.00, hhPrice:10.00},
      { id:"i8", name:"Paloma",         desc:"tequila · grapefruit · lime", price:12.00, hhPrice:8.00 },
    ]
  },
  { id:"s3", title:"Bar Bites", emoji:"🍟",
    items:[
      { id:"i9",  name:"Truffle Fries", desc:"parmesan & herbs",    price:9.00,  hhPrice:6.00 },
      { id:"i10", name:"Chicken Wings", desc:"choice of 3 sauces",  price:14.00, hhPrice:10.00},
      { id:"i11", name:"Nachos",        desc:"queso · jalapeños",   price:12.00, hhPrice:8.00 },
    ]
  },
];

const VENUE_FONTS = ["Pacifico","Lobster","Righteous","Fredoka One","Bungee","Permanent Marker"];
const MENU_FONTS  = ["Caveat","Kalam","Patrick Hand","Permanent Marker"];

const PRESETS = {
  "Bar Classic":   { bg:"#080f08", title:"#ffd700", titleGlow:"#ff8c00", s:["#00ffff","#ff6ec7","#a8ff3e"], hh:"#ff6ec7", hhGlow:"#ff00aa" },
  "Violet Lounge": { bg:"#09060f", title:"#bf5fff", titleGlow:"#7700ff", s:["#bf5fff","#00cfff","#ff6ec7"], hh:"#00cfff", hhGlow:"#0088ff" },
  "Hot Summer":    { bg:"#0f0700", title:"#ff4500", titleGlow:"#cc2200", s:["#ff6348","#ffd700","#ff9f43"], hh:"#ffd700", hhGlow:"#ffaa00" },
  "Ocean Dive":    { bg:"#00080f", title:"#00cfff", titleGlow:"#0055cc", s:["#00cfff","#7df9ff","#00ff88"], hh:"#00ff88", hhGlow:"#00cc55" },
  "Rose Gold":     { bg:"#0f0708", title:"#ff9eb5", titleGlow:"#ff4488", s:["#ff9eb5","#ffd700","#ffb347"], hh:"#ffd700", hhGlow:"#ffaa00" },
};

/* ── Default config ─────────────────────────────────────────────────────── */
const DEFAULT_CONFIG = {
  venueName:   "The Copper Still",
  tagline:     "Craft Cocktails & Local Beer",
  venueFont:   "Pacifico",
  menuFont:    "Caveat",
  bg:          "#080f08",
  title:       "#ffd700",
  titleGlow:   "#ff8c00",
  s:           ["#00ffff", "#ff6ec7", "#a8ff3e"],
  hh:          "#ff6ec7",
  hhGlow:      "#ff00aa",
  hhEnabled:   true,
  hhLabel:     "Happy Hour",
  hhTagline:   "All drinks discounted · Mon–Fri",
  hhStart:     "16:00",
  hhEnd:       "19:00",
  glowIntensity: 1.0,
};

/* ── Helpers ─────────────────────────────────────────────────────────────── */
const fmt = (n) => `$${Number(n).toFixed(2)}`;

const neon = (color, intensity = 1) => ({
  color: "#fff",
  textShadow: [
    `0 0 ${4  * intensity}px #fff`,
    `0 0 ${11 * intensity}px #fff`,
    `0 0 ${19 * intensity}px #fff`,
    `0 0 ${40 * intensity}px ${color}`,
    `0 0 ${80 * intensity}px ${color}`,
    `0 0 ${100 * intensity}px ${color}`,
    `0 0 ${150 * intensity}px ${color}`,
  ].join(", ")
});

const softNeon = (color, intensity = 1) => ({
  color,
  textShadow: `0 0 ${6 * intensity}px ${color}cc, 0 0 ${18 * intensity}px ${color}88, 0 0 ${40 * intensity}px ${color}55`
});

const edgeGlow = (color) => ({
  boxShadow: `0 0 8px ${color}88, 0 0 20px ${color}44, inset 0 0 8px ${color}11`
});

const CHALK_SVG = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3CfeColorMatrix type='saturate' values='0'/%3E%3C/filter%3E%3Crect width='300' height='300' filter='url(%23n)' opacity='0.045'/%3E%3C/svg%3E")`;

/* ── Toast ───────────────────────────────────────────────────────────────── */
function Toast({ msg, onDone }) {
  useEffect(() => { const t = setTimeout(onDone, 2400); return () => clearTimeout(t); }, []);
  return (
    <div style={{ position:"fixed", bottom:28, right:28, zIndex:1000,
      background:"linear-gradient(135deg,#22c55e,#16a34a)", color:"#fff",
      borderRadius:12, padding:"13px 22px", fontWeight:600, fontSize:14,
      boxShadow:"0 8px 32px rgba(0,0,0,0.5)", display:"flex", alignItems:"center", gap:10,
      animation:"toastIn 0.25s ease" }}>
      <span style={{ fontSize:18 }}>✓</span> {msg}
    </div>
  );
}

/* ── CHALKBOARD PREVIEW ─────────────────────────────────────────────────── */
function ChalkboardPreview({ cfg, scale = 1 }) {
  const [time, setTime] = useState(new Date());
  useEffect(() => { const t = setInterval(() => setTime(new Date()), 1000); return () => clearTimeout(t); }, []);

  const gi = cfg.glowIntensity;
  const timeStr = time.toLocaleTimeString([], { hour:"2-digit", minute:"2-digit" });

  return (
    <div style={{
      width: 960, height: 540,
      transform: `scale(${scale})`, transformOrigin: "top left",
      background: cfg.bg,
      backgroundImage: `${CHALK_SVG}, radial-gradient(ellipse at 30% 40%, ${cfg.title}08 0%, transparent 55%), radial-gradient(ellipse at 70% 60%, ${cfg.s[1]}06 0%, transparent 50%)`,
      position: "relative", overflow: "hidden", flexShrink: 0,
    }}>
      {/* Scanlines */}
      <div style={{ position:"absolute", inset:0, zIndex:8, pointerEvents:"none",
        backgroundImage:"repeating-linear-gradient(to bottom, transparent 0px, transparent 3px, rgba(0,0,0,0.07) 3px, rgba(0,0,0,0.07) 4px)" }} />

      {/* Neon frame */}
      <div style={{ position:"absolute", inset:8, borderRadius:6, pointerEvents:"none",
        border:`1px solid ${cfg.title}50`,
        boxShadow:`0 0 10px ${cfg.title}30, 0 0 28px ${cfg.title}15, inset 0 0 16px ${cfg.title}08` }} />

      {/* Corner brackets */}
      {[[true,true],[true,false],[false,true],[false,false]].map(([top,left], i) => (
        <div key={i} style={{ position:"absolute",
          top:top?14:"auto", bottom:top?"auto":14,
          left:left?14:"auto", right:left?"auto":14,
          width:18, height:18, pointerEvents:"none",
          borderTop:    top  ? `2px solid ${cfg.title}90` : "none",
          borderBottom: !top ? `2px solid ${cfg.title}90` : "none",
          borderLeft:   left ? `2px solid ${cfg.title}90` : "none",
          borderRight: !left ? `2px solid ${cfg.title}90` : "none",
          boxShadow:`0 0 7px ${cfg.title}70`,
        }} />
      ))}

      {/* Header */}
      <div style={{ textAlign:"center", padding:"22px 50px 0" }}>
        <h1 className="venue-neon" style={{
          fontFamily:`'${cfg.venueFont}', cursive`,
          fontSize:62, lineHeight:1.1, letterSpacing:"0.02em",
          ...neon(cfg.titleGlow, gi * 1.1), color:cfg.title,
        }}>{cfg.venueName}</h1>
        <p style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:15,
          letterSpacing:"0.22em", textTransform:"uppercase", marginTop:2,
          ...softNeon(cfg.title, gi * 0.6) }}>
          ✦ {cfg.tagline} ✦
        </p>
        <div style={{ margin:"10px auto 0", width:"55%", height:1,
          background:`linear-gradient(to right, transparent, ${cfg.title}cc, transparent)`,
          boxShadow:`0 0 8px ${cfg.title}88, 0 0 20px ${cfg.title}44` }} />
      </div>

      {/* HH Banner */}
      {cfg.hhEnabled && (
        <div className="hh-banner" style={{ margin:"8px auto 0", width:"fit-content",
          padding:"5px 28px", borderRadius:5,
          border:`1px solid ${cfg.hh}70`, ...edgeGlow(cfg.hh),
          background:`${cfg.hh}10` }}>
          <span style={{ fontFamily:"'Permanent Marker', cursive", fontSize:18,
            letterSpacing:"0.1em", ...neon(cfg.hhGlow, gi * 0.75), color:cfg.hh }}>
            ★ {cfg.hhLabel.toUpperCase()} ★ &nbsp; {cfg.hhStart} – {cfg.hhEnd}
          </span>
        </div>
      )}

      {/* Menu columns */}
      <div style={{ display:"flex", padding:"14px 20px 10px", gap:0,
        height: cfg.hhEnabled ? 370 : 390 }}>
        {MENU_DATA.map((section, si) => {
          const col = cfg.s[si];
          return (
            <div key={section.id} style={{ flex:1, padding:"0 14px",
              borderRight: si < 2 ? `1px solid #ffffff08` : "none" }}>
              <div style={{ textAlign:"center", marginBottom:2 }}>
                <span style={{ fontFamily:"'Permanent Marker', cursive", fontSize:24,
                  letterSpacing:"0.04em", ...softNeon(col, gi) }}>
                  {section.title}
                </span>
              </div>
              {/* Section divider */}
              <div style={{ margin:"6px 0 10px", height:1,
                background:`linear-gradient(to right, transparent, ${col}cc, transparent)`,
                boxShadow:`0 0 6px ${col}88, 0 0 14px ${col}44` }} />
              {section.items.map((item, ii) => (
                <div key={item.id} className="chalk-item"
                  style={{ animationDelay:`${si*180 + ii*80}ms`,
                    display:"flex", alignItems:"flex-start", gap:6, marginBottom:10 }}>
                  <div style={{ width:4, height:4, borderRadius:"50%", marginTop:8, flexShrink:0,
                    background:col, boxShadow:`0 0 5px ${col}, 0 0 10px ${col}` }} />
                  <div style={{ flex:1, minWidth:0 }}>
                    <div style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:17, fontWeight:700,
                      color:"#f0ece0", textShadow:"0 0 6px rgba(240,236,224,0.25), 0 1px 2px rgba(0,0,0,0.9)",
                      whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>
                      {item.name}
                    </div>
                    <div style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:12,
                      color:"#ffffff50", fontStyle:"italic", marginTop:-1 }}>
                      {item.desc}
                    </div>
                  </div>
                  <div style={{ flexShrink:0, textAlign:"right" }}>
                    {cfg.hhEnabled ? (
                      <>
                        <div style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:18, fontWeight:700,
                          ...neon(col, gi * 0.55) }}>{fmt(item.hhPrice)}</div>
                        <div style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:11,
                          color:"#ffffff30", textDecoration:"line-through" }}>{fmt(item.price)}</div>
                      </>
                    ) : (
                      <div style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:18, fontWeight:700,
                        ...neon(col, gi * 0.55) }}>{fmt(item.price)}</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          );
        })}
      </div>

      {/* Footer */}
      <div style={{ position:"absolute", bottom:12, left:0, right:0,
        display:"flex", alignItems:"center", justifyContent:"space-between", padding:"0 28px" }}>
        <div style={{ fontFamily:"'DM Sans'", fontSize:10, color:"#ffffff20", letterSpacing:"0.06em" }}>
          TapBoard · sc-main-bar · Connected ●
        </div>
        <div style={{ display:"flex", gap:10 }}>
          {["GF gluten free","V vegetarian","🔥 spicy"].map(t => (
            <span key={t} style={{ fontFamily:`'${cfg.menuFont}', cursive`, fontSize:11, color:"#ffffff30" }}>· {t}</span>
          ))}
        </div>
        <div style={{ fontFamily:"'Permanent Marker', cursive", fontSize:18,
          ...softNeon(cfg.title, gi * 0.7) }}>{timeStr}</div>
      </div>
    </div>
  );
}

/* ── Color Swatch Control ────────────────────────────────────────────────── */
function ColorControl({ label, value, onChange, size = 36 }) {
  return (
    <div style={{ display:"flex", flexDirection:"column", alignItems:"center", gap:5 }}>
      <div className="swatch-btn" style={{ width:size, height:size, borderRadius:"50%",
        background:value, boxShadow:`0 0 10px ${value}cc, 0 0 20px ${value}66`,
        border:"2px solid rgba(255,255,255,0.15)", overflow:"hidden", position:"relative" }}>
        <input type="color" value={value} onChange={e => onChange(e.target.value)} />
      </div>
      {label && <span style={{ fontSize:10, color:"#8b949e", whiteSpace:"nowrap" }}>{label}</span>}
    </div>
  );
}

/* ── Admin Panel ─────────────────────────────────────────────────────────── */
function AdminPanel({ cfg, setCfg, onPush }) {
  const set = useCallback((key, val) => setCfg(c => ({ ...c, [key]: val })), [setCfg]);
  const setS = useCallback((i, val) => setCfg(c => { const s=[...c.s]; s[i]=val; return {...c, s}; }), [setCfg]);

  const inputStyle = {
    background:"#21262d", border:"1px solid #30363d", borderRadius:8,
    padding:"8px 12px", color:"#e2e8f0", fontSize:13, outline:"none", width:"100%",
    fontFamily:"'DM Sans'",
  };
  const label = (txt) => (
    <div style={{ fontSize:11, fontWeight:600, color:"#8b949e",
      textTransform:"uppercase", letterSpacing:"0.06em", marginBottom:7 }}>{txt}</div>
  );

  return (
    <div style={{ width:300, height:"100%", overflowY:"auto", background:"#0d1117",
      borderRight:"1px solid #21262d", padding:"0 18px", flexShrink:0 }}>

      {/* Header */}
      <div style={{ padding:"18px 0 16px", borderBottom:"1px solid #21262d",
        display:"flex", alignItems:"center", gap:10 }}>
        <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:17, color:"#f59e0b" }}>TapBoard</div>
        <div style={{ fontSize:11, color:"#8b949e" }}>/ Theme Builder</div>
      </div>

      {/* Presets */}
      <div className="panel-section">
        {label("Quick Presets")}
        <div style={{ display:"flex", flexWrap:"wrap", gap:6 }}>
          {Object.entries(PRESETS).map(([name, p]) => (
            <button key={name} onClick={() => setCfg(c => ({ ...c, ...p }))}
              style={{ padding:"5px 10px", borderRadius:6, fontSize:12, cursor:"pointer",
                background:"#21262d", border:`1px solid ${p.title}40`, color:"#e2e8f0",
                boxShadow:`0 0 6px ${p.title}30`, fontFamily:"'DM Sans'" }}>
              {name}
            </button>
          ))}
        </div>
      </div>

      {/* Venue Identity */}
      <div className="panel-section">
        {label("Venue Name")}
        <input value={cfg.venueName} onChange={e => set("venueName", e.target.value)} style={inputStyle} />
        <div style={{ marginTop:10 }}>
          {label("Tagline")}
          <input value={cfg.tagline} onChange={e => set("tagline", e.target.value)} style={inputStyle} />
        </div>
      </div>

      {/* Title color + glow */}
      <div className="panel-section">
        {label("Title Neon")}
        <div style={{ display:"flex", gap:14, alignItems:"center" }}>
          <ColorControl label="Color" value={cfg.title}    onChange={v => set("title", v)} />
          <ColorControl label="Glow"  value={cfg.titleGlow} onChange={v => set("titleGlow", v)} />
          <div style={{ flex:1, fontSize:12, color:"#8b949e" }}>
            Tip: make Glow a darker shade of Color for depth
          </div>
        </div>
      </div>

      {/* Section colors */}
      <div className="panel-section">
        {label("Section Neon Colors")}
        <div style={{ display:"flex", gap:18 }}>
          {["Beers","Cocktails","Bites"].map((name, i) => (
            <ColorControl key={i} label={name} value={cfg.s[i]} onChange={v => setS(i, v)} />
          ))}
        </div>
      </div>

      {/* Glow intensity */}
      <div className="panel-section">
        {label(`Glow Intensity — ${Math.round(cfg.glowIntensity * 100)}%`)}
        <input type="range" min="0.2" max="2" step="0.05"
          value={cfg.glowIntensity} onChange={e => set("glowIntensity", parseFloat(e.target.value))} />
        <div style={{ display:"flex", justifyContent:"space-between", fontSize:10, color:"#8b949e", marginTop:3 }}>
          <span>Subtle</span><span>Electric</span>
        </div>
      </div>

      {/* Background */}
      <div className="panel-section">
        {label("Board Background")}
        <div style={{ display:"flex", gap:10, alignItems:"center" }}>
          <ColorControl value={cfg.bg} onChange={v => set("bg", v)} />
          <div style={{ display:"flex", gap:6, flexWrap:"wrap" }}>
            {["#080f08","#09060f","#0f0700","#00080f","#0d0d0d","#080808"].map(c => (
              <div key={c} onClick={() => set("bg", c)} className="swatch-btn"
                style={{ width:22, height:22, borderRadius:4, background:c, cursor:"pointer",
                  border: cfg.bg===c ? "2px solid #f59e0b" : "2px solid #30363d" }} />
            ))}
          </div>
        </div>
      </div>

      {/* Fonts */}
      <div className="panel-section">
        {label("Venue Name Font")}
        <div style={{ display:"flex", flexDirection:"column", gap:5 }}>
          {VENUE_FONTS.map(f => (
            <div key={f} onClick={() => set("venueFont", f)}
              style={{ padding:"7px 12px", borderRadius:7, cursor:"pointer",
                background: cfg.venueFont===f ? "#21262d" : "transparent",
                border: cfg.venueFont===f ? "1px solid #f59e0b40" : "1px solid transparent",
                display:"flex", alignItems:"center", justifyContent:"space-between" }}>
              <span style={{ fontFamily:`'${f}', cursive`, fontSize:18, color:"#f5f0e8" }}>{f}</span>
              {cfg.venueFont===f && <span style={{ color:"#f59e0b", fontSize:12 }}>✓</span>}
            </div>
          ))}
        </div>

        <div style={{ marginTop:14 }}>
          {label("Menu Items Font")}
          <div style={{ display:"flex", flexDirection:"column", gap:5 }}>
            {MENU_FONTS.map(f => (
              <div key={f} onClick={() => set("menuFont", f)}
                style={{ padding:"7px 12px", borderRadius:7, cursor:"pointer",
                  background: cfg.menuFont===f ? "#21262d" : "transparent",
                  border: cfg.menuFont===f ? "1px solid #f59e0b40" : "1px solid transparent",
                  display:"flex", alignItems:"center", justifyContent:"space-between" }}>
                <span style={{ fontFamily:`'${f}', cursive`, fontSize:16, color:"#f5f0e8" }}>Hazy IPA · $8.00</span>
                {cfg.menuFont===f && <span style={{ color:"#f59e0b", fontSize:12 }}>✓</span>}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Happy Hour */}
      <div className="panel-section">
        <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between", marginBottom:12 }}>
          {label("Happy Hour")}
          <div onClick={() => set("hhEnabled", !cfg.hhEnabled)}
            style={{ width:42, height:22, borderRadius:11, cursor:"pointer", transition:"all 0.2s",
              background:cfg.hhEnabled ? "#f59e0b" : "#21262d",
              boxShadow:cfg.hhEnabled ? "0 0 10px #f59e0b88" : "none", flexShrink:0 }}>
            <div style={{ width:16, height:16, borderRadius:"50%", background:"#fff",
              margin:3, marginLeft:cfg.hhEnabled ? 23 : 3, transition:"margin-left 0.2s" }} />
          </div>
        </div>
        {cfg.hhEnabled && (
          <div style={{ display:"flex", flexDirection:"column", gap:10 }}>
            <div style={{ display:"flex", gap:8 }}>
              <ColorControl label="Color" value={cfg.hh}     onChange={v => set("hh", v)} />
              <ColorControl label="Glow"  value={cfg.hhGlow} onChange={v => set("hhGlow", v)} />
            </div>
            <input value={cfg.hhLabel} onChange={e => set("hhLabel", e.target.value)}
              style={inputStyle} placeholder="Happy Hour" />
            <div style={{ display:"flex", gap:8 }}>
              <input type="time" value={cfg.hhStart} onChange={e => set("hhStart", e.target.value)}
                style={{ ...inputStyle, colorScheme:"dark" }} />
              <input type="time" value={cfg.hhEnd} onChange={e => set("hhEnd", e.target.value)}
                style={{ ...inputStyle, colorScheme:"dark" }} />
            </div>
          </div>
        )}
      </div>

      {/* Push button */}
      <div style={{ padding:"16px 0 24px" }}>
        <button onClick={onPush}
          style={{ width:"100%", padding:"13px", borderRadius:10, border:"none",
            background:"linear-gradient(135deg, #f59e0b, #d97706)", color:"#000",
            fontFamily:"'Syne'", fontWeight:800, fontSize:15, cursor:"pointer",
            boxShadow:"0 0 16px #f59e0b60, 0 4px 12px rgba(0,0,0,0.4)",
            transition:"transform 0.1s, box-shadow 0.1s" }}
          onMouseEnter={e => { e.target.style.transform="scale(1.02)"; e.target.style.boxShadow="0 0 24px #f59e0b88, 0 4px 16px rgba(0,0,0,0.5)"; }}
          onMouseLeave={e => { e.target.style.transform="scale(1)";    e.target.style.boxShadow="0 0 16px #f59e0b60, 0 4px 12px rgba(0,0,0,0.4)"; }}>
          📺 &nbsp; Push to All Screens
        </button>
        <div style={{ textAlign:"center", fontSize:11, color:"#8b949e", marginTop:8 }}>
          2 screens will update instantly via SignalR
        </div>
      </div>
    </div>
  );
}

/* ── App ─────────────────────────────────────────────────────────────────── */
export default function ThemeBuilder() {
  const [cfg, setCfg] = useState(DEFAULT_CONFIG);
  const [toast, setToast] = useState(null);
  const [previewScale, setPreviewScale] = useState(1);

  // Compute scale so preview fits in available space
  useEffect(() => {
    const calc = () => {
      const avail = window.innerWidth - 300 - 48; // panel width + padding
      const scale = Math.min(1, avail / 960);
      setPreviewScale(scale);
    };
    calc();
    window.addEventListener("resize", calc);
    return () => window.removeEventListener("resize", calc);
  }, []);

  const handlePush = () => setToast("Theme saved & pushed to 2 screens!");

  return (
    <>
      <style>{STYLES}</style>
      <div style={{ display:"flex", height:"100vh", overflow:"hidden", background:"#0a0c10" }}>

        {/* Left: Admin Panel */}
        <AdminPanel cfg={cfg} setCfg={setCfg} onPush={handlePush} />

        {/* Right: Preview area */}
        <div style={{ flex:1, display:"flex", flexDirection:"column",
          background:"#0a0c10", overflow:"hidden" }}>

          {/* Preview header */}
          <div style={{ padding:"14px 24px", borderBottom:"1px solid #21262d",
            display:"flex", alignItems:"center", gap:12, flexShrink:0 }}>
            <div style={{ width:8, height:8, borderRadius:"50%", background:"#22c55e",
              boxShadow:"0 0 6px #22c55e", animation:"glow-breathe 2s infinite" }} />
            <span style={{ fontSize:13, fontWeight:600, color:"#8b949e" }}>
              Live Preview — updates in real time
            </span>
            <div style={{ marginLeft:"auto", fontSize:12, color:"#8b949e",
              background:"#21262d", padding:"4px 12px", borderRadius:6 }}>
              1920 × 1080 @ {Math.round(previewScale * 100)}%
            </div>
          </div>

          {/* Preview canvas */}
          <div style={{ flex:1, display:"flex", alignItems:"center", justifyContent:"center",
            padding:24, overflow:"hidden" }}>
            <div style={{
              width: 960 * previewScale,
              height: 540 * previewScale,
              borderRadius: 10,
              overflow: "hidden",
              boxShadow: `0 0 40px ${cfg.title}30, 0 20px 60px rgba(0,0,0,0.7)`,
              border: "1px solid #30363d",
              flexShrink: 0,
            }}>
              <ChalkboardPreview cfg={cfg} scale={previewScale} />
            </div>
          </div>

          {/* Bottom bar */}
          <div style={{ padding:"10px 24px", borderTop:"1px solid #21262d",
            display:"flex", gap:16, alignItems:"center", flexShrink:0 }}>
            <span style={{ fontSize:11, color:"#8b949e" }}>Fonts:</span>
            <span style={{ fontSize:12, color:"#f59e0b", fontFamily:`'${cfg.venueFont}', cursive` }}>
              {cfg.venueFont}
            </span>
            <span style={{ fontSize:11, color:"#8b949e" }}>+</span>
            <span style={{ fontSize:12, color:"#f59e0b", fontFamily:`'${cfg.menuFont}', cursive` }}>
              {cfg.menuFont}
            </span>
            <span style={{ marginLeft:"auto", fontSize:11, color:"#8b949e" }}>
              Glow: {Math.round(cfg.glowIntensity * 100)}% · HH: {cfg.hhEnabled ? "On" : "Off"} · BG: {cfg.bg}
            </span>
          </div>
        </div>
      </div>

      {toast && <Toast msg={toast} onDone={() => setToast(null)} />}
    </>
  );
}
