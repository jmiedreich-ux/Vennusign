import { useState, useEffect, useRef, useCallback } from "react";

/* ── Styles ─────────────────────────────────────────────────────────────── */
const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Syne:wght@700;800&family=DM+Sans:wght@300;400;500;600;700&family=Caveat:wght@700&family=Pacifico&display=swap');
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  body { background: #0a0c10; font-family: 'DM Sans', sans-serif; color: #e2e8f0; }
  ::-webkit-scrollbar { width: 4px; } ::-webkit-scrollbar-track { background: #0d1117; }
  ::-webkit-scrollbar-thumb { background: #30363d; border-radius: 2px; }

  @keyframes fadeUp   { from{opacity:0;transform:translateY(8px)} to{opacity:1;transform:translateY(0)} }
  @keyframes popIn    { from{opacity:0;transform:scale(0.94)} to{opacity:1;transform:scale(1)} }
  @keyframes toastIn  { from{opacity:0;transform:translateX(40px)} to{opacity:1;transform:translateX(0)} }
  @keyframes shimmer  { from{background-position:-200% 0} to{background-position:200% 0} }
  @keyframes pulse    { 0%,100%{opacity:1} 50%{opacity:0.5} }
  @keyframes slideNext{ from{opacity:0;transform:translateX(30px)} to{opacity:1;transform:translateX(0)} }

  .card-hover { transition: transform 0.2s ease, box-shadow 0.2s ease; cursor: grab; }
  .card-hover:hover { transform: scale(1.02); }
  .fade-up  { animation: fadeUp 0.3s ease forwards; }
  .pop-in   { animation: popIn 0.25s ease forwards; }
  .pulse-dot { animation: pulse 2s infinite; }
  input, select { font-family: 'DM Sans', sans-serif; }
  button { font-family: 'DM Sans', sans-serif; }

  .drag-over { outline: 2px dashed #f59e0b !important; outline-offset: 2px; }
`;

/* ── Food data ──────────────────────────────────────────────────────────── */
// gradient: [from, to], emoji shown as placeholder for the food photo
const ALL_ITEMS = [
  // Screen 1 candidates
  { id:"f1",  name:"General Tso's Chicken", desc:"Sweet & spicy crispy chicken",     price:14.99, emoji:"🍗", g:["#c1440e","#ff6b35"], bestseller:true,  cat:"Chicken" },
  { id:"f2",  name:"Kung Pao Shrimp",       desc:"Wok-fired with peanuts & chili",   price:16.99, emoji:"🦐", g:["#0077b6","#48cae4"], bestseller:true,  cat:"Seafood" },
  { id:"f3",  name:"Beef & Broccoli",       desc:"Tender beef, savory brown sauce",  price:15.99, emoji:"🥦", g:["#3d6b45","#52b788"], bestseller:true,  cat:"Beef"    },
  { id:"f4",  name:"Peking Duck",           desc:"Crispy skin, hoisin, scallion",    price:24.99, emoji:"🦆", g:["#8b1a1a","#c1440e"], bestseller:false, cat:"Duck"    },
  { id:"f5",  name:"Dim Sum Basket",        desc:"Steamed pork & shrimp dumplings",  price:12.99, emoji:"🥟", g:["#d4a017","#f7c59f"], bestseller:true,  cat:"Dim Sum" },
  { id:"f6",  name:"Mapo Tofu",             desc:"Silken tofu in spicy sauce",       price:13.99, emoji:"🌶",  g:["#9e1b00","#e63946"], bestseller:false, cat:"Tofu"    },
  // Screen 2 candidates
  { id:"f7",  name:"Dan Dan Noodles",       desc:"Sesame paste, minced pork",        price:13.99, emoji:"🍜", g:["#b5451b","#e9c46a"], bestseller:true,  cat:"Noodles" },
  { id:"f8",  name:"Char Siu Pork",         desc:"BBQ glazed pork belly",            price:17.99, emoji:"🥩", g:["#7b2d00","#c1440e"], bestseller:false, cat:"Pork"    },
  { id:"f9",  name:"Seafood Fried Rice",    desc:"Shrimp, scallop & egg",            price:15.99, emoji:"🍚", g:["#005f73","#0a9396"], bestseller:true,  cat:"Rice"    },
  { id:"f10", name:"Hot & Sour Soup",       desc:"Classic silken egg-drop broth",    price:8.99,  emoji:"🍲", g:["#ca6702","#ee9b00"], bestseller:false, cat:"Soup"    },
  { id:"f11", name:"Spring Rolls (6)",      desc:"Crispy veg & pork filling",        price:9.99,  emoji:"🌮", g:["#606c38","#dda15e"], bestseller:false, cat:"Sides"   },
  { id:"f12", name:"Mongolian Beef",        desc:"Tender flank steak, green onion",  price:16.99, emoji:"🐄", g:["#3a0000","#9b2226"], bestseller:true,  cat:"Beef"    },
  // Screen 3 candidates
  { id:"f13", name:"Steamed Sea Bass",      desc:"Ginger, scallion, soy oil",        price:22.99, emoji:"🐟", g:["#023e8a","#0096c7"], bestseller:false, cat:"Seafood" },
  { id:"f14", name:"Pork Dumplings",        desc:"Pan-fried with vinegar dip",       price:11.99, emoji:"🥟", g:["#e9c46a","#f4a261"], bestseller:true,  cat:"Dim Sum" },
  { id:"f15", name:"Sesame Chicken",        desc:"Honey glaze, toasted sesame",      price:14.99, emoji:"🍗", g:["#f77f00","#fcbf49"], bestseller:true,  cat:"Chicken" },
  { id:"f16", name:"Vegetable Lo Mein",     desc:"Wok-tossed egg noodles",           price:12.99, emoji:"🍝", g:["#386641","#6a994e"], bestseller:false, cat:"Noodles" },
  { id:"f17", name:"Chili Garlic Eggplant", desc:"Silky braised with black bean",    price:13.99, emoji:"🍆", g:["#4a0e8f","#7b2fbe"], bestseller:false, cat:"Veg"     },
  { id:"f18", name:"Mango Pudding",         desc:"Chilled with fresh mango coulis",  price:7.99,  emoji:"🥭", g:["#f4a261","#e9c46a"], bestseller:false, cat:"Dessert" },
];

const GRID_OPTIONS = [
  { cols:2, rows:2, label:"2×2", count:4,  desc:"Large photos — high impact" },
  { cols:3, rows:2, label:"3×2", count:6,  desc:"Balanced — most popular"   },
  { cols:4, rows:2, label:"4×2", count:8,  desc:"More items, smaller photos" },
  { cols:3, rows:3, label:"3×3", count:9,  desc:"Maximum density"            },
];

/* ── Helpers ─────────────────────────────────────────────────────────────── */
const fmt = (n) => `$${Number(n).toFixed(2)}`;
const chunk = (arr, size) => Array.from({ length: Math.ceil(arr.length / size) }, (_, i) => arr.slice(i * size, i * size + size));

/* ── Food photo card (gradient placeholder) ─────────────────────────────── */
function PhotoCard({ item, size = "normal", isHH = false }) {
  const [from, to] = item.g;
  const isLarge = size === "large";
  return (
    <div style={{
      borderRadius: 10, overflow: "hidden", background: "#111",
      position: "relative", display: "flex", flexDirection: "column",
      boxShadow: "0 4px 20px rgba(0,0,0,0.6)",
    }}>
      {/* Photo area — gradient + emoji */}
      <div style={{
        flex: 1, minHeight: isLarge ? 180 : 120,
        background: `linear-gradient(135deg, ${from}, ${to})`,
        display: "flex", alignItems: "center", justifyContent: "center",
        position: "relative",
      }}>
        <span style={{ fontSize: isLarge ? 72 : 48, filter: "drop-shadow(0 4px 8px rgba(0,0,0,0.5))" }}>
          {item.emoji}
        </span>
        {/* Bestseller ribbon */}
        {item.bestseller && (
          <div style={{
            position: "absolute", top: 10, left: -1,
            background: "#f59e0b", color: "#000", fontWeight: 800,
            fontSize: isLarge ? 11 : 9, padding: "3px 10px 3px 8px",
            borderRadius: "0 4px 4px 0",
            boxShadow: "0 2px 8px rgba(245,158,11,0.6)",
            letterSpacing: "0.05em",
          }}>★ POPULAR</div>
        )}
        {/* Gradient fade to info bar */}
        <div style={{ position:"absolute", bottom:0, left:0, right:0, height:"40%",
          background:"linear-gradient(to top, rgba(0,0,0,0.85), transparent)" }} />
      </div>
      {/* Info bar */}
      <div style={{ padding: isLarge ? "10px 12px" : "8px 10px", background: "#111" }}>
        <div style={{ fontWeight:700, fontSize: isLarge ? 15 : 13, color:"#f5f0e8",
          whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{item.name}</div>
        <div style={{ fontSize: isLarge ? 12 : 10, color:"#8b949e", marginTop:2,
          whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{item.desc}</div>
        <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between", marginTop:6 }}>
          <div style={{ display:"flex", alignItems:"center", gap:6 }}>
            {isHH && (
              <span style={{ fontSize: isLarge ? 14 : 12, fontWeight:800, color:"#f59e0b",
                background:"#f59e0b15", padding:"2px 7px", borderRadius:4 }}>
                {fmt(item.hhPrice || item.price * 0.75)}
              </span>
            )}
            <span style={{ fontSize: isLarge ? 14 : 12, fontWeight:700,
              color: isHH ? "#8b949e" : "#f59e0b",
              textDecoration: isHH ? "line-through" : "none" }}>
              {fmt(item.price)}
            </span>
          </div>
          <span style={{ fontSize: 10, color:"#ffffff30", fontStyle:"italic" }}>{item.cat}</span>
        </div>
      </div>
    </div>
  );
}

/* ── Display Board: Photo Grid ──────────────────────────────────────────── */
function PhotoBoard({ items, grid, screenNum, totalScreens, venueName, isHH }) {
  const [time, setTime] = useState(new Date());
  useEffect(() => { const t = setInterval(() => setTime(new Date()), 1000); return () => clearTimeout(t); }, []);

  return (
    <div style={{ width:"100%", height:"100%", background:"#0d0d0d", display:"flex",
      flexDirection:"column", position:"relative", overflow:"hidden" }}>

      {/* Header */}
      <div style={{ background:"#111", borderBottom:"1px solid #ffffff10",
        padding:"10px 20px", display:"flex", alignItems:"center", gap:12, flexShrink:0 }}>
        <div style={{ flex:1 }}>
          <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:20, color:"#f5f0e8",
            letterSpacing:"-0.01em" }}>{venueName}</div>
          {isHH && (
            <div style={{ fontSize:11, color:"#f59e0b", fontWeight:700, letterSpacing:"0.08em" }}>
              ★ HAPPY HOUR · PRICES SHOWN
            </div>
          )}
        </div>
        {/* Screen indicator dots */}
        <div style={{ display:"flex", gap:5, alignItems:"center" }}>
          {Array.from({length:totalScreens}).map((_,i) => (
            <div key={i} style={{ width: i+1===screenNum ? 20 : 8, height:8, borderRadius:4,
              background: i+1===screenNum ? "#f59e0b" : "#ffffff20",
              transition:"all 0.3s", boxShadow: i+1===screenNum ? "0 0 8px #f59e0b" : "none" }} />
          ))}
          <span style={{ marginLeft:4, fontSize:11, color:"#8b949e" }}>
            {screenNum}/{totalScreens}
          </span>
        </div>
        <div style={{ fontFamily:"'DM Sans'", fontSize:18, fontWeight:700, color:"#f5f0e8" }}>
          {time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}
        </div>
      </div>

      {/* Photo grid */}
      <div style={{ flex:1, padding:12, overflow:"hidden",
        display:"grid", gap:10,
        gridTemplateColumns:`repeat(${grid.cols}, 1fr)`,
        gridTemplateRows:`repeat(${grid.rows}, 1fr)`,
      }}>
        {items.map((item, i) => (
          <div key={item.id} className="pop-in" style={{ animationDelay:`${i*60}ms` }}>
            <PhotoCard item={item} size={grid.cols <= 2 ? "large" : "normal"} isHH={isHH} />
          </div>
        ))}
        {/* Empty slots */}
        {Array.from({length: Math.max(0, grid.cols * grid.rows - items.length)}).map((_,i) => (
          <div key={`empty-${i}`} style={{ borderRadius:10, background:"#ffffff05",
            border:"1px dashed #ffffff10" }} />
        ))}
      </div>

      {/* Footer */}
      <div style={{ background:"#080808", borderTop:"1px solid #ffffff08",
        padding:"6px 20px", display:"flex", alignItems:"center", justifyContent:"space-between",
        flexShrink:0 }}>
        <div style={{ fontSize:10, color:"#ffffff20", letterSpacing:"0.05em" }}>
          TapBoard · Screen {screenNum} of {totalScreens} · Connected
        </div>
        <div style={{ display:"flex", gap:12 }}>
          {["GF Gluten Free","V Vegetarian","🌶 Spicy"].map(t=>(
            <span key={t} style={{fontSize:10, color:"#ffffff25"}}>· {t}</span>
          ))}
        </div>
        <div style={{ fontSize:10, color:"#ffffff20" }}>
          {time.toLocaleDateString([],{weekday:"long",month:"short",day:"numeric"})}
        </div>
      </div>
    </div>
  );
}

/* ── Mini screen preview (for overflow visualization) ───────────────────── */
function MiniScreen({ items, grid, screenNum, totalScreens, isActive, onClick, isHH }) {
  return (
    <div onClick={onClick} style={{ cursor:"pointer", display:"flex", flexDirection:"column",
      gap:6, alignItems:"center" }}>
      <div style={{ fontSize:11, fontWeight:600,
        color: isActive ? "#f59e0b" : "#8b949e" }}>
        Screen {screenNum}
        {screenNum===1 && <span style={{ marginLeft:4, fontSize:9, color:"#22c55e" }}>●</span>}
      </div>
      <div style={{ width:160, height:90, borderRadius:6, overflow:"hidden",
        border: isActive ? "2px solid #f59e0b" : "2px solid #30363d",
        boxShadow: isActive ? "0 0 12px #f59e0b60" : "none",
        transition:"all 0.2s", background:"#0d0d0d",
        display:"flex", flexDirection:"column" }}>
        {/* Mini header */}
        <div style={{ height:12, background:"#111", borderBottom:"1px solid #ffffff10",
          display:"flex", alignItems:"center", paddingLeft:5, gap:3 }}>
          <div style={{ width:30, height:3, borderRadius:2, background:"#ffffff30" }} />
        </div>
        {/* Mini grid */}
        <div style={{ flex:1, padding:4, display:"grid", gap:3,
          gridTemplateColumns:`repeat(${grid.cols},1fr)`,
          gridTemplateRows:`repeat(${grid.rows},1fr)` }}>
          {items.map((item) => (
            <div key={item.id} style={{ borderRadius:3, overflow:"hidden",
              background:`linear-gradient(135deg, ${item.g[0]}, ${item.g[1]})`,
              display:"flex", alignItems:"center", justifyContent:"center", fontSize:8 }}>
              {item.emoji}
            </div>
          ))}
          {Array.from({length:Math.max(0,grid.cols*grid.rows-items.length)}).map((_,i)=>(
            <div key={i} style={{ borderRadius:3, background:"#ffffff06",
              border:"1px dashed #ffffff10" }} />
          ))}
        </div>
      </div>
      <div style={{ fontSize:10, color:"#8b949e" }}>
        {items.length} item{items.length!==1?"s":""}
      </div>
    </div>
  );
}

/* ── Admin: Item list with drag handles ─────────────────────────────────── */
function ItemManager({ featured, all, onToggle, onReorder }) {
  const dragItem = useRef(null);
  const dragOver = useRef(null);

  const handleDragStart = (i) => { dragItem.current = i; };
  const handleDragEnter = (i) => { dragOver.current = i; };
  const handleDrop = () => {
    if (dragItem.current === null || dragItem.current === dragOver.current) return;
    const reordered = [...featured];
    const [moved] = reordered.splice(dragItem.current, 1);
    reordered.splice(dragOver.current, 0, moved);
    onReorder(reordered);
    dragItem.current = null; dragOver.current = null;
  };

  const unfeatured = all.filter(i => !featured.find(f => f.id === i.id));

  return (
    <div style={{ display:"flex", flexDirection:"column", gap:8 }}>
      <div style={{ fontSize:11, fontWeight:600, color:"#f59e0b", textTransform:"uppercase",
        letterSpacing:"0.06em", marginBottom:2 }}>
        On Display ({featured.length}) — drag to reorder
      </div>
      {featured.map((item, i) => (
        <div key={item.id} draggable
          onDragStart={()=>handleDragStart(i)}
          onDragEnter={()=>handleDragEnter(i)}
          onDragEnd={handleDrop}
          onDragOver={e=>e.preventDefault()}
          style={{ display:"flex", alignItems:"center", gap:8, padding:"7px 10px",
            borderRadius:8, background:"#21262d", border:"1px solid #30363d",
            cursor:"grab", userSelect:"none" }}>
          <span style={{ color:"#8b949e", fontSize:14 }}>⠿</span>
          <div style={{ width:26, height:26, borderRadius:6, flexShrink:0,
            background:`linear-gradient(135deg,${item.g[0]},${item.g[1]})`,
            display:"flex", alignItems:"center", justifyContent:"center", fontSize:14 }}>
            {item.emoji}
          </div>
          <div style={{ flex:1, minWidth:0 }}>
            <div style={{ fontSize:13, fontWeight:600, color:"#f5f0e8",
              whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{item.name}</div>
            <div style={{ fontSize:11, color:"#8b949e" }}>{fmt(item.price)} · {item.cat}</div>
          </div>
          {item.bestseller && (
            <span style={{ fontSize:9, padding:"2px 6px", borderRadius:3,
              background:"#f59e0b20", color:"#f59e0b", fontWeight:700 }}>★</span>
          )}
          <button onClick={()=>onToggle(item.id)}
            style={{ background:"#ef444420", border:"none", color:"#ef4444",
              borderRadius:5, padding:"3px 7px", fontSize:11, cursor:"pointer", fontWeight:600 }}>
            Remove
          </button>
        </div>
      ))}

      {unfeatured.length > 0 && (
        <>
          <div style={{ fontSize:11, fontWeight:600, color:"#8b949e", textTransform:"uppercase",
            letterSpacing:"0.06em", marginTop:8, marginBottom:2 }}>
            Not on display ({unfeatured.length})
          </div>
          {unfeatured.map(item => (
            <div key={item.id} style={{ display:"flex", alignItems:"center", gap:8,
              padding:"7px 10px", borderRadius:8, background:"#161b22",
              border:"1px solid #21262d", opacity:0.7 }}>
              <div style={{ width:26, height:26, borderRadius:6, flexShrink:0,
                background:`linear-gradient(135deg,${item.g[0]},${item.g[1]})`,
                display:"flex", alignItems:"center", justifyContent:"center", fontSize:14 }}>
                {item.emoji}
              </div>
              <div style={{ flex:1, minWidth:0 }}>
                <div style={{ fontSize:13, fontWeight:500, color:"#8b949e",
                  whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{item.name}</div>
                <div style={{ fontSize:11, color:"#8b949e50" }}>{fmt(item.price)} · {item.cat}</div>
              </div>
              <button onClick={()=>onToggle(item.id)}
                style={{ background:"#22c55e20", border:"none", color:"#22c55e",
                  borderRadius:5, padding:"3px 7px", fontSize:11, cursor:"pointer", fontWeight:600 }}>
                + Add
              </button>
            </div>
          ))}
        </>
      )}
    </div>
  );
}

/* ── Toast ───────────────────────────────────────────────────────────────── */
function Toast({ msg, onDone }) {
  useEffect(() => { const t = setTimeout(onDone, 2500); return () => clearTimeout(t); }, []);
  return (
    <div style={{ position:"fixed", bottom:28, right:28, zIndex:1000,
      background:"linear-gradient(135deg,#22c55e,#16a34a)", color:"#fff",
      borderRadius:12, padding:"13px 22px", fontWeight:600, fontSize:14,
      boxShadow:"0 8px 32px rgba(0,0,0,0.5)", display:"flex", alignItems:"center", gap:10,
      animation:"toastIn 0.25s ease" }}>
      <span style={{fontSize:18}}>✓</span> {msg}
    </div>
  );
}

/* ── Main App ────────────────────────────────────────────────────────────── */
export default function PhotoMenuBuilder() {
  const [featured, setFeatured] = useState(ALL_ITEMS.slice(0, 12));
  const [grid, setGrid]         = useState(GRID_OPTIONS[1]); // 3×2 default
  const [activeScreen, setActiveScreen] = useState(1);
  const [isHH, setIsHH]         = useState(false);
  const [venueName, setVenueName] = useState("Golden Dragon");
  const [toast, setToast]       = useState(null);
  const [tab, setTab]           = useState("items"); // "items" | "grid" | "settings"
  const [previewScale, setPreviewScale] = useState(0.55);

  useEffect(() => {
    const calc = () => {
      const panelW = 320;
      const padding = 48;
      const avail = window.innerWidth - panelW - padding;
      setPreviewScale(Math.min(0.65, avail / 960));
    };
    calc();
    window.addEventListener("resize", calc);
    return () => window.removeEventListener("resize", calc);
  }, []);

  const itemsPerScreen  = grid.cols * grid.rows;
  const screens         = chunk(featured, itemsPerScreen);
  const totalScreens    = Math.max(1, screens.length);
  const currentItems    = screens[activeScreen - 1] || [];

  const toggleItem = useCallback((id) => {
    setFeatured(prev => {
      if (prev.find(i => i.id === id)) return prev.filter(i => i.id !== id);
      const item = ALL_ITEMS.find(i => i.id === id);
      return item ? [...prev, item] : prev;
    });
    setActiveScreen(1);
  }, []);

  const reorderFeatured = useCallback((newOrder) => {
    setFeatured(newOrder);
  }, []);

  const labelStyle = {
    fontSize:11, fontWeight:600, color:"#8b949e",
    textTransform:"uppercase", letterSpacing:"0.06em", marginBottom:7,
    display:"block",
  };

  const inputStyle = {
    background:"#21262d", border:"1px solid #30363d", borderRadius:8,
    padding:"8px 12px", color:"#e2e8f0", fontSize:13, outline:"none", width:"100%",
  };

  return (
    <>
      <style>{STYLES}</style>
      <div style={{ display:"flex", height:"100vh", overflow:"hidden", background:"#0a0c10" }}>

        {/* ── LEFT PANEL ── */}
        <div style={{ width:320, height:"100%", overflowY:"auto", background:"#0d1117",
          borderRight:"1px solid #21262d", flexShrink:0, display:"flex", flexDirection:"column" }}>

          {/* Header */}
          <div style={{ padding:"16px 18px 0", borderBottom:"1px solid #21262d", flexShrink:0 }}>
            <div style={{ fontFamily:"'Syne'", fontWeight:800, fontSize:17, color:"#f59e0b",
              marginBottom:14 }}>TapBoard <span style={{color:"#8b949e",fontWeight:400,fontSize:13}}>/ Photo Board</span></div>
            {/* Tabs */}
            <div style={{ display:"flex", gap:0 }}>
              {[["items","🍱 Items"],["grid","⊞ Grid"],["settings","⚙ Settings"]].map(([id,label])=>(
                <button key={id} onClick={()=>setTab(id)} style={{
                  flex:1, padding:"8px 0", border:"none", cursor:"pointer",
                  background:"transparent", fontFamily:"'DM Sans'", fontSize:12, fontWeight:600,
                  color: tab===id ? "#f59e0b" : "#8b949e",
                  borderBottom: tab===id ? "2px solid #f59e0b" : "2px solid transparent",
                }}>
                  {label}
                </button>
              ))}
            </div>
          </div>

          {/* Panel content */}
          <div style={{ flex:1, overflowY:"auto", padding:"16px 18px" }}>

            {tab==="items" && (
              <ItemManager
                featured={featured}
                all={ALL_ITEMS}
                onToggle={toggleItem}
                onReorder={reorderFeatured}
              />
            )}

            {tab==="grid" && (
              <div style={{ display:"flex", flexDirection:"column", gap:20 }}>
                <div>
                  <span style={labelStyle}>Grid Layout</span>
                  <div style={{ display:"flex", flexDirection:"column", gap:8 }}>
                    {GRID_OPTIONS.map(opt => (
                      <div key={opt.label} onClick={()=>{ setGrid(opt); setActiveScreen(1); }}
                        style={{ padding:"12px 14px", borderRadius:10, cursor:"pointer",
                          background: grid.label===opt.label ? "#1c2128" : "#161b22",
                          border: grid.label===opt.label ? "1px solid #f59e0b50" : "1px solid #21262d",
                          display:"flex", alignItems:"center", gap:12, transition:"all 0.15s" }}>
                        {/* Mini grid preview */}
                        <div style={{ display:"grid", gap:2, width:40, height:28,
                          gridTemplateColumns:`repeat(${opt.cols},1fr)`,
                          gridTemplateRows:`repeat(${opt.rows},1fr)` }}>
                          {Array.from({length:opt.cols*opt.rows}).map((_,i)=>(
                            <div key={i} style={{ borderRadius:2,
                              background: grid.label===opt.label ? "#f59e0b80" : "#30363d" }} />
                          ))}
                        </div>
                        <div style={{ flex:1 }}>
                          <div style={{ fontWeight:700, fontSize:14,
                            color: grid.label===opt.label ? "#f59e0b" : "#e2e8f0" }}>
                            {opt.label} &nbsp;
                            <span style={{ fontWeight:400, fontSize:11, color:"#8b949e" }}>
                              {opt.count} items/screen
                            </span>
                          </div>
                          <div style={{ fontSize:11, color:"#8b949e" }}>{opt.desc}</div>
                        </div>
                        {grid.label===opt.label && <span style={{color:"#f59e0b"}}>✓</span>}
                      </div>
                    ))}
                  </div>
                </div>

                {/* Overflow stats */}
                <div style={{ padding:"14px", borderRadius:10, background:"#161b22",
                  border:"1px solid #21262d" }}>
                  <div style={{ fontSize:13, fontWeight:600, marginBottom:10 }}>Screen Distribution</div>
                  <div style={{ display:"flex", flexDirection:"column", gap:6 }}>
                    {screens.map((screenItems, i) => (
                      <div key={i} style={{ display:"flex", alignItems:"center", gap:8 }}>
                        <div style={{ fontSize:12, color:"#8b949e", width:58 }}>Screen {i+1}</div>
                        <div style={{ flex:1, height:8, borderRadius:4, background:"#21262d", overflow:"hidden" }}>
                          <div style={{ height:"100%", borderRadius:4,
                            width:`${(screenItems.length/itemsPerScreen)*100}%`,
                            background: i===activeScreen-1 ? "#f59e0b" : "#30363d",
                            transition:"width 0.3s" }} />
                        </div>
                        <div style={{ fontSize:11, color:"#8b949e", width:30, textAlign:"right" }}>
                          {screenItems.length}/{itemsPerScreen}
                        </div>
                      </div>
                    ))}
                  </div>
                  <div style={{ marginTop:10, fontSize:11, color:"#8b949e" }}>
                    {featured.length} items → <strong style={{color:"#f59e0b"}}>{totalScreens} screen{totalScreens!==1?"s":""}</strong> needed
                    &nbsp;·&nbsp; {itemsPerScreen} items each
                  </div>
                </div>
              </div>
            )}

            {tab==="settings" && (
              <div style={{ display:"flex", flexDirection:"column", gap:16 }}>
                <div>
                  <span style={labelStyle}>Venue Name</span>
                  <input value={venueName} onChange={e=>setVenueName(e.target.value)} style={inputStyle} />
                </div>
                <div>
                  <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between", marginBottom:8 }}>
                    <span style={labelStyle}>Happy Hour Prices</span>
                    <div onClick={()=>setIsHH(h=>!h)} style={{ width:42, height:22, borderRadius:11,
                      cursor:"pointer", transition:"all 0.2s",
                      background:isHH ? "#f59e0b" : "#21262d",
                      boxShadow:isHH ? "0 0 10px #f59e0b88" : "none" }}>
                      <div style={{ width:16, height:16, borderRadius:"50%", background:"#fff",
                        margin:3, marginLeft:isHH?23:3, transition:"margin-left 0.2s" }} />
                    </div>
                  </div>
                  <div style={{ fontSize:12, color:"#8b949e" }}>
                    Prices on display show 75% of regular price as a demo. In production, each item has a configurable HH price.
                  </div>
                </div>
                <div style={{ padding:14, borderRadius:10, background:"#161b22", border:"1px solid #21262d",
                  fontSize:12, color:"#8b949e", lineHeight:1.7 }}>
                  <div style={{ fontWeight:700, color:"#e2e8f0", marginBottom:6 }}>💡 How overflow works</div>
                  Each screen knows its <strong style={{color:"#f59e0b"}}>position number</strong> from its Screen ID. 
                  The display app receives the full item list and calculates which slice to show:
                  <br/><br/>
                  <code style={{ color:"#7df9ff", fontSize:11 }}>
                    start = (screenPos - 1) × itemsPerScreen
                  </code>
                  <br/>
                  No server-side per-screen logic needed — all screens sync from one payload.
                </div>
              </div>
            )}
          </div>

          {/* Push button */}
          <div style={{ padding:"14px 18px 18px", borderTop:"1px solid #21262d", flexShrink:0 }}>
            <button onClick={()=>setToast(`Pushed to ${totalScreens} screen${totalScreens!==1?"s":""}!`)}
              style={{ width:"100%", padding:"12px", borderRadius:10, border:"none",
                background:"linear-gradient(135deg,#f59e0b,#d97706)", color:"#000",
                fontFamily:"'Syne'", fontWeight:800, fontSize:15, cursor:"pointer",
                boxShadow:"0 0 16px #f59e0b60" }}>
              📺 &nbsp;Push to {totalScreens} Screen{totalScreens!==1?"s":""}
            </button>
          </div>
        </div>

        {/* ── RIGHT: Preview ── */}
        <div style={{ flex:1, display:"flex", flexDirection:"column", overflow:"hidden" }}>

          {/* Preview header */}
          <div style={{ padding:"12px 24px", borderBottom:"1px solid #21262d",
            display:"flex", alignItems:"center", gap:12, flexShrink:0 }}>
            <div style={{ width:8, height:8, borderRadius:"50%", background:"#22c55e",
              boxShadow:"0 0 6px #22c55e" }} className="pulse-dot" />
            <span style={{ fontSize:13, fontWeight:600, color:"#8b949e" }}>
              Live Preview — {grid.label} grid · {totalScreens} screen{totalScreens!==1?"s":""} · {featured.length} featured items
            </span>
            <div style={{ marginLeft:"auto", fontSize:12, color:"#8b949e",
              background:"#21262d", padding:"4px 12px", borderRadius:6 }}>
              1920 × 1080 · Screen {activeScreen}
            </div>
          </div>

          {/* Main preview */}
          <div style={{ flex:1, display:"flex", alignItems:"center", justifyContent:"center",
            padding:"20px 24px", overflow:"hidden" }}>
            <div style={{ width:960*previewScale, height:540*previewScale,
              borderRadius:10, overflow:"hidden", flexShrink:0,
              boxShadow:"0 20px 60px rgba(0,0,0,0.7), 0 0 0 1px #30363d" }}>
              <div style={{ width:960, height:540, transform:`scale(${previewScale})`,
                transformOrigin:"top left" }}>
                <PhotoBoard
                  items={currentItems}
                  grid={grid}
                  screenNum={activeScreen}
                  totalScreens={totalScreens}
                  venueName={venueName}
                  isHH={isHH}
                />
              </div>
            </div>
          </div>

          {/* Multi-screen selector row */}
          <div style={{ padding:"12px 24px 16px", borderTop:"1px solid #21262d",
            display:"flex", gap:16, alignItems:"flex-end", overflowX:"auto", flexShrink:0 }}>
            <div style={{ fontSize:11, color:"#8b949e", fontWeight:600,
              textTransform:"uppercase", letterSpacing:"0.06em", flexShrink:0, paddingBottom:22 }}>
              Screens
            </div>
            {screens.map((screenItems, i) => (
              <MiniScreen key={i}
                items={screenItems}
                grid={grid}
                screenNum={i+1}
                totalScreens={totalScreens}
                isActive={i+1===activeScreen}
                isHH={isHH}
                onClick={()=>setActiveScreen(i+1)}
              />
            ))}
            {screens.length === 0 && (
              <div style={{ fontSize:13, color:"#8b949e", fontStyle:"italic", paddingBottom:22 }}>
                Add items to see screen distribution
              </div>
            )}
          </div>
        </div>
      </div>

      {toast && <Toast msg={toast} onDone={()=>setToast(null)} />}
    </>
  );
}
