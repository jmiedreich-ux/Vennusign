import { useState, useEffect } from "react";

const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Permanent+Marker&family=Caveat:wght@400;600;700&family=Kalam:wght@400;700&family=Pacifico&family=Righteous&family=Bungee&family=DM+Sans:wght@300;400;500;600;700&family=Syne:wght@700;800&family=DM+Mono:wght@400;500&display=swap');
  *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
  body { background:#111; font-family:'DM Sans',sans-serif; color:#e8e3db; }
  ::-webkit-scrollbar { width:3px; } ::-webkit-scrollbar-track { background:transparent; }
  ::-webkit-scrollbar-thumb { background:#333; border-radius:2px; }
  input,select,textarea,button { font-family:'DM Sans',sans-serif; }

  @keyframes fadeUp    { from{opacity:0;transform:translateY(10px)} to{opacity:1;transform:translateY(0)} }
  @keyframes chalkDraw { from{clip-path:inset(0 100% 0 0)} to{clip-path:inset(0 0% 0 0)} }
  @keyframes flicker   { 0%,19%,21%,23%,25%,54%,56%,100%{opacity:1} 20%,24%,55%{opacity:.8} }
  @keyframes glowBreathe { 0%,100%{filter:brightness(1)} 50%{filter:brightness(1.25)} }
  @keyframes toastIn   { from{opacity:0;transform:translateY(14px)} to{opacity:1;transform:translateY(0)} }
  @keyframes pulse     { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.6;transform:scale(1.2)} }
  @keyframes tapSlide  { from{opacity:0;transform:translateX(-20px)} to{opacity:1;transform:translateX(0)} }
  @keyframes woodGrain { 0%{background-position:0 0} 100%{background-position:0 100%} }

  .chalk-title { animation: flicker 6s infinite, glowBreathe 3s ease-in-out infinite; }
  .chalk-item  { animation: chalkDraw 0.45s ease forwards; }
  .tap-strip   { animation: tapSlide 0.4s ease forwards; }
  .fade-up     { animation: fadeUp 0.35s ease forwards; }
  .pulse-dot   { animation: pulse 2s ease-in-out infinite; }
  .btn         { transition:all 0.15s ease; cursor:pointer; border:none; outline:none; }
  .btn:active  { transform:scale(0.97); }
  .nav-btn     { transition:all 0.15s ease; }
`;

/* ── Tap data ──────────────────────────────────────────────────────────── */
const TAPS = [
  { id:"t1",  name:"480B",          style:"West Coast IPA",    abv:8.2,  ibu:65, color:"#d4a832", desc:"Crisp, piney, and aggressively hopped. Named after the brewery's original address.",       price:7.00, nameColor:"#ffd700",  available:true  },
  { id:"t2",  name:"Boom Box",      style:"Rotating Fruited Sour", abv:5.5, ibu:12, color:"#e87bff", desc:"Tart, juicy, and effervescent. This week: passion fruit and mango.",                    price:8.00, nameColor:"#bf5fff",  available:true  },
  { id:"t3",  name:"Lolli",         style:"Belgian Blonde",    abv:8.2,  ibu:22, color:"#f0c060", desc:"Light, golden, and deceptively strong. Honey and clove notes with a dry finish.",          price:7.00, nameColor:"#ffd700",  available:true  },
  { id:"t4",  name:"PRIM",          style:"Hazy Peach IPA",    abv:5.7,  ibu:35, color:"#ffb347", desc:"Soft and hazy with fresh peach on the nose. Low bitterness, high drinkability.",           price:7.50, nameColor:"#ff6ec7",  available:true  },
  { id:"t5",  name:"Mornin' Sex",   style:"Coffee Stout",      abv:5.8,  ibu:30, color:"#3d1c02", desc:"Rich roasted coffee with a velvet finish. Brewed with locally sourced cold brew.",         price:7.00, nameColor:"#c084fc",  available:true  },
  { id:"t6",  name:"Bubble Water",  style:"Hard Seltzer",      abv:6.5,  ibu:0,  color:"#e8f4fd", desc:"Clean, crisp, and crushable. Hint of lime. For when you want something light.",            price:6.00, nameColor:"#7dd3fc",  available:true  },
  { id:"t7",  name:"Urban Art",     style:"New England IPA",   abv:6.5,  ibu:40, color:"#f5c842", desc:"Soft, tropical, and unfiltered. Stone fruit and citrus with a pillowy mouthfeel.",         price:7.50, nameColor:"#4ade80",  available:true  },
  { id:"t8",  name:"¡Vamonos!",     style:"Lager with Lime",   abv:4.2,  ibu:10, color:"#c8e86a", desc:"Light and refreshing Mexican-style lager. Squeeze of lime baked right in.",               price:6.00, nameColor:"#facc15",  available:true  },
  { id:"t9",  name:"Cyril Figgis",  style:"Imperial Red",      abv:8.8,  ibu:55, color:"#8b1a1a", desc:"Bold, malt-forward, and deeply red. Caramel backbone with a warming finish.",              price:8.50, nameColor:"#fca5a5",  available:false },
  { id:"t10", name:"Rice Krispy Bois", style:"Rotating Craft Cola", abv:0, ibu:0, color:"#c8a45a", desc:"Non-alcoholic house-made craft cola. Rotating flavors, always interesting.",              price:5.00, nameColor:"#fde68a",  available:true  },
];

const COCKTAILS = [
  { id:"c1",  name:"Cosmopolitan",    col:0 }, { id:"c2",  name:"Mojito",          col:0 },
  { id:"c3",  name:"Sidecar",         col:0 }, { id:"c4",  name:"Martini",         col:0 },
  { id:"c5",  name:"Kamikaze",        col:0 }, { id:"c6",  name:"Manhattan",       col:0 },
  { id:"c7",  name:"Old Fashioned",   col:0 }, { id:"c8",  name:"Margarita",       col:0 },
  { id:"c9",  name:"Sex On The Beach",col:0 }, { id:"c10", name:"Gin Sling",        col:1 },
  { id:"c11", name:"Black Russian",   col:1 }, { id:"c12", name:"Mai Tai",          col:1 },
  { id:"c13", name:"Screwdriver",     col:1 }, { id:"c14", name:"Piña Colada",      col:1 },
  { id:"c15", name:"Pink Lady",       col:1 }, { id:"c16", name:"Long Island",      col:1 },
  { id:"c17", name:"Blue Hawaii",     col:1 },
];

const IMPORT_BEERS = ["Singha","Sapporo","Tsingtao","Heineken","Corona Extra"];
const DOMESTIC_BEERS = ["Bud Light","Coors Light","Michelob Ultra","Lite","Yuengling Lager"];

const CHALK_NOISE = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='400'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.75' numOctaves='4' stitchTiles='stitch'/%3E%3CfeColorMatrix type='saturate' values='0'/%3E%3C/filter%3E%3Crect width='400' height='400' filter='url(%23n)' opacity='0.05'/%3E%3C/svg%3E")`;

const WOOD_NOISE = `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='400'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.02 0.8' numOctaves='3' stitchTiles='stitch'/%3E%3CfeColorMatrix type='saturate' values='0.3'/%3E%3C/filter%3E%3Crect width='400' height='400' filter='url(%23n)' opacity='0.18'/%3E%3C/svg%3E")`;

/* ── Beer glass SVG illustration ─────────────────────────────────────── */
function BeerGlass({ color = "#f5c842", size = 60, style: beerStyle = "" }) {
  const isStout = beerStyle.toLowerCase().includes("stout");
  const isSour  = beerStyle.toLowerCase().includes("sour");
  const isLight = beerStyle.toLowerCase().includes("seltzer") || beerStyle.toLowerCase().includes("lager");
  const liquidColor = isStout ? "#2a0a00" : isSour ? "#e87bff" : isLight ? "#e8f4fd" : color;
  const foamColor = "#f5f0e8";

  return (
    <svg width={size} height={size * 1.4} viewBox="0 0 60 84" fill="none" xmlns="http://www.w3.org/2000/svg">
      {/* Glass outline */}
      <path d="M12 10 L8 78 L52 78 L48 10 Z" fill={`${liquidColor}cc`} stroke="rgba(255,255,255,0.3)" strokeWidth="1.5"/>
      {/* Foam */}
      <ellipse cx="30" cy="10" rx="18" ry="6" fill={foamColor} opacity="0.9"/>
      <ellipse cx="22" cy="8"  rx="7"  ry="5" fill={foamColor} opacity="0.8"/>
      <ellipse cx="38" cy="8"  rx="7"  ry="5" fill={foamColor} opacity="0.8"/>
      <ellipse cx="30" cy="6"  rx="5"  ry="4" fill={foamColor} opacity="0.7"/>
      {/* Handle */}
      <path d="M48 20 Q60 20 60 35 Q60 50 48 50" stroke="rgba(255,255,255,0.4)" strokeWidth="3" fill="none"/>
      {/* Highlight */}
      <path d="M15 20 L14 60" stroke="rgba(255,255,255,0.2)" strokeWidth="3" strokeLinecap="round"/>
    </svg>
  );
}

function CocktailGlass({ color = "#ff6eb4", size = 60 }) {
  return (
    <svg width={size} height={size * 1.2} viewBox="0 0 60 72" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M5 8 L30 45 L55 8 Z" fill={`${color}99`} stroke="rgba(255,255,255,0.3)" strokeWidth="1.5"/>
      <line x1="30" y1="45" x2="30" y2="65" stroke="rgba(255,255,255,0.4)" strokeWidth="2"/>
      <line x1="18" y1="65" x2="42" y2="65" stroke="rgba(255,255,255,0.4)" strokeWidth="2"/>
      <ellipse cx="30" cy="10" rx="20" ry="4" fill={`${color}60`}/>
      {/* Straw */}
      <line x1="38" y1="5" x2="28" y2="48" stroke="#ff9999" strokeWidth="1.5" strokeLinecap="round"/>
      {/* Umbrella */}
      <ellipse cx="38" cy="5" rx="8" ry="3" fill="#ff6eb4" opacity="0.8"/>
    </svg>
  );
}

/* ══════════════════════════════════════════════════════════════════════
   DISPLAY 1: CLASSIC CHALKBOARD (Image 1 style)
   ══════════════════════════════════════════════════════════════════════ */
function ClassicChalkboard({ venueName }) {
  const [time, setTime] = useState(new Date());
  useEffect(()=>{ const t=setInterval(()=>setTime(new Date()),1000); return()=>clearInterval(t); },[]);

  const left  = COCKTAILS.filter(c=>c.col===0);
  const right = COCKTAILS.filter(c=>c.col===1);

  return (
    <div style={{ width:"100%", height:"100%", position:"relative", overflow:"hidden",
      background:"#1a1f1a",
      backgroundImage:`${CHALK_NOISE}, radial-gradient(ellipse at 20% 80%, #1f2a1f 0%, #1a1f1a 60%)` }}>

      {/* Chalkboard frame lines */}
      <div style={{ position:"absolute", inset:6, border:"2px solid rgba(255,255,255,0.06)",
        borderRadius:4, pointerEvents:"none" }} />
      <div style={{ position:"absolute", inset:10, border:"1px solid rgba(255,255,255,0.04)",
        borderRadius:3, pointerEvents:"none" }} />

      {/* Scanlines */}
      <div style={{ position:"absolute", inset:0, pointerEvents:"none", zIndex:8,
        backgroundImage:"repeating-linear-gradient(to bottom,transparent 0px,transparent 3px,rgba(0,0,0,0.04) 3px,rgba(0,0,0,0.04) 4px)" }} />

      <div style={{ display:"flex", height:"100%", padding:"16px 20px", gap:20 }}>

        {/* LEFT — Cocktails */}
        <div style={{ flex:1.4, display:"flex", flexDirection:"column" }}>
          {/* "Drinks" title */}
          <div className="chalk-title" style={{ marginBottom:10 }}>
            <span style={{ fontFamily:"'Pacifico',cursive", fontSize:62, color:"#5bb8ff",
              textShadow:"0 0 12px #5bb8ffaa, 0 0 30px #5bb8ff66, 2px 2px 0 rgba(0,0,0,0.3)",
              lineHeight:1 }}>Drinks</span>
          </div>

          {/* COCKTAILS header */}
          <div style={{ display:"inline-flex", alignItems:"center", gap:10, marginBottom:8 }}>
            <div style={{ height:1, width:12, background:"rgba(255,255,255,0.3)" }} />
            <span style={{ fontFamily:"'Kalam',cursive", fontSize:16, fontWeight:700,
              color:"rgba(255,255,255,0.6)", letterSpacing:"0.15em" }}>COCKTAILS</span>
            <div style={{ height:1, flex:1, background:"rgba(255,255,255,0.3)" }} />
          </div>

          {/* Price box */}
          <div style={{ display:"inline-block", marginBottom:14 }}>
            <div style={{ border:"2px solid rgba(255,255,255,0.4)", borderRadius:4,
              padding:"4px 16px", display:"inline-block",
              boxShadow:"inset 0 0 0 1px rgba(255,255,255,0.1)" }}>
              <span style={{ fontFamily:"'Caveat',cursive", fontSize:32, fontWeight:700,
                color:"rgba(255,255,255,0.85)" }}>10.95</span>
            </div>
          </div>

          {/* Two columns of cocktails */}
          <div style={{ display:"flex", gap:0, flex:1 }}>
            <div style={{ flex:1, display:"flex", flexDirection:"column", gap:6 }}>
              {left.map((c,i) => (
                <div key={c.id} className="chalk-item"
                  style={{ animationDelay:`${i*50}ms`, fontFamily:"'Caveat',cursive",
                    fontSize:18, fontWeight:600, color:"#d4a853",
                    textShadow:"0 1px 3px rgba(0,0,0,0.5)" }}>{c.name}</div>
              ))}
            </div>
            <div style={{ flex:1, display:"flex", flexDirection:"column", gap:6 }}>
              {right.map((c,i) => (
                <div key={c.id} className="chalk-item"
                  style={{ animationDelay:`${(i+9)*50}ms`, fontFamily:"'Caveat',cursive",
                    fontSize:18, fontWeight:600, color:"#d4a853",
                    textShadow:"0 1px 3px rgba(0,0,0,0.5)" }}>{c.name}</div>
              ))}
            </div>
          </div>
        </div>

        {/* CENTER — Chalk illustrations */}
        <div style={{ width:180, display:"flex", flexDirection:"column",
          alignItems:"center", justifyContent:"flex-start", paddingTop:8, gap:4 }}>
          <div style={{ display:"flex", gap:8, alignItems:"flex-end" }}>
            <CocktailGlass color="#ff6eb4" size={55} />
            <CocktailGlass color="#ff8c42" size={65} />
            <CocktailGlass color="#7bdb8e" size={50} />
          </div>
          <BeerGlass color="#d4a832" size={52} beerStyle="lager" />
        </div>

        {/* RIGHT — Beer sections */}
        <div style={{ flex:1, display:"flex", flexDirection:"column", gap:0 }}>
          {/* Import Beer */}
          <div style={{ marginBottom:18 }}>
            <div style={{ fontFamily:"'Kalam',cursive", fontSize:20, fontWeight:700,
              color:"#5bb8ff", letterSpacing:"0.1em", marginBottom:6,
              textShadow:"0 0 10px #5bb8ff88" }}>IMPORT BEER</div>
            <div style={{ border:"2px solid rgba(255,255,255,0.35)", borderRadius:4,
              padding:"4px 14px", display:"inline-block", marginBottom:10 }}>
              <span style={{ fontFamily:"'Caveat',cursive", fontSize:30, fontWeight:700,
                color:"rgba(255,255,255,0.85)" }}>4.00</span>
            </div>
            <div style={{ display:"flex", flexDirection:"column", gap:4 }}>
              {IMPORT_BEERS.map(b => (
                <div key={b} style={{ fontFamily:"'Caveat',cursive", fontSize:17, fontWeight:600,
                  color:"#d4a853", textShadow:"0 1px 2px rgba(0,0,0,0.5)" }}>{b}</div>
              ))}
            </div>
          </div>

          {/* Divider */}
          <div style={{ height:1, background:"rgba(255,255,255,0.15)", marginBottom:14 }} />

          {/* Domestic Beer */}
          <div>
            <div style={{ fontFamily:"'Kalam',cursive", fontSize:20, fontWeight:700,
              color:"#5bb8ff", letterSpacing:"0.1em", marginBottom:6,
              textShadow:"0 0 10px #5bb8ff88" }}>DOMESTIC BEER</div>
            <div style={{ border:"2px solid rgba(255,255,255,0.35)", borderRadius:4,
              padding:"4px 14px", display:"inline-block", marginBottom:10 }}>
              <span style={{ fontFamily:"'Caveat',cursive", fontSize:30, fontWeight:700,
                color:"rgba(255,255,255,0.85)" }}>3.00</span>
            </div>
            <div style={{ display:"flex", flexWrap:"wrap", gap:"4px 16px" }}>
              {DOMESTIC_BEERS.map((b,i) => (
                <div key={b} style={{ display:"flex", alignItems:"center", gap:5 }}>
                  {i > 0 && i < DOMESTIC_BEERS.length && (
                    <div style={{ width:5, height:5, borderRadius:"50%", background:"#5bb8ff",
                      boxShadow:"0 0 4px #5bb8ff" }} />
                  )}
                  <span style={{ fontFamily:"'Caveat',cursive", fontSize:16, fontWeight:600,
                    color:"#d4a853" }}>{b}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Clock */}
          <div style={{ marginTop:"auto", textAlign:"right" }}>
            <div style={{ fontFamily:"'Caveat',cursive", fontSize:22, fontWeight:700,
              color:"rgba(255,255,255,0.3)" }}>
              {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
            </div>
          </div>
        </div>
      </div>

      {/* Footer */}
      <div style={{ position:"absolute", bottom:8, left:0, right:0, textAlign:"center",
        fontFamily:"'DM Sans'", fontSize:9, color:"rgba(255,255,255,0.12)",
        letterSpacing:"0.1em" }}>VENNU · CONNECTED</div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════
   DISPLAY 2: TAP STRIPS (Image 2 style — brewery board)
   ══════════════════════════════════════════════════════════════════════ */
const TAP_FONTS = [
  "'Permanent Marker',cursive",
  "'Bungee',cursive",
  "'Righteous',cursive",
  "'Caveat',cursive",
  "'Pacifico',cursive",
  "'Kalam',cursive",
];

function TapStripBoard({ venueName }) {
  const [time, setTime] = useState(new Date());
  useEffect(()=>{ const t=setInterval(()=>setTime(new Date()),1000); return()=>clearInterval(t); },[]);
  const available = TAPS.filter(t=>t.available);

  return (
    <div style={{ width:"100%", height:"100%", background:"#111",
      backgroundImage:`${CHALK_NOISE}, linear-gradient(180deg,#151515 0%,#111 100%)`,
      display:"flex", flexDirection:"column", padding:"14px 16px", gap:0, overflow:"hidden" }}>

      {/* Venue header */}
      <div style={{ display:"flex", alignItems:"center", gap:12, marginBottom:12, flexShrink:0 }}>
        <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:20, color:"#f0a500",
          letterSpacing:"-0.01em" }}>{venueName}</div>
        <div style={{ flex:1, height:1, background:"rgba(255,255,255,0.08)" }} />
        <div style={{ fontFamily:"'DM Mono'", fontSize:14, color:"rgba(255,255,255,0.3)" }}>
          {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
        </div>
      </div>

      {/* Tap strips grid */}
      <div style={{ flex:1, display:"grid",
        gridTemplateColumns:"1fr 1fr 1fr",
        gridTemplateRows:`repeat(${Math.ceil(available.length/3)},1fr)`,
        gap:"6px", overflow:"hidden" }}>
        {available.map((tap, i) => (
          <div key={tap.id} className="tap-strip"
            style={{ animationDelay:`${i*60}ms`,
              background:"#1a1a1a",
              backgroundImage:`${CHALK_NOISE}`,
              borderRadius:4,
              border:"1px solid rgba(255,255,255,0.06)",
              padding:"10px 14px",
              display:"flex", flexDirection:"column", justifyContent:"center",
              position:"relative", overflow:"hidden",
              boxShadow:"inset 0 1px 0 rgba(255,255,255,0.04), 0 2px 8px rgba(0,0,0,0.4)" }}>
            {/* Tap number */}
            <div style={{ position:"absolute", top:8, right:10,
              fontFamily:"'DM Mono'", fontSize:11, color:"rgba(255,255,255,0.2)",
              fontWeight:500 }}>TAP {i+1}</div>
            {/* Beer name */}
            <div style={{
              fontFamily: TAP_FONTS[i % TAP_FONTS.length],
              fontSize: tap.name.length > 12 ? 22 : tap.name.length > 8 ? 28 : 34,
              color: tap.nameColor,
              textShadow:`0 0 10px ${tap.nameColor}66, 0 0 25px ${tap.nameColor}33`,
              lineHeight:1.1, marginBottom:4,
            }}>{tap.name}</div>
            {/* Style + ABV */}
            <div style={{ display:"flex", alignItems:"center", gap:8 }}>
              <span style={{ fontFamily:"'Caveat',cursive", fontSize:14, color:"rgba(255,255,255,0.5)" }}>
                {tap.style}
              </span>
              {tap.abv > 0 && (
                <span style={{ fontFamily:"'DM Mono'", fontSize:12, fontWeight:500,
                  color:"rgba(255,255,255,0.35)" }}>{tap.abv}%</span>
              )}
            </div>
            {/* Price */}
            <div style={{ marginTop:4, fontFamily:"'Kalam',cursive", fontSize:16, fontWeight:700,
              color:"rgba(255,255,255,0.45)" }}>
              ${tap.price.toFixed(2)}
            </div>
          </div>
        ))}
      </div>

      <div style={{ textAlign:"center", marginTop:8, fontSize:9,
        color:"rgba(255,255,255,0.1)", letterSpacing:"0.1em", flexShrink:0 }}>
        VENNU · TAPLIST · CONNECTED
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════
   DISPLAY 3: DIGITAL BOARD (Image 3 style — Taplist.io competitor)
   ══════════════════════════════════════════════════════════════════════ */
function DigitalBoard({ venueName }) {
  const [time, setTime] = useState(new Date());
  useEffect(()=>{ const t=setInterval(()=>setTime(new Date()),1000); return()=>clearInterval(t); },[]);
  const featured = TAPS.filter(t=>t.available).slice(0,6);

  return (
    <div style={{ width:"100%", height:"100%", position:"relative", overflow:"hidden",
      background:"#2c1a0e",
      backgroundImage:`${WOOD_NOISE}, linear-gradient(145deg, #3a2010 0%, #2c1a0e 40%, #1e1008 100%)` }}>

      {/* Wood grain overlay */}
      <div style={{ position:"absolute", inset:0, opacity:0.15, pointerEvents:"none",
        backgroundImage:"repeating-linear-gradient(90deg, transparent, transparent 2px, rgba(255,255,255,0.03) 2px, rgba(255,255,255,0.03) 4px)" }} />

      {/* Header */}
      <div style={{ background:"rgba(0,0,0,0.4)", borderBottom:"2px solid rgba(255,255,255,0.06)",
        padding:"14px 32px", display:"flex", alignItems:"center", flexShrink:0 }}>
        <div style={{ flex:1 }}>
          <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:30,
            color:"#a3e635", letterSpacing:"0.02em",
            textShadow:"0 0 20px #a3e63566" }}>{venueName.toUpperCase()}</div>
          <div style={{ fontFamily:"'DM Sans'", fontSize:12, color:"rgba(255,255,255,0.4)",
            letterSpacing:"0.15em", textTransform:"uppercase", marginTop:2 }}>
            What's On Tap · Updated Live
          </div>
        </div>
        <div style={{ display:"flex", gap:24, alignItems:"center" }}>
          <div style={{ textAlign:"center" }}>
            <div style={{ fontSize:11, color:"rgba(255,255,255,0.35)", letterSpacing:"0.1em" }}>TAPS</div>
            <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:24, color:"#a3e635" }}>
              {featured.length}
            </div>
          </div>
          <div style={{ fontFamily:"'DM Mono'", fontSize:22, fontWeight:500,
            color:"rgba(255,255,255,0.7)" }}>
            {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
          </div>
        </div>
      </div>

      {/* Beer grid */}
      <div style={{ flex:1, display:"grid", gridTemplateColumns:"1fr 1fr",
        gridTemplateRows:"repeat(3,1fr)", gap:"1px",
        background:"rgba(0,0,0,0.3)", height:"calc(100% - 75px)" }}>
        {featured.map((tap, i) => (
          <div key={tap.id} style={{ background:"rgba(0,0,0,0.25)",
            padding:"16px 20px", display:"flex", alignItems:"flex-start", gap:16,
            borderBottom:"1px solid rgba(255,255,255,0.05)",
            borderRight: i%2===0 ? "1px solid rgba(255,255,255,0.05)" : "none",
            position:"relative" }}>

            {/* Beer glass illustration */}
            <div style={{ flexShrink:0, opacity:0.9 }}>
              <BeerGlass color={tap.color} size={48} beerStyle={tap.style} />
            </div>

            {/* Info */}
            <div style={{ flex:1, minWidth:0 }}>
              <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:15,
                color:"#a3e635", letterSpacing:"0.04em", marginBottom:3,
                textShadow:"0 0 12px #a3e63550" }}>
                {tap.name.toUpperCase()}
              </div>
              <div style={{ display:"flex", alignItems:"center", gap:8, marginBottom:6 }}>
                <span style={{ fontSize:12, color:"rgba(255,255,255,0.55)",
                  fontStyle:"italic" }}>{tap.style}</span>
                {tap.abv > 0 && <>
                  <span style={{ color:"rgba(255,255,255,0.2)" }}>·</span>
                  <span style={{ fontFamily:"'DM Mono'", fontSize:12,
                    color:"rgba(255,255,255,0.45)", fontWeight:500 }}>{tap.abv}%</span>
                </>}
                {tap.ibu > 0 && <>
                  <span style={{ color:"rgba(255,255,255,0.2)" }}>·</span>
                  <span style={{ fontFamily:"'DM Mono'", fontSize:11,
                    color:"rgba(255,255,255,0.3)" }}>{tap.ibu} IBU</span>
                </>}
              </div>
              <div style={{ fontSize:12, color:"rgba(255,255,255,0.45)", lineHeight:1.5,
                display:"-webkit-box", WebkitLineClamp:2, WebkitBoxOrient:"vertical",
                overflow:"hidden" }}>{tap.desc}</div>
            </div>

            {/* Price */}
            <div style={{ flexShrink:0, textAlign:"right" }}>
              <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:18,
                color:"rgba(255,255,255,0.8)" }}>${tap.price.toFixed(2)}</div>
              <div style={{ fontSize:10, color:"rgba(255,255,255,0.25)",
                letterSpacing:"0.06em" }}>PINT</div>
            </div>
          </div>
        ))}
      </div>

      {/* Footer */}
      <div style={{ position:"absolute", bottom:0, left:0, right:0,
        padding:"6px 20px", display:"flex", justifyContent:"space-between",
        background:"rgba(0,0,0,0.5)", borderTop:"1px solid rgba(255,255,255,0.05)" }}>
        <div style={{ fontSize:9, color:"rgba(255,255,255,0.15)", letterSpacing:"0.1em" }}>
          Powered by Vennu · Taplist
        </div>
        <div style={{ fontSize:9, color:"rgba(255,255,255,0.15)", letterSpacing:"0.06em" }}>
          Prices subject to change · Please drink responsibly
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════
   ADMIN — Tap List Manager
   ══════════════════════════════════════════════════════════════════════ */
function TapManager({ taps, setTaps, toast }) {
  const [editId, setEditId] = useState(null);
  const [editData, setEditData] = useState({});

  const startEdit = (tap) => { setEditId(tap.id); setEditData({...tap}); };
  const save = () => {
    setTaps(t => t.map(tap => tap.id===editId ? {...editData} : tap));
    toast("Tap updated · Synced to board");
    setEditId(null);
  };
  const toggleAvail = (id) => {
    setTaps(t => t.map(tap => tap.id===id ? {...tap,available:!tap.available} : tap));
    toast("Tap availability updated");
  };

  const inputSt = {
    background:"#1a1e26", border:"1px solid #23272f", borderRadius:7,
    padding:"7px 11px", color:"#e8e3db", fontSize:13, outline:"none", width:"100%",
  };

  return (
    <div style={{ display:"flex", flexDirection:"column", gap:8 }}>
      {taps.map((tap,i) => (
        <div key={tap.id} style={{ background:"#13161c",
          border:`1px solid ${editId===tap.id ? "#f0a50040" : "#23272f"}`,
          borderRadius:10, overflow:"hidden", opacity:tap.available?1:0.5, transition:"all 0.15s" }}>
          {editId===tap.id ? (
            <div style={{ padding:"12px 14px", display:"flex", flexDirection:"column", gap:10 }}>
              <div style={{ display:"grid", gridTemplateColumns:"1fr 1fr", gap:10 }}>
                <div>
                  <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>Name</div>
                  <input value={editData.name} onChange={e=>setEditData(d=>({...d,name:e.target.value}))} style={inputSt} />
                </div>
                <div>
                  <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>Style</div>
                  <input value={editData.style} onChange={e=>setEditData(d=>({...d,style:e.target.value}))} style={inputSt} />
                </div>
              </div>
              <div style={{ display:"grid", gridTemplateColumns:"1fr 1fr 1fr", gap:10 }}>
                <div>
                  <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>ABV %</div>
                  <input type="number" step="0.1" value={editData.abv} onChange={e=>setEditData(d=>({...d,abv:parseFloat(e.target.value)}))} style={inputSt} />
                </div>
                <div>
                  <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>IBU</div>
                  <input type="number" value={editData.ibu} onChange={e=>setEditData(d=>({...d,ibu:parseInt(e.target.value)}))} style={inputSt} />
                </div>
                <div>
                  <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>Price $</div>
                  <input type="number" step="0.5" value={editData.price} onChange={e=>setEditData(d=>({...d,price:parseFloat(e.target.value)}))} style={inputSt} />
                </div>
              </div>
              <div>
                <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5}}>Description</div>
                <textarea value={editData.desc} onChange={e=>setEditData(d=>({...d,desc:e.target.value}))}
                  style={{...inputSt,height:52,resize:"none"}} />
              </div>
              <div style={{ display:"flex", alignItems:"center", gap:10 }}>
                <div style={{fontSize:10,fontWeight:600,color:"#8892a0",textTransform:"uppercase",letterSpacing:"0.07em"}}>Name Color</div>
                <div style={{ width:30, height:30, borderRadius:"50%", overflow:"hidden",
                  border:"2px solid #333", background:editData.nameColor }}>
                  <input type="color" value={editData.nameColor} onChange={e=>setEditData(d=>({...d,nameColor:e.target.value}))} />
                </div>
                <div style={{ width:30, height:30, borderRadius:"50%", overflow:"hidden",
                  border:"2px solid #333", background:editData.color }}>
                  <input type="color" value={editData.color} onChange={e=>setEditData(d=>({...d,color:e.target.value}))}
                    title="Glass color" />
                </div>
                <div style={{fontSize:10,color:"#8892a0"}}>← glass color</div>
                <div style={{flex:1}} />
                <button className="btn" onClick={()=>setEditId(null)}
                  style={{padding:"6px 14px",borderRadius:7,background:"#1a1e26",color:"#8892a0",fontSize:12,fontWeight:600,cursor:"pointer"}}>Cancel</button>
                <button className="btn" onClick={save}
                  style={{padding:"6px 14px",borderRadius:7,background:"#f0a500",color:"#000",fontSize:12,fontWeight:700,cursor:"pointer"}}>Save & Sync</button>
              </div>
            </div>
          ) : (
            <div style={{ display:"flex", alignItems:"center", gap:10, padding:"10px 14px" }}>
              <div style={{ fontFamily:"'DM Mono'", fontSize:11, color:"#8892a0", width:24,
                textAlign:"center" }}>#{i+1}</div>
              <div style={{ width:28, height:28, borderRadius:"50%", flexShrink:0,
                background:tap.color, opacity:0.8 }} />
              <div style={{ flex:1, minWidth:0 }}>
                <div style={{ display:"flex", alignItems:"center", gap:8 }}>
                  <span style={{ fontWeight:700, fontSize:14, color:tap.nameColor,
                    textShadow:`0 0 8px ${tap.nameColor}44` }}>{tap.name}</span>
                  <span style={{ fontSize:11, color:"#8892a0" }}>{tap.style}</span>
                </div>
                <div style={{ display:"flex", gap:10 }}>
                  {tap.abv>0 && <span style={{ fontFamily:"'DM Mono'", fontSize:11, color:"#8892a0" }}>{tap.abv}% ABV</span>}
                  {tap.ibu>0 && <span style={{ fontFamily:"'DM Mono'", fontSize:11, color:"#8892a0" }}>{tap.ibu} IBU</span>}
                  <span style={{ fontFamily:"'DM Mono'", fontSize:11, color:"#f0a500" }}>${tap.price.toFixed(2)}</span>
                </div>
              </div>
              <div style={{ display:"flex", gap:6, alignItems:"center" }}>
                <button className="btn" onClick={()=>toggleAvail(tap.id)}
                  style={{ fontSize:11, padding:"3px 9px", borderRadius:5, cursor:"pointer", border:"none",
                    background:tap.available?"#22c55e20":"#ef444420",
                    color:tap.available?"#22c55e":"#ef4444", fontWeight:600 }}>
                  {tap.available?"On Tap":"Off"}
                </button>
                <button className="btn" onClick={()=>startEdit(tap)}
                  style={{background:"transparent",border:"none",color:"#8892a0",cursor:"pointer",fontSize:14}}>✏️</button>
              </div>
            </div>
          )}
        </div>
      ))}
      <button className="btn" onClick={()=>{
        const newTap={id:`t${Date.now()}`,name:"New Tap",style:"",abv:0,ibu:0,color:"#f5c842",
          desc:"",price:7.00,nameColor:"#ffd700",available:true};
        setTaps(t=>[...t,newTap]);
        setTimeout(()=>startEdit(newTap),50);
      }} style={{ padding:"11px",borderRadius:10,background:"transparent",
        border:"1px dashed #23272f",color:"#8892a0",fontSize:13,cursor:"pointer",fontWeight:500 }}>
        + Add Tap
      </button>
    </div>
  );
}

