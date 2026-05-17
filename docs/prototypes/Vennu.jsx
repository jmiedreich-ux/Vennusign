import { useState, useEffect, useCallback, useRef } from "react";

/* ── Global Styles ──────────────────────────────────────────────────────── */
const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@500;600;700&family=Outfit:wght@300;400;500;600;700&family=DM+Mono:wght@400;500&family=Caveat:wght@600;700&family=Noto+Sans+SC:wght@400;700&family=Pacifico&display=swap');
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  body { background: #0c0e12; font-family: 'Outfit', sans-serif; color: #e8e3db; overflow: hidden; }
  ::-webkit-scrollbar { width: 3px; } ::-webkit-scrollbar-track { background: transparent; }
  ::-webkit-scrollbar-thumb { background: #2a2e38; border-radius: 2px; }
  input, select, textarea, button { font-family: 'Outfit', sans-serif; }
  input[type=range] { -webkit-appearance:none; width:100%; background:transparent; cursor:pointer; }
  input[type=range]::-webkit-slider-runnable-track { background:#2a2e38; height:3px; border-radius:2px; }
  input[type=range]::-webkit-slider-thumb { -webkit-appearance:none; width:14px; height:14px; border-radius:50%; background:#f0a500; margin-top:-5.5px; }
  input[type=color] { -webkit-appearance:none; border:none; background:none; cursor:pointer; padding:0; width:100%; height:100%; border-radius:50%; }
  input[type=color]::-webkit-color-swatch-wrapper { padding:0; }
  input[type=color]::-webkit-color-swatch { border:none; border-radius:50%; }

  @keyframes fadeUp    { from{opacity:0;transform:translateY(10px)} to{opacity:1;transform:translateY(0)} }
  @keyframes fadeIn    { from{opacity:0} to{opacity:1} }
  @keyframes slideIn   { from{opacity:0;transform:translateX(-12px)} to{opacity:1;transform:translateX(0)} }
  @keyframes popIn     { from{opacity:0;transform:scale(0.95)} to{opacity:1;transform:scale(1)} }
  @keyframes toastIn   { from{opacity:0;transform:translateY(16px)} to{opacity:1;transform:translateY(0)} }
  @keyframes pulse     { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.6;transform:scale(1.2)} }
  @keyframes flicker   { 0%,19%,21%,23%,25%,54%,56%,100%{opacity:1} 20%,24%,55%{opacity:.75} }
  @keyframes glowBreathe { 0%,100%{filter:brightness(1)} 50%{filter:brightness(1.3)} }
  @keyframes chalkDraw { from{clip-path:inset(0 100% 0 0)} to{clip-path:inset(0 0% 0 0)} }
  @keyframes shimmer   { 0%{background-position:-200% 0} 100%{background-position:200% 0} }
  @keyframes spin      { to{transform:rotate(360deg)} }
  @keyframes countUp   { from{transform:translateY(8px);opacity:0} to{transform:translateY(0);opacity:1} }

  .fade-up    { animation: fadeUp 0.35s ease forwards; }
  .fade-in    { animation: fadeIn 0.25s ease forwards; }
  .slide-in   { animation: slideIn 0.3s ease forwards; }
  .pop-in     { animation: popIn 0.2s ease forwards; }
  .pulse-dot  { animation: pulse 2s ease-in-out infinite; }
  .venue-neon { animation: flicker 5s infinite, glowBreathe 3s ease-in-out infinite; }
  .chalk-item { animation: chalkDraw 0.4s ease forwards; }
  .nav-link   { transition: all 0.15s ease; }
  .nav-link:hover { background: rgba(240,165,0,0.06) !important; }
  .btn        { transition: all 0.15s ease; cursor: pointer; border: none; outline: none; }
  .btn:active { transform: scale(0.97); }
  .card       { transition: box-shadow 0.2s ease; }
`;

/* ── Vennu Brand Colors ─────────────────────────────────────────────────── */
const V = {
  bg:        "#0c0e12",
  surface:   "#13161c",
  elevated:  "#1a1e26",
  border:    "#23272f",
  borderHov: "#363c4a",
  amber:     "#f0a500",
  amberSoft: "#f0a50015",
  amberBord: "#f0a50030",
  coral:     "#e8673a",
  sage:      "#5cb88a",
  sky:       "#4ab3d4",
  muted:     "#8892a0",
  text:      "#e8e3db",
  textSoft:  "#b8b3ab",
};

/* ── Sample Data ────────────────────────────────────────────────────────── */
const MENU_DATA = {
  sections: [
    {
      id:"s1", title:"Starters", titleZh:"开胃菜", emoji:"🥟",
      items:[
        { id:"i1", name:"Spring Rolls",    nameZh:"春卷",    desc:"Crispy pork & veg",     descZh:"香脆猪肉蔬菜",  price:9.00,  hhPrice:6.50, tags:["V"],    available:true,  qty:null },
        { id:"i2", name:"Soup Dumplings",  nameZh:"小笼包",  desc:"Steamed pork broth",    descZh:"猪肉汤汁",     price:12.00, hhPrice:9.00, tags:["🔥"],   available:true,  qty:8    },
        { id:"i3", name:"Edamame",         nameZh:"毛豆",    desc:"Sea salt & sesame",     descZh:"海盐芝麻",     price:6.00,  hhPrice:4.00, tags:["V","GF"], available:true, qty:null },
      ]
    },
    {
      id:"s2", title:"Mains", titleZh:"主菜", emoji:"🍛",
      items:[
        { id:"i4", name:"General Tso's",   nameZh:"左宗棠鸡", desc:"Sweet & spicy chicken",  descZh:"香辣酥脆鸡肉", price:15.99, hhPrice:11.00, tags:["🔥"],  available:true,  qty:null },
        { id:"i5", name:"Kung Pao Shrimp", nameZh:"宫保虾",   desc:"Wok-fired with peanuts", descZh:"花生炒虾",    price:17.99, hhPrice:13.00, tags:["GF"],  available:true,  qty:null },
        { id:"i6", name:"Beef & Broccoli", nameZh:"西兰花牛肉", desc:"Tender beef, brown sauce",descZh:"嫩牛肉棕汁", price:16.99, hhPrice:12.00, tags:[],      available:false, qty:null },
        { id:"i7", name:"Mapo Tofu",       nameZh:"麻婆豆腐",  desc:"Silken tofu, spicy",    descZh:"嫩豆腐香辣",  price:13.99, hhPrice:10.00, tags:["V","🔥"], available:true, qty:3   },
      ]
    },
    {
      id:"s3", title:"Drinks", titleZh:"饮品", emoji:"🍵",
      items:[
        { id:"i8", name:"Jasmine Tea",     nameZh:"茉莉花茶", desc:"Hot or iced",           descZh:"热或冰",       price:3.50,  hhPrice:2.00, tags:["V","GF"], available:true, qty:null },
        { id:"i9", name:"Tsingtao Beer",   nameZh:"青岛啤酒", desc:"Cold & crisp",           descZh:"冰爽清脆",    price:5.00,  hhPrice:3.50, tags:[],        available:true,  qty:null },
      ]
    }
  ]
};

const SCREENS_DATA = [
  { id:"sc1", name:"Main Dining",   location:"Front wall",   status:"online",  lastSeen:"Just now",   wallGroup:null },
  { id:"sc2", name:"Bar Counter",   location:"Above bar",    status:"online",  lastSeen:"1 min ago",  wallGroup:"wall-1" },
  { id:"sc3", name:"Bar Counter 2", location:"Above bar",    status:"online",  lastSeen:"1 min ago",  wallGroup:"wall-1" },
  { id:"sc4", name:"Entrance",      location:"Front door",   status:"offline", lastSeen:"3 hrs ago",  wallGroup:null },
];

const MEAL_PERIODS = [
  { id:"mp1", name:"Breakfast", icon:"☀️", start:"06:00", end:"11:00", active:false, color:"#f0a500" },
  { id:"mp2", name:"Lunch",     icon:"🌤",  start:"11:00", end:"15:00", active:true,  color:"#5cb88a" },
  { id:"mp3", name:"Afternoon", icon:"☁️",  start:"15:00", end:"17:00", active:false, color:"#8892a0" },
  { id:"mp4", name:"Dinner",    icon:"🌙",  start:"17:00", end:"22:00", active:false, color:"#4ab3d4" },
];

const VENUE_TYPES = [
  { id:"restaurant", label:"Restaurant",   icon:"🍽" },
  { id:"bar",        label:"Bar & Lounge", icon:"🍸" },
  { id:"qsr",        label:"Quick Service",icon:"🥡" },
  { id:"cafe",       label:"Café",         icon:"☕" },
  { id:"brewery",    label:"Brewery",      icon:"🍺" },
  { id:"foodhall",   label:"Food Hall",    icon:"🏪" },
];

/* ── Helpers ─────────────────────────────────────────────────────────────── */
const fmt = (n) => `$${Number(n).toFixed(2)}`;
const uid = () => Math.random().toString(36).slice(2,8);

const inputSt = {
  background: V.elevated, border:`1px solid ${V.border}`, borderRadius:8,
  padding:"8px 12px", color:V.text, fontSize:13, outline:"none", width:"100%",
};
const labelSt = {
  display:"block", fontSize:11, fontWeight:600, color:V.muted,
  textTransform:"uppercase", letterSpacing:"0.07em", marginBottom:6,
};

/* ── Toast ───────────────────────────────────────────────────────────────── */
function Toast({ msg, onDone }) {
  useEffect(() => { const t = setTimeout(onDone, 2400); return () => clearTimeout(t); }, []);
  return (
    <div style={{ position:"fixed", bottom:28, right:28, zIndex:9999,
      background:`linear-gradient(135deg, ${V.sage}, #3a9e6d)`, color:"#fff",
      borderRadius:12, padding:"13px 22px", fontWeight:600, fontSize:13,
      boxShadow:"0 8px 32px rgba(0,0,0,0.6)", display:"flex", alignItems:"center",
      gap:10, animation:"toastIn 0.3s ease" }}>
      <span style={{fontSize:16}}>✓</span> {msg}
    </div>
  );
}