/* ── Toast ────────────────────────────────────────────────────────────── */
function Toast({ msg, onDone }) {
  useEffect(()=>{ const t=setTimeout(onDone,2400); return()=>clearTimeout(t); },[]);
  return (
    <div style={{ position:"fixed",bottom:28,right:28,zIndex:9999,
      background:"linear-gradient(135deg,#5cb88a,#3a9e6d)",color:"#fff",
      borderRadius:12,padding:"13px 22px",fontWeight:600,fontSize:13,
      boxShadow:"0 8px 32px rgba(0,0,0,0.6)",display:"flex",alignItems:"center",
      gap:10,animation:"toastIn 0.3s ease" }}>
      <span>✓</span>{msg}
    </div>
  );
}

/* ── Main App ─────────────────────────────────────────────────────────── */
const BOARD_STYLES = [
  { id:"chalkboard", label:"🍸 Classic Chalk",  desc:"Cocktail & drinks menu" },
  { id:"tapstrip",   label:"🍺 Tap Strips",     desc:"Brewery board style"    },
  { id:"digital",    label:"📺 Digital Board",  desc:"Taplist.io style"       },
];

export default function VennuTapList() {
  const [taps, setTaps]           = useState(TAPS);
  const [boardStyle, setBoardStyle] = useState("chalkboard");
  const [venueName, setVenueName] = useState("480 Brewing Co.");
  const [showPreview, setShowPreview] = useState(false);
  const [toast, setToast]         = useState(null);
  const [previewScale, setPreviewScale] = useState(0.52);

  useEffect(()=>{
    const calc = () => {
      const avail = window.innerWidth - 360 - 56;
      setPreviewScale(Math.min(0.60, avail / 960));
    };
    calc();
    window.addEventListener("resize",calc);
    return ()=>window.removeEventListener("resize",calc);
  },[]);

  const Board = boardStyle==="chalkboard" ? ClassicChalkboard
    : boardStyle==="tapstrip" ? TapStripBoard : DigitalBoard;

  return (
    <>
      <style>{STYLES}</style>
      {showPreview ? (
        <div style={{ width:"100vw",height:"100vh",background:"#000",
          display:"flex",flexDirection:"column" }}>
          <div style={{ background:"#13161c",borderBottom:"1px solid #23272f",
            padding:"10px 20px",display:"flex",alignItems:"center",gap:12,flexShrink:0 }}>
            <button className="btn" onClick={()=>setShowPreview(false)}
              style={{ padding:"6px 14px",borderRadius:7,background:"#1a1e26",
                color:"#f0a500",border:"1px solid #f0a50040",fontSize:12,fontWeight:600,cursor:"pointer" }}>← Admin</button>
            <span style={{ fontSize:12,color:"#8892a0" }}>Full Preview · {venueName}</span>
            <div style={{ marginLeft:"auto",display:"flex",gap:8 }}>
              {BOARD_STYLES.map(s => (
                <button key={s.id} className="btn" onClick={()=>setBoardStyle(s.id)}
                  style={{ padding:"5px 12px",borderRadius:6,fontSize:12,fontWeight:600,
                    cursor:"pointer",border:"none",
                    background:boardStyle===s.id?"#f0a500":"#1a1e26",
                    color:boardStyle===s.id?"#000":"#8892a0" }}>{s.label}</button>
              ))}
            </div>
          </div>
          <div style={{ flex:1 }}>
            <Board venueName={venueName} taps={taps} />
          </div>
        </div>
      ) : (
        <div style={{ display:"flex",height:"100vh",overflow:"hidden",background:"#0c0e12" }}>

          {/* ── Admin Panel ── */}
          <div style={{ width:360,background:"#13161c",borderRight:"1px solid #23272f",
            display:"flex",flexDirection:"column",flexShrink:0 }}>
            {/* Header */}
            <div style={{ padding:"18px 20px 14px",borderBottom:"1px solid #23272f" }}>
              <div style={{ display:"flex",alignItems:"baseline",gap:8,marginBottom:3 }}>
                <span style={{ fontFamily:"'Playfair Display',serif",fontWeight:700,fontSize:22,color:"#f0a500" }}>vennu</span>
                <span style={{ fontSize:13,color:"#8892a0" }}>/ Tap List</span>
              </div>
              <input value={venueName} onChange={e=>setVenueName(e.target.value)}
                style={{ marginTop:8,background:"#1a1e26",border:"1px solid #23272f",borderRadius:8,
                  padding:"7px 12px",color:"#e8e3db",fontSize:14,outline:"none",width:"100%",
                  fontFamily:"'Syne',sans-serif",fontWeight:700 }} />
            </div>

            {/* Board style selector */}
            <div style={{ padding:"12px 16px",borderBottom:"1px solid #23272f",
              display:"flex",flexDirection:"column",gap:6 }}>
              <div style={{ fontSize:10,fontWeight:600,color:"#8892a0",
                textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:2 }}>Board Style</div>
              {BOARD_STYLES.map(s=>(
                <div key={s.id} onClick={()=>setBoardStyle(s.id)}
                  style={{ display:"flex",alignItems:"center",gap:10,padding:"8px 12px",
                    borderRadius:8,cursor:"pointer",transition:"all 0.15s",
                    background:boardStyle===s.id?"#f0a50015":"transparent",
                    border:`1px solid ${boardStyle===s.id?"#f0a50040":"transparent"}` }}>
                  <span style={{ fontSize:14 }}>{s.label.split(" ")[0]}</span>
                  <div style={{ flex:1 }}>
                    <div style={{ fontSize:13,fontWeight:600,
                      color:boardStyle===s.id?"#f0a500":"#e8e3db" }}>{s.label.slice(3)}</div>
                    <div style={{ fontSize:11,color:"#8892a0" }}>{s.desc}</div>
                  </div>
                  {boardStyle===s.id && <span style={{ color:"#f0a500",fontSize:13 }}>✓</span>}
                </div>
              ))}
            </div>

            {/* Tap list */}
            <div style={{ flex:1,overflowY:"auto",padding:"12px 16px" }}>
              <div style={{ fontSize:10,fontWeight:600,color:"#8892a0",
                textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:10 }}>
                Taps ({taps.filter(t=>t.available).length} on / {taps.length} total)
              </div>
              <TapManager taps={taps} setTaps={setTaps} toast={msg=>setToast(msg)} />
            </div>

            {/* Footer actions */}
            <div style={{ padding:"14px 16px",borderTop:"1px solid #23272f" }}>
              <button className="btn" onClick={()=>setShowPreview(true)}
                style={{ width:"100%",padding:"11px",borderRadius:9,background:"#f0a500",
                  color:"#000",fontWeight:700,fontSize:13,border:"none",cursor:"pointer",
                  boxShadow:"0 0 14px #f0a50040" }}>📺 Full Screen Preview</button>
              <button className="btn" onClick={()=>setToast("Pushed to all screens!")}
                style={{ width:"100%",marginTop:8,padding:"9px",borderRadius:9,
                  background:"#1a1e26",color:"#8892a0",fontWeight:600,fontSize:12,
                  border:"1px solid #23272f",cursor:"pointer" }}>Push to Screens</button>
            </div>
          </div>

          {/* ── Preview Pane ── */}
          <div style={{ flex:1,display:"flex",flexDirection:"column",background:"#0c0e12",overflow:"hidden" }}>
            <div style={{ padding:"12px 24px",borderBottom:"1px solid #23272f",
              display:"flex",alignItems:"center",gap:12,flexShrink:0 }}>
              <div style={{ width:7,height:7,borderRadius:"50%",background:"#5cb88a",
                boxShadow:"0 0 6px #5cb88a" }} className="pulse-dot" />
              <span style={{ fontSize:13,color:"#8892a0",fontWeight:500 }}>Live Preview</span>
              <div style={{ marginLeft:"auto",fontFamily:"'DM Mono'",fontSize:11,
                color:"#8892a0",background:"#1a1e26",padding:"4px 12px",borderRadius:6 }}>
                1920×1080 · {Math.round(previewScale*100)}%
              </div>
            </div>

            <div style={{ flex:1,display:"flex",alignItems:"center",
              justifyContent:"center",padding:24,overflow:"hidden" }}>
              <div style={{ width:960*previewScale,height:540*previewScale,
                borderRadius:10,overflow:"hidden",flexShrink:0,
                boxShadow:"0 20px 60px rgba(0,0,0,0.8),0 0 0 1px #23272f" }}>
                <div style={{ width:960,height:540,
                  transform:`scale(${previewScale})`,transformOrigin:"top left" }}>
                  <Board venueName={venueName} taps={taps} />
                </div>
              </div>
            </div>

            {/* Style comparison strip */}
            <div style={{ padding:"10px 24px",borderTop:"1px solid #23272f",
              display:"flex",gap:12,alignItems:"center",flexShrink:0,overflowX:"auto" }}>
              <span style={{ fontSize:11,color:"#8892a0",flexShrink:0 }}>Switch style:</span>
              {BOARD_STYLES.map(s=>(
                <button key={s.id} className="btn" onClick={()=>setBoardStyle(s.id)}
                  style={{ padding:"5px 12px",borderRadius:6,fontSize:11,fontWeight:600,
                    cursor:"pointer",flexShrink:0,
                    border:`1px solid ${boardStyle===s.id?"#f0a50040":"#23272f"}`,
                    background:boardStyle===s.id?"#f0a50015":"#13161c",
                    color:boardStyle===s.id?"#f0a500":"#8892a0" }}>
                  {s.label}
                </button>
              ))}
              <div style={{ marginLeft:"auto",fontSize:11,color:"#8892a0",
                display:"flex",gap:16,flexShrink:0 }}>
                <span>🍸 Cocktails + fixed-price beers</span>
                <span>🍺 Individual tap strips</span>
                <span>📺 Full beer detail cards</span>
              </div>
            </div>
          </div>
        </div>
      )}
      {toast && <Toast msg={toast} onDone={()=>setToast(null)} />}
    </>
  );
}