/* ── Toggle ──────────────────────────────────────────────────────────────── */
function Toggle({ value, onChange, color = V.amber }) {
  return (
    <div onClick={() => onChange(!value)} style={{
      width:40, height:22, borderRadius:11, cursor:"pointer", transition:"all 0.2s",
      background: value ? color : V.elevated,
      border:`1px solid ${value ? color : V.border}`,
      boxShadow: value ? `0 0 8px ${color}60` : "none",
      position:"relative", flexShrink:0,
    }}>
      <div style={{ width:16, height:16, borderRadius:"50%", background:"#fff",
        position:"absolute", top:2, left: value ? 20 : 2, transition:"left 0.2s",
        boxShadow:"0 1px 4px rgba(0,0,0,0.3)" }} />
    </div>
  );
}

/* ── NEON CHALKBOARD DISPLAY ─────────────────────────────────────────────── */
const CHALK_BG = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3CfeColorMatrix type='saturate' values='0'/%3E%3C/filter%3E%3Crect width='300' height='300' filter='url(%23n)' opacity='0.04'/%3E%3C/svg%3E")`;

function neon(color, i=1) {
  return { color:"#fff", textShadow:[
    `0 0 ${4*i}px #fff`, `0 0 ${11*i}px #fff`, `0 0 ${19*i}px #fff`,
    `0 0 ${40*i}px ${color}`, `0 0 ${80*i}px ${color}`, `0 0 ${120*i}px ${color}`,
  ].join(",") };
}
function softNeon(color, i=1) {
  return { color, textShadow:`0 0 ${6*i}px ${color}cc, 0 0 ${18*i}px ${color}66, 0 0 ${36*i}px ${color}33` };
}

function ChalkboardDisplay({ sections, venueName, isHH, bilingual, lang2="zh" }) {
  const [time, setTime] = useState(new Date());
  useEffect(() => { const t=setInterval(()=>setTime(new Date()),1000); return ()=>clearInterval(t); },[]);
  const tc="#ffd700", sc=["#00ffff","#ff6ec7","#a8ff3e"];
  const timeStr = time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"});

  return (
    <div style={{ width:"100%", height:"100%", background:"#080f08",
      backgroundImage:`${CHALK_BG}, radial-gradient(ellipse at 25% 35%, ${tc}08 0%, transparent 55%)`,
      position:"relative", overflow:"hidden", display:"flex", flexDirection:"column" }}>
      {/* Frame */}
      <div style={{ position:"absolute", inset:8, borderRadius:6, pointerEvents:"none",
        border:`1px solid ${tc}40`, boxShadow:`0 0 10px ${tc}25, inset 0 0 20px ${tc}06` }} />
      {/* Scanlines */}
      <div style={{ position:"absolute", inset:0, pointerEvents:"none", zIndex:5,
        backgroundImage:"repeating-linear-gradient(to bottom,transparent 0px,transparent 3px,rgba(0,0,0,0.06) 3px,rgba(0,0,0,0.06) 4px)" }} />
      {/* Header */}
      <div style={{ textAlign:"center", padding:"18px 50px 0", flexShrink:0 }}>
        <div className="venue-neon" style={{ fontFamily:"'Pacifico',cursive", fontSize:52,
          lineHeight:1.1, ...neon(tc, 1), color:tc }}>{venueName}</div>
        <div style={{ fontFamily:"'Caveat',cursive", fontSize:14, letterSpacing:"0.22em",
          textTransform:"uppercase", marginTop:2, ...softNeon(tc, 0.5) }}>
          ✦ Every Venue · Every Menu ✦
        </div>
        <div style={{ margin:"10px auto 0", width:"50%", height:1,
          background:`linear-gradient(to right,transparent,${tc}cc,transparent)`,
          boxShadow:`0 0 8px ${tc}88` }} />
      </div>
      {/* HH Banner */}
      {isHH && (
        <div style={{ margin:"8px auto 0", width:"fit-content", padding:"5px 24px",
          borderRadius:5, border:`1px solid #ff6ec780`, background:"#ff6ec710",
          boxShadow:"0 0 12px #ff6ec740" }}>
          <span style={{ fontFamily:"'Caveat',cursive", fontSize:16, fontWeight:700,
            letterSpacing:"0.1em", ...neon("#ff6ec7", 0.7), color:"#ff6ec7" }}>
            ★ HAPPY HOUR · SPECIAL PRICES ★
          </span>
        </div>
      )}
      {/* Menu grid */}
      <div style={{ display:"flex", flex:1, padding:"12px 20px 10px", gap:0, overflow:"hidden" }}>
        {sections.map((sec, si) => {
          const col = sc[si] || sc[0];
          return (
            <div key={sec.id} style={{ flex:1, padding:"0 12px",
              borderRight: si<sections.length-1 ? "1px solid #ffffff08" : "none" }}>
              <div style={{ textAlign:"center", marginBottom:2 }}>
                <span style={{ fontFamily:"'Caveat',cursive", fontSize:22, fontWeight:700,
                  letterSpacing:"0.04em", ...softNeon(col) }}>
                  {sec.title}
                  {bilingual && <span style={{ fontFamily:"'Noto Sans SC',sans-serif",
                    fontSize:14, marginLeft:8, opacity:0.7 }}>{sec.titleZh}</span>}
                </span>
              </div>
              <div style={{ margin:"6px 0 8px", height:1,
                background:`linear-gradient(to right,transparent,${col}cc,transparent)`,
                boxShadow:`0 0 6px ${col}88` }} />
              {sec.items.filter(i=>i.available).map((item,ii) => (
                <div key={item.id} className="chalk-item"
                  style={{ animationDelay:`${si*150+ii*70}ms`,
                    display:"flex", alignItems:"flex-start", gap:6, marginBottom:10 }}>
                  <div style={{ width:4, height:4, borderRadius:"50%", marginTop:8, flexShrink:0,
                    background:col, boxShadow:`0 0 5px ${col}` }} />
                  <div style={{ flex:1, minWidth:0 }}>
                    <div style={{ fontFamily:"'Caveat',cursive", fontSize:16, fontWeight:700,
                      color:"#f0ece0", lineHeight:1.2 }}>{item.name}</div>
                    {bilingual && (
                      <div style={{ fontFamily:"'Noto Sans SC',sans-serif", fontSize:11,
                        color:"#ffffff60", marginTop:-1 }}>{item.nameZh}</div>
                    )}
                    <div style={{ fontFamily:"'Caveat',cursive", fontSize:11,
                      color:"#ffffff45", fontStyle:"italic" }}>{item.desc}</div>
                  </div>
                  <div style={{ flexShrink:0, textAlign:"right" }}>
                    {isHH ? (
                      <>
                        <div style={{ fontFamily:"'Caveat',cursive", fontSize:17, fontWeight:700,
                          ...neon(col, 0.5) }}>{fmt(item.hhPrice)}</div>
                        <div style={{ fontFamily:"'Caveat',cursive", fontSize:11,
                          color:"#ffffff30", textDecoration:"line-through" }}>{fmt(item.price)}</div>
                      </>
                    ) : (
                      <div style={{ fontFamily:"'Caveat',cursive", fontSize:17, fontWeight:700,
                        ...neon(col, 0.45) }}>{fmt(item.price)}</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          );
        })}
      </div>
      {/* Footer */}
      <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center",
        padding:"6px 24px 10px", borderTop:"1px solid #ffffff06", flexShrink:0 }}>
        <div style={{ fontFamily:"'Outfit'", fontSize:9, color:"#ffffff18", letterSpacing:"0.1em" }}>
          VENNU · sc-main · CONNECTED ●
        </div>
        <div style={{ fontFamily:"'Caveat',cursive", fontSize:18, ...softNeon(tc, 0.6) }}>
          {timeStr}
        </div>
      </div>
    </div>
  );
}

/* ── PHOTO GRID DISPLAY ──────────────────────────────────────────────────── */
function PhotoCard({ item, bilingual, isHH }) {
  const [from, to] = [["#c1440e","#ff6b35"],["#0077b6","#48cae4"],["#3d6b45","#52b788"],
    ["#8b1a1a","#c1440e"],["#d4a017","#f7c59f"],["#9e1b00","#e63946"]][
    Math.abs(item.name.length) % 6];
  return (
    <div style={{ borderRadius:10, overflow:"hidden", background:"#111",
      display:"flex", flexDirection:"column", position:"relative" }}>
      <div style={{ flex:1, minHeight:100, background:`linear-gradient(135deg,${from},${to})`,
        display:"flex", alignItems:"center", justifyContent:"center", position:"relative" }}>
        <span style={{ fontSize:40, filter:"drop-shadow(0 2px 6px rgba(0,0,0,0.5))" }}>{item.emoji||"🍽"}</span>
        {item.qty && item.qty <= 5 && (
          <div style={{ position:"absolute", top:8, right:8, background:"#ef4444",
            color:"#fff", fontSize:9, fontWeight:700, padding:"2px 6px", borderRadius:4 }}>
            Only {item.qty} left!
          </div>
        )}
        {item.tags?.includes("V") && (
          <div style={{ position:"absolute", top:8, left:8, background:"#22c55e",
            color:"#fff", fontSize:9, fontWeight:700, padding:"2px 6px", borderRadius:4 }}>🌱 V</div>
        )}
        <div style={{ position:"absolute", bottom:0, left:0, right:0, height:"40%",
          background:"linear-gradient(to top,rgba(0,0,0,0.85),transparent)" }} />
      </div>
      <div style={{ padding:"8px 10px", background:"#111" }}>
        <div style={{ fontWeight:700, fontSize:13, color:"#f5f0e8",
          whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{item.name}</div>
        {bilingual && <div style={{ fontFamily:"'Noto Sans SC'", fontSize:11, color:"#8b949e" }}>{item.nameZh}</div>}
        <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center", marginTop:5 }}>
          <div style={{ display:"flex", gap:5, alignItems:"center" }}>
            {isHH && <span style={{ fontSize:13, fontWeight:800, color:"#f0a500" }}>{fmt(item.hhPrice)}</span>}
            <span style={{ fontSize:13, fontWeight:700,
              color: isHH ? "#8b949e" : "#f0a500",
              textDecoration: isHH ? "line-through" : "none" }}>{fmt(item.price)}</span>
          </div>
          {item.qty && <span style={{ fontSize:9, color:"#ef4444", fontWeight:700 }}>{item.qty} left</span>}
        </div>
      </div>
    </div>
  );
}

function PhotoDisplay({ sections, venueName, isHH, bilingual }) {
  const [time, setTime] = useState(new Date());
  useEffect(() => { const t=setInterval(()=>setTime(new Date()),1000); return ()=>clearInterval(t); },[]);
  const allItems = sections.flatMap(s => s.items.filter(i=>i.available)).slice(0,6);

  return (
    <div style={{ width:"100%", height:"100%", background:"#0d0d0d",
      display:"flex", flexDirection:"column", overflow:"hidden" }}>
      <div style={{ background:"#111", borderBottom:"1px solid #ffffff10",
        padding:"10px 18px", display:"flex", alignItems:"center", flexShrink:0 }}>
        <div style={{ flex:1 }}>
          <div style={{ fontFamily:"'Playfair Display'", fontSize:22, fontWeight:600,
            color:"#f5f0e8" }}>{venueName}</div>
          {isHH && <div style={{ fontSize:10, color:"#f0a500", fontWeight:700,
            letterSpacing:"0.08em" }}>★ HAPPY HOUR PRICES</div>}
        </div>
        <div style={{ fontFamily:"'Outfit'", fontSize:18, fontWeight:600, color:"#f5f0e8" }}>
          {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
        </div>
      </div>
      <div style={{ flex:1, padding:10, display:"grid", gap:8,
        gridTemplateColumns:"repeat(3,1fr)", gridTemplateRows:"repeat(2,1fr)", overflow:"hidden" }}>
        {allItems.map((item,i) => (
          <div key={item.id} className="pop-in" style={{ animationDelay:`${i*50}ms` }}>
            <PhotoCard item={item} bilingual={bilingual} isHH={isHH} />
          </div>
        ))}
      </div>
      <div style={{ padding:"6px 18px", borderTop:"1px solid #ffffff06", flexShrink:0,
        display:"flex", justifyContent:"space-between", alignItems:"center" }}>
        <div style={{ fontSize:9, color:"#ffffff18", letterSpacing:"0.06em" }}>VENNU · CONNECTED</div>
        <div style={{ display:"flex", gap:10 }}>
          {["GF","V 🌱","🔥 Spicy"].map(t=>(
            <span key={t} style={{ fontSize:9, color:"#ffffff25" }}>· {t}</span>
          ))}
        </div>
      </div>
    </div>
  );
}

/* ── DINER DISPLAY ───────────────────────────────────────────────────────── */
function DinerDisplay({ sections, venueName, isHH, bilingual }) {
  const [time, setTime] = useState(new Date());
  useEffect(() => { const t=setInterval(()=>setTime(new Date()),1000); return ()=>clearInterval(t); },[]);
  return (
    <div style={{ width:"100%", height:"100%", background:"#fffdf7",
      display:"flex", flexDirection:"column", overflow:"hidden", color:"#1a1a1a" }}>
      {/* Header */}
      <div style={{ background:"#1a1a1a", padding:"12px 24px",
        display:"flex", alignItems:"center", justifyContent:"space-between", flexShrink:0 }}>
        <div style={{ fontFamily:"'Playfair Display'", fontSize:26, fontWeight:700, color:"#f0a500" }}>
          {venueName}
        </div>
        <div style={{ color:"#f0a500", fontFamily:"'DM Mono'", fontSize:16 }}>
          {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
        </div>
      </div>
      {isHH && (
        <div style={{ background:"#f0a500", color:"#1a1a1a", textAlign:"center",
          padding:"5px", fontSize:12, fontWeight:700, letterSpacing:"0.08em" }}>
          ★ HAPPY HOUR — DISCOUNTED PRICES SHOWN ★
        </div>
      )}
      {/* Content */}
      <div style={{ flex:1, display:"grid",
        gridTemplateColumns:`repeat(${sections.length},1fr)`, gap:0, overflow:"hidden" }}>
        {sections.map((sec, si) => (
          <div key={sec.id} style={{ padding:"16px 20px",
            borderRight: si<sections.length-1 ? "2px solid #e8e3d8" : "none" }}>
            <div style={{ fontFamily:"'Playfair Display'", fontSize:18, fontWeight:700,
              color:"#1a1a1a", marginBottom:2 }}>
              {sec.title}
              {bilingual && <span style={{ fontFamily:"'Noto Sans SC'", fontSize:13,
                marginLeft:8, color:"#666" }}>{sec.titleZh}</span>}
            </div>
            <div style={{ width:"100%", height:2, background:"#1a1a1a", marginBottom:12, opacity:0.15 }} />
            {sec.items.filter(i=>i.available).map(item => (
              <div key={item.id} style={{ display:"flex", justifyContent:"space-between",
                alignItems:"flex-start", marginBottom:10, gap:8 }}>
                <div style={{ flex:1 }}>
                  <div style={{ fontWeight:600, fontSize:13, color:"#1a1a1a" }}>
                    {item.name}
                    {item.tags?.map(t => (
                      <span key={t} style={{ fontSize:9, marginLeft:5, padding:"1px 4px",
                        background:"#f0a50020", color:"#c07800", borderRadius:3,
                        fontWeight:700, border:"1px solid #f0a50040" }}>{t}</span>
                    ))}
                  </div>
                  {bilingual && <div style={{ fontFamily:"'Noto Sans SC'", fontSize:11,
                    color:"#888", marginTop:1 }}>{item.nameZh}</div>}
                  <div style={{ fontSize:11, color:"#888", marginTop:1 }}>{item.desc}</div>
                  {item.qty && <div style={{ fontSize:10, color:"#ef4444", fontWeight:700, marginTop:2 }}>Only {item.qty} left</div>}
                </div>
                <div style={{ textAlign:"right", flexShrink:0 }}>
                  {isHH ? (
                    <>
                      <div style={{ fontWeight:800, color:"#c07800", fontSize:14 }}>{fmt(item.hhPrice)}</div>
                      <div style={{ fontSize:10, color:"#aaa", textDecoration:"line-through" }}>{fmt(item.price)}</div>
                    </>
                  ) : (
                    <div style={{ fontWeight:700, color:"#1a1a1a", fontSize:14 }}>{fmt(item.price)}</div>
                  )}
                </div>
              </div>
            ))}
          </div>
        ))}
      </div>
      <div style={{ background:"#1a1a1a", padding:"6px 24px", flexShrink:0,
        display:"flex", justifyContent:"space-between" }}>
        <div style={{ fontSize:9, color:"#ffffff30", letterSpacing:"0.06em" }}>VENNU · CONNECTED</div>
        <div style={{ fontSize:9, color:"#ffffff30" }}>GF · V · 🔥 · ☪ Halal · 🌱 Vegan</div>
      </div>
    </div>
  );
}

/* ── MENU EDITOR ─────────────────────────────────────────────────────────── */
function MenuEditor({ sections, setSections, isHH, bilingual, toast }) {
  const [editId, setEditId] = useState(null);
  const [editData, setEditData] = useState({});
  const [expanded, setExpanded] = useState("s1");

  const startEdit = (item) => { setEditId(item.id); setEditData({...item}); };
  const cancelEdit = () => { setEditId(null); setEditData({}); };
  const saveItem = () => {
    setSections(secs => secs.map(s => ({
      ...s, items: s.items.map(i => i.id===editId ? {...editData,id:editId} : i)
    })));
    toast("Item saved · Synced to screens");
    cancelEdit();
  };
  const toggleAvail = (id) => {
    setSections(secs => secs.map(s => ({
      ...s, items: s.items.map(i => i.id===id ? {...i,available:!i.available} : i)
    })));
    toast("Availability updated");
  };

  return (
    <div style={{ display:"flex", flexDirection:"column", gap:10 }}>
      {sections.map(sec => (
        <div key={sec.id} style={{ background:V.surface, borderRadius:12, overflow:"hidden",
          border:`1px solid ${V.border}` }}>
          <div onClick={() => setExpanded(expanded===sec.id ? null : sec.id)}
            style={{ display:"flex", alignItems:"center", padding:"13px 18px", cursor:"pointer",
              background: expanded===sec.id ? V.elevated : "transparent",
              borderBottom: expanded===sec.id ? `1px solid ${V.border}` : "none" }}>
            <span style={{ fontSize:18, marginRight:10 }}>{sec.emoji}</span>
            <span style={{ fontFamily:"'Playfair Display'", fontWeight:600, fontSize:16, flex:1 }}>{sec.title}</span>
            {bilingual && <span style={{ fontFamily:"'Noto Sans SC'", fontSize:13, color:V.muted, marginRight:12 }}>{sec.titleZh}</span>}
            <span style={{ fontSize:12, color:V.muted }}>{sec.items.length} items</span>
            <span style={{ color:V.muted, marginLeft:10, fontSize:11 }}>{expanded===sec.id?"▲":"▼"}</span>
          </div>
          {expanded===sec.id && (
            <div style={{ padding:"8px 12px 12px" }}>
              {sec.items.map(item => (
                <div key={item.id} style={{ padding:"10px 8px", borderRadius:8, marginBottom:4,
                  background: editId===item.id ? V.elevated : "transparent",
                  border: editId===item.id ? `1px solid ${V.amberBord}` : "1px solid transparent",
                  opacity: item.available ? 1 : 0.5, transition:"all 0.15s" }}>
                  {editId===item.id ? (
                    <div style={{ display:"flex", flexDirection:"column", gap:10 }}>
                      <div style={{ display:"grid", gridTemplateColumns:"1fr 1fr", gap:10 }}>
                        <div>
                          <span style={labelSt}>Name (EN)</span>
                          <input value={editData.name} onChange={e=>setEditData(d=>({...d,name:e.target.value}))} style={inputSt} />
                        </div>
                        {bilingual && <div>
                          <span style={labelSt}>名称 (ZH)</span>
                          <input value={editData.nameZh||""} onChange={e=>setEditData(d=>({...d,nameZh:e.target.value}))}
                            style={{...inputSt,fontFamily:"'Noto Sans SC',sans-serif"}} />
                        </div>}
                      </div>
                      <div style={{ display:"grid", gridTemplateColumns:"1fr 1fr 1fr", gap:10 }}>
                        <div>
                          <span style={labelSt}>Description</span>
                          <input value={editData.desc} onChange={e=>setEditData(d=>({...d,desc:e.target.value}))} style={inputSt} />
                        </div>
                        <div>
                          <span style={labelSt}>Price</span>
                          <input type="number" value={editData.price} onChange={e=>setEditData(d=>({...d,price:e.target.value}))} style={inputSt} />
                        </div>
                        <div>
                          <span style={labelSt}>HH Price</span>
                          <input type="number" value={editData.hhPrice} onChange={e=>setEditData(d=>({...d,hhPrice:e.target.value}))} style={inputSt} />
                        </div>
                      </div>
                      <div style={{ display:"flex", gap:8, justifyContent:"flex-end" }}>
                        <button className="btn" onClick={cancelEdit}
                          style={{ padding:"7px 16px", borderRadius:7, background:V.elevated,
                            color:V.muted, fontSize:13, fontWeight:600, cursor:"pointer" }}>Cancel</button>
                        <button className="btn" onClick={saveItem}
                          style={{ padding:"7px 16px", borderRadius:7, background:V.amber,
                            color:"#000", fontSize:13, fontWeight:700, cursor:"pointer" }}>Save & Sync</button>
                      </div>
                    </div>
                  ) : (
                    <div style={{ display:"flex", alignItems:"center", gap:10 }}>
                      <div style={{ flex:1, minWidth:0 }}>
                        <div style={{ display:"flex", alignItems:"center", gap:7 }}>
                          <span style={{ fontWeight:600, fontSize:14 }}>{item.name}</span>
                          {bilingual && <span style={{ fontFamily:"'Noto Sans SC'", fontSize:12, color:V.muted }}>{item.nameZh}</span>}
                          {item.tags?.map(t=>(
                            <span key={t} style={{ fontSize:9, padding:"1px 5px", borderRadius:3,
                              background:V.amberSoft, color:V.amber, fontWeight:700, border:`1px solid ${V.amberBord}` }}>{t}</span>
                          ))}
                          {item.qty && <span style={{ fontSize:9, padding:"1px 5px", borderRadius:3,
                            background:"#ef444415", color:"#ef4444", fontWeight:700 }}>⚠ {item.qty} left</span>}
                        </div>
                        <div style={{ fontSize:12, color:V.muted }}>{item.desc}</div>
                      </div>
                      <div style={{ textAlign:"right", minWidth:70 }}>
                        <div style={{ fontWeight:700, color:V.amber }}>{fmt(item.price)}</div>
                        {isHH && <div style={{ fontSize:11, color:`${V.amber}80` }}>HH: {fmt(item.hhPrice)}</div>}
                      </div>
                      <div style={{ display:"flex", gap:5 }}>
                        <button className="btn" onClick={()=>toggleAvail(item.id)}
                          style={{ fontSize:11, padding:"3px 9px", borderRadius:5, cursor:"pointer",
                            background: item.available ? "#22c55e20" : "#ef444420",
                            color: item.available ? "#22c55e" : "#ef4444", border:"none", fontWeight:600 }}>
                          {item.available ? "Live" : "Off"}
                        </button>
                        <button className="btn" onClick={()=>startEdit(item)}
                          style={{ background:"transparent", border:"none", color:V.muted, cursor:"pointer", fontSize:14 }}>✏️</button>
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}

/* ── QUICK UPDATE PANEL ──────────────────────────────────────────────────── */
function QuickUpdate({ sections, setSections, toast }) {
  const [special, setSpecial] = useState("");
  const allItems = sections.flatMap(s => s.items);

  const toggle = (id) => {
    setSections(secs => secs.map(s => ({
      ...s, items: s.items.map(i => i.id===id ? {...i,available:!i.available} : i)
    })));
    toast("Updated · Live on screens");
  };

  return (
    <div style={{ display:"flex", flexDirection:"column", gap:16 }}>
      {/* Daily special */}
      <div style={{ background:V.surface, borderRadius:12, padding:"16px",
        border:`1px solid ${V.border}` }}>
        <div style={{ fontSize:12, fontWeight:700, color:V.amber, textTransform:"uppercase",
          letterSpacing:"0.07em", marginBottom:10 }}>📢 Today's Special</div>
        <textarea value={special} onChange={e=>setSpecial(e.target.value)}
          placeholder="e.g. Soup of the day: Hot & Sour · $5.99"
          style={{...inputSt, height:64, resize:"none", lineHeight:1.5}} />
        <button className="btn" onClick={()=>{ if(special.trim()) { toast("Special pushed to all screens!"); }}}
          style={{ marginTop:10, width:"100%", padding:"10px", borderRadius:8,
            background: special.trim() ? V.amber : V.elevated,
            color: special.trim() ? "#000" : V.muted,
            fontWeight:700, fontSize:13, cursor:"pointer", border:"none", transition:"all 0.2s" }}>
          Push to Screens
        </button>
      </div>

      {/* 86 board */}
      <div style={{ background:V.surface, borderRadius:12, padding:"16px",
        border:`1px solid ${V.border}` }}>
        <div style={{ fontSize:12, fontWeight:700, color:V.muted, textTransform:"uppercase",
          letterSpacing:"0.07em", marginBottom:10 }}>⚡ Quick 86 / Restore</div>
        <div style={{ display:"flex", flexDirection:"column", gap:6 }}>
          {allItems.map(item => (
            <div key={item.id} style={{ display:"flex", alignItems:"center", gap:10,
              padding:"8px 10px", borderRadius:8,
              background: item.available ? "transparent" : "#ef444410",
              border: `1px solid ${item.available ? V.border : "#ef444430"}` }}>
              <div style={{ flex:1 }}>
                <div style={{ fontSize:13, fontWeight:600, color:V.text }}>{item.name}</div>
                <div style={{ fontSize:11, color:V.muted }}>{fmt(item.price)}</div>
              </div>
              {item.qty && (
                <div style={{ fontSize:10, color:"#ef4444", fontWeight:700, marginRight:4 }}>
                  {item.qty} left
                </div>
              )}
              <Toggle value={item.available} onChange={()=>toggle(item.id)}
                color={item.available ? V.sage : "#ef4444"} />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

/* ── SCREENS PANEL ───────────────────────────────────────────────────────── */
function ScreensPanel({ toast }) {
  return (
    <div style={{ display:"flex", flexDirection:"column", gap:10 }}>
      {SCREENS_DATA.map(s => (
        <div key={s.id} style={{ background:V.surface, borderRadius:12, padding:"16px 18px",
          border:`1px solid ${V.border}`, display:"flex", alignItems:"center", gap:14 }}>
          <div style={{ width:9, height:9, borderRadius:"50%",
            background: s.status==="online" ? V.sage : "#ef4444",
            boxShadow: s.status==="online" ? `0 0 7px ${V.sage}` : "none" }}
            className={s.status==="online" ? "pulse-dot" : ""} />
          <div style={{ flex:1 }}>
            <div style={{ fontWeight:600, fontSize:14 }}>{s.name}
              {s.wallGroup && <span style={{ marginLeft:8, fontSize:10, padding:"2px 7px",
                borderRadius:4, background:V.amberSoft, color:V.amber,
                border:`1px solid ${V.amberBord}`, fontWeight:700 }}>WALL</span>}
            </div>
            <div style={{ fontSize:12, color:V.muted }}>{s.location} · Last seen: {s.lastSeen}</div>
          </div>
          <div style={{ fontSize:11, padding:"3px 9px", borderRadius:5,
            background: s.status==="online" ? "#22c55e15" : "#ef444415",
            color: s.status==="online" ? V.sage : "#ef4444",
            fontWeight:700, textTransform:"uppercase", letterSpacing:"0.06em" }}>
            {s.status}
          </div>
          <button className="btn" onClick={()=>toast(`Content pushed to ${s.name}`)}
            style={{ padding:"7px 14px", borderRadius:7, background:V.elevated,
              color:V.textSoft, fontSize:12, fontWeight:600, cursor:"pointer", border:"none" }}>
            Push
          </button>
        </div>
      ))}
      {/* Wall info */}
      <div style={{ padding:"14px 16px", borderRadius:10, background:V.amberSoft,
        border:`1px solid ${V.amberBord}`, fontSize:12, color:V.muted }}>
        <strong style={{color:V.amber}}>🖥🖥 Video Wall:</strong> Bar Counter screens are linked as a 2×1 wall group. Both TVs show a single canvas spanning both displays, synced via SignalR.
      </div>
      <button className="btn" onClick={()=>toast("Registration link copied!")}
        style={{ padding:"12px", borderRadius:10, background:V.surface,
          border:`1px dashed ${V.border}`, color:V.muted, fontSize:13,
          cursor:"pointer", fontWeight:500 }}>+ Register New Screen</button>
    </div>
  );
}

/* ── MEAL PERIODS PANEL ──────────────────────────────────────────────────── */
function MealPeriods({ toast }) {
  const [periods, setPeriods] = useState(MEAL_PERIODS);
  const toggle = (id) => setPeriods(p => p.map(m => ({...m, active: m.id===id ? !m.active : m.active})));

  return (
    <div style={{ display:"flex", flexDirection:"column", gap:10 }}>
      <div style={{ fontSize:12, color:V.muted, marginBottom:4 }}>
        Menus auto-switch based on time of day. Set the window for each meal period and assign a different layout or menu to each.
      </div>
      {periods.map(p => (
        <div key={p.id} style={{ background:V.surface, borderRadius:12, padding:"14px 16px",
          border:`1px solid ${p.active ? p.color+"40" : V.border}`,
          boxShadow: p.active ? `0 0 12px ${p.color}15` : "none", transition:"all 0.2s" }}>
          <div style={{ display:"flex", alignItems:"center", gap:12 }}>
            <span style={{ fontSize:20 }}>{p.icon}</span>
            <div style={{ flex:1 }}>
              <div style={{ fontWeight:600, fontSize:14, color: p.active ? p.color : V.text }}>{p.name}</div>
              <div style={{ fontFamily:"'DM Mono'", fontSize:12, color:V.muted }}>{p.start} – {p.end}</div>
            </div>
            <div style={{ fontSize:11, padding:"3px 9px", borderRadius:5,
              background: p.active ? `${p.color}20` : V.elevated,
              color: p.active ? p.color : V.muted,
              fontWeight:700, marginRight:8 }}>
              {p.active ? "ACTIVE" : "Inactive"}
            </div>
            <Toggle value={p.active} onChange={()=>toggle(p.id)} color={p.color} />
          </div>
        </div>
      ))}
      <button className="btn" onClick={()=>toast("Meal periods saved & synced!")}
        style={{ padding:"12px", borderRadius:10, background:V.amber, color:"#000",
          fontWeight:700, fontSize:13, border:"none", cursor:"pointer" }}>
        Save Schedule
      </button>
    </div>
  );
}

/* ── MAIN APP ────────────────────────────────────────────────────────────── */
const NAV_ITEMS = [
  { id:"quick",    icon:"⚡", label:"Quick Update" },
  { id:"menu",     icon:"🍽", label:"Menu Editor"  },
  { id:"schedule", icon:"🕐", label:"Meal Periods" },
  { id:"screens",  icon:"📺", label:"Screens"      },
];

const DISPLAY_MODES = [
  { id:"chalkboard", label:"🪩 Chalkboard", desc:"Bars & restaurants" },
  { id:"photo",      label:"📸 Photo Grid", desc:"QSR & Asian cuisine" },
  { id:"diner",      label:"📋 Classic",    desc:"Diners & cafes"     },
];

export default function Vennu() {
  const [sections, setSections]     = useState(MENU_DATA.sections);
  const [navTab, setNavTab]         = useState("quick");
  const [displayMode, setDisplayMode] = useState("chalkboard");
  const [isHH, setIsHH]             = useState(false);
  const [bilingual, setBilingual]   = useState(true);
  const [venueName, setVenueName]   = useState("Golden Dragon");
  const [venueType, setVenueType]   = useState("restaurant");
  const [showPreview, setShowPreview] = useState(false);
  const [toast, setToast]           = useState(null);
  const [previewScale, setPreviewScale] = useState(0.5);
  const previewRef = useRef(null);

  const showToast = useCallback((msg) => setToast(msg), []);

  useEffect(() => {
    const calc = () => {
      if (!previewRef.current) return;
      const w = previewRef.current.clientWidth - 48;
      setPreviewScale(Math.min(0.62, w / 960));
    };
    calc();
    window.addEventListener("resize", calc);
    return () => window.removeEventListener("resize", calc);
  }, []);

  const DisplayComponent = displayMode === "chalkboard" ? ChalkboardDisplay
    : displayMode === "photo" ? PhotoDisplay : DinerDisplay;

  return (
    <>
      <style>{STYLES}</style>
      {showPreview ? (
        /* ── Full Display Preview ── */
        <div style={{ width:"100vw", height:"100vh", background:"#000",
          display:"flex", flexDirection:"column" }}>
          <div style={{ background:V.surface, borderBottom:`1px solid ${V.border}`,
            padding:"10px 20px", display:"flex", alignItems:"center", gap:12, flexShrink:0 }}>
            <button className="btn" onClick={()=>setShowPreview(false)}
              style={{ padding:"6px 14px", borderRadius:7, background:V.elevated,
                color:V.amber, border:`1px solid ${V.amberBord}`, fontSize:12,
                fontWeight:600, cursor:"pointer" }}>← Admin</button>
            <div style={{ fontSize:12, color:V.muted }}>Display Preview · {venueName}</div>
            <div style={{ marginLeft:"auto", display:"flex", gap:8 }}>
              {DISPLAY_MODES.map(m => (
                <button key={m.id} className="btn" onClick={()=>setDisplayMode(m.id)}
                  style={{ padding:"5px 12px", borderRadius:6, fontSize:12, fontWeight:600,
                    cursor:"pointer", border:"none",
                    background: displayMode===m.id ? V.amber : V.elevated,
                    color: displayMode===m.id ? "#000" : V.muted }}>
                  {m.label}
                </button>
              ))}
            </div>
          </div>
          <div style={{ flex:1 }}>
            <DisplayComponent sections={sections} venueName={venueName}
              isHH={isHH} bilingual={bilingual} />
          </div>
        </div>
      ) : (
        /* ── Admin Dashboard ── */
        <div style={{ display:"flex", height:"100vh", overflow:"hidden", background:V.bg }}>

          {/* Sidebar */}
          <div style={{ width:220, background:V.surface, borderRight:`1px solid ${V.border}`,
            display:"flex", flexDirection:"column", flexShrink:0 }}>
            {/* Logo */}
            <div style={{ padding:"20px 20px 16px", borderBottom:`1px solid ${V.border}` }}>
              <div style={{ fontFamily:"'Playfair Display'", fontWeight:700, fontSize:26,
                color:V.amber, letterSpacing:"-0.01em", lineHeight:1 }}>vennu</div>
              <div style={{ fontSize:10, color:V.muted, marginTop:3,
                letterSpacing:"0.1em", textTransform:"uppercase" }}>Every venue · Every menu</div>
            </div>

            {/* Venue identity */}
            <div style={{ padding:"14px 16px", borderBottom:`1px solid ${V.border}` }}>
              <input value={venueName} onChange={e=>setVenueName(e.target.value)}
                style={{ ...inputSt, fontSize:14, fontWeight:600, padding:"7px 10px",
                  fontFamily:"'Playfair Display'" }} />
              <div style={{ display:"flex", flexWrap:"wrap", gap:4, marginTop:8 }}>
                {VENUE_TYPES.map(vt => (
                  <div key={vt.id} onClick={()=>setVenueType(vt.id)}
                    style={{ padding:"3px 8px", borderRadius:5, cursor:"pointer", fontSize:11,
                      fontWeight:600, transition:"all 0.15s",
                      background: venueType===vt.id ? V.amberSoft : "transparent",
                      border: `1px solid ${venueType===vt.id ? V.amberBord : V.border}`,
                      color: venueType===vt.id ? V.amber : V.muted }}>
                    {vt.icon} {vt.label}
                  </div>
                ))}
              </div>
            </div>

            {/* Nav */}
            <div style={{ padding:"10px 10px", flex:1 }}>
              {NAV_ITEMS.map(n => (
                <button key={n.id} className="nav-link btn" onClick={()=>setNavTab(n.id)}
                  style={{ display:"flex", alignItems:"center", gap:10, width:"100%",
                    padding:"10px 12px", borderRadius:8, border:"none", cursor:"pointer",
                    background: navTab===n.id ? V.amberSoft : "transparent",
                    borderLeft: navTab===n.id ? `2px solid ${V.amber}` : "2px solid transparent",
                    color: navTab===n.id ? V.amber : V.muted,
                    fontFamily:"'Outfit'", fontWeight:navTab===n.id ? 600 : 400, fontSize:13,
                    marginBottom:2, textAlign:"left" }}>
                  <span style={{ fontSize:15 }}>{n.icon}</span> {n.label}
                </button>
              ))}
            </div>

            {/* Settings strip */}
            <div style={{ padding:"12px 16px", borderTop:`1px solid ${V.border}`,
              display:"flex", flexDirection:"column", gap:10 }}>
              <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between" }}>
                <span style={{ fontSize:12, color:V.muted }}>🍺 Happy Hour</span>
                <Toggle value={isHH} onChange={setIsHH} />
              </div>
              <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between" }}>
                <span style={{ fontSize:12, color:V.muted }}>🌏 Bilingual</span>
                <Toggle value={bilingual} onChange={setBilingual} color={V.sky} />
              </div>
              <button className="btn" onClick={()=>setShowPreview(true)}
                style={{ padding:"9px", borderRadius:8, background:V.amber, color:"#000",
                  fontWeight:700, fontSize:13, border:"none", cursor:"pointer", marginTop:4,
                  boxShadow:`0 0 14px ${V.amber}40` }}>
                📺 Preview Display
              </button>
            </div>
          </div>

          {/* Main content */}
          <div style={{ flex:1, display:"flex", overflow:"hidden" }}>
            {/* Panel */}
            <div style={{ width:420, borderRight:`1px solid ${V.border}`,
              display:"flex", flexDirection:"column", overflow:"hidden" }}>
              <div style={{ padding:"20px 22px 16px", borderBottom:`1px solid ${V.border}`, flexShrink:0 }}>
                <h2 style={{ fontFamily:"'Playfair Display'", fontSize:20, fontWeight:600 }}>
                  {NAV_ITEMS.find(n=>n.id===navTab)?.label}
                </h2>
                <p style={{ fontSize:12, color:V.muted, marginTop:3 }}>
                  {navTab==="quick" && "Fast updates from any device — built for busy service"}
                  {navTab==="menu" && "Edit items · changes sync to screens in ~200ms via SignalR"}
                  {navTab==="schedule" && "Auto-switch menus by time of day — set once, runs forever"}
                  {navTab==="screens" && "Connected displays and video wall groups"}
                </p>
                {bilingual && navTab==="menu" && (
                  <div style={{ marginTop:8, padding:"6px 10px", borderRadius:6,
                    background:V.amberSoft, border:`1px solid ${V.amberBord}`,
                    fontSize:11, color:V.amber, display:"flex", alignItems:"center", gap:6 }}>
                    🌏 Bilingual mode · Showing English + 中文
                  </div>
                )}
              </div>
              <div style={{ flex:1, overflowY:"auto", padding:"16px 22px" }}>
                {navTab==="quick"    && <QuickUpdate sections={sections} setSections={setSections} toast={showToast} />}
                {navTab==="menu"     && <MenuEditor sections={sections} setSections={setSections} isHH={isHH} bilingual={bilingual} toast={showToast} />}
                {navTab==="schedule" && <MealPeriods toast={showToast} />}
                {navTab==="screens"  && <ScreensPanel toast={showToast} />}
              </div>
              {/* Push all */}
              <div style={{ padding:"14px 22px", borderTop:`1px solid ${V.border}`, flexShrink:0 }}>
                <button className="btn" onClick={()=>showToast("Pushed to all screens!")}
                  style={{ width:"100%", padding:"12px", borderRadius:10, background:V.amber,
                    color:"#000", fontWeight:700, fontSize:14, border:"none", cursor:"pointer",
                    boxShadow:`0 0 16px ${V.amber}50` }}>
                  Push to All Screens
                </button>
                <div style={{ textAlign:"center", fontSize:11, color:V.muted, marginTop:6 }}>
                  {SCREENS_DATA.filter(s=>s.status==="online").length} screens online · SignalR connected
                </div>
              </div>
            </div>

            {/* Live preview pane */}
            <div ref={previewRef} style={{ flex:1, display:"flex", flexDirection:"column",
              background:V.bg, overflow:"hidden" }}>
              {/* Preview header */}
              <div style={{ padding:"14px 24px", borderBottom:`1px solid ${V.border}`,
                display:"flex", alignItems:"center", gap:12, flexShrink:0 }}>
                <div style={{ width:7, height:7, borderRadius:"50%", background:V.sage,
                  boxShadow:`0 0 6px ${V.sage}` }} className="pulse-dot" />
                <span style={{ fontSize:13, fontWeight:500, color:V.muted }}>Live Preview</span>
                <div style={{ marginLeft:"auto", display:"flex", gap:6 }}>
                  {DISPLAY_MODES.map(m => (
                    <button key={m.id} className="btn" onClick={()=>setDisplayMode(m.id)}
                      style={{ padding:"5px 11px", borderRadius:6, fontSize:11, fontWeight:600,
                        cursor:"pointer", border:`1px solid ${displayMode===m.id ? V.amberBord : V.border}`,
                        background: displayMode===m.id ? V.amberSoft : V.elevated,
                        color: displayMode===m.id ? V.amber : V.muted }}>
                      {m.label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Preview canvas */}
              <div style={{ flex:1, display:"flex", alignItems:"center", justifyContent:"center",
                padding:24, overflow:"hidden" }}>
                <div style={{ width:960*previewScale, height:540*previewScale,
                  borderRadius:10, overflow:"hidden", flexShrink:0,
                  boxShadow:`0 20px 60px rgba(0,0,0,0.7), 0 0 0 1px ${V.border}` }}>
                  <div style={{ width:960, height:540, transform:`scale(${previewScale})`,
                    transformOrigin:"top left" }}>
                    <DisplayComponent sections={sections} venueName={venueName}
                      isHH={isHH} bilingual={bilingual} />
                  </div>
                </div>
              </div>

              {/* Status bar */}
              <div style={{ padding:"10px 24px", borderTop:`1px solid ${V.border}`,
                display:"flex", gap:16, alignItems:"center", flexShrink:0 }}>
                <div style={{ display:"flex", gap:12 }}>
                  <span style={{ fontSize:11, color:V.muted }}>Layout:</span>
                  <span style={{ fontSize:11, color:V.amber, fontWeight:600 }}>
                    {DISPLAY_MODES.find(m=>m.id===displayMode)?.desc}
                  </span>
                </div>
                <div style={{ width:1, height:14, background:V.border }} />
                <div style={{ display:"flex", gap:12 }}>
                  <span style={{ fontSize:11, color:V.muted }}>Bilingual:</span>
                  <span style={{ fontSize:11, color: bilingual ? V.sky : V.muted, fontWeight:600 }}>
                    {bilingual ? "EN + 中文" : "English only"}
                  </span>
                </div>
                <div style={{ width:1, height:14, background:V.border }} />
                <div style={{ display:"flex", gap:12 }}>
                  <span style={{ fontSize:11, color:V.muted }}>Happy Hour:</span>
                  <span style={{ fontSize:11, color: isHH ? V.amber : V.muted, fontWeight:600 }}>
                    {isHH ? "Active" : "Off"}
                  </span>
                </div>
                <div style={{ marginLeft:"auto", fontFamily:"'DM Mono'",
                  fontSize:11, color:V.muted }}>1920×1080 · {Math.round(previewScale*100)}%</div>
              </div>
            </div>
          </div>
        </div>
      )}
      {toast && <Toast msg={toast} onDone={()=>setToast(null)} />}
    </>
  );
}
