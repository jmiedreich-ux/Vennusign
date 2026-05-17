import { useState, useCallback, useEffect } from "react";

const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=Outfit:wght@300;400;500;600;700&family=DM+Mono:wght@400;500&display=swap');
  *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
  body { background:#0c0e12; font-family:'Outfit',sans-serif; color:#e8e3db; overflow:hidden; }
  ::-webkit-scrollbar { width:3px; } ::-webkit-scrollbar-track { background:transparent; }
  ::-webkit-scrollbar-thumb { background:#2a2e38; border-radius:2px; }
  input,select,textarea,button { font-family:'Outfit',sans-serif; }

  @keyframes fadeUp    { from{opacity:0;transform:translateY(8px)} to{opacity:1;transform:translateY(0)} }
  @keyframes fadeIn    { from{opacity:0} to{opacity:1} }
  @keyframes slideDown { from{opacity:0;transform:translateY(-6px)} to{opacity:1;transform:translateY(0)} }
  @keyframes toastIn   { from{opacity:0;transform:translateY(14px)} to{opacity:1;transform:translateY(0)} }
  @keyframes pulse     { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.5;transform:scale(1.25)} }
  @keyframes shimmer   { 0%{background-position:-300% 0} 100%{background-position:300% 0} }
  @keyframes hintSlide { from{opacity:0;transform:translateX(8px)} to{opacity:1;transform:translateX(0)} }

  .fade-up    { animation:fadeUp 0.3s ease forwards; }
  .fade-in    { animation:fadeIn 0.2s ease forwards; }
  .hint-slide { animation:hintSlide 0.35s ease forwards; }
  .pulse-dot  { animation:pulse 2s ease-in-out infinite; }
  .btn        { transition:all 0.15s ease; cursor:pointer; border:none; outline:none; }
  .btn:active { transform:scale(0.97); }
  .nav-btn    { transition:all 0.15s ease; }
  .upgrade-shimmer {
    background:linear-gradient(90deg,transparent 0%,rgba(240,165,0,0.06) 50%,transparent 100%);
    background-size:300% 100%; animation:shimmer 3s ease-in-out infinite;
  }
  .lock-hover { transition:all 0.15s; }
  .lock-hover:hover { filter:brightness(1.15); }
`;

/* ── Brand ───────────────────────────────────────────────────────────────── */
const V = {
  bg:"#0c0e12", surf:"#13161c", elev:"#1a1e26", border:"#23272f",
  amber:"#f0a500", amberDim:"#f0a50012", amberBord:"#f0a50028",
  green:"#22c55e", sage:"#5cb88a", sky:"#4ab3d4", muted:"#8892a0",
  text:"#e8e3db", textSoft:"#b8b3ab", red:"#ef4444",
};

/* ── Tier definitions ────────────────────────────────────────────────────── */
const TIERS = {
  starter:     { label:"Starter",            color:"#64748b", price:39  },
  rs:          { label:"Restaurant Starter", color:"#22c55e", price:49  },
  pro:         { label:"Pro",                color:"#f0a500", price:89  },
  business:    { label:"Business",           color:"#a855f7", price:179 },
};

/* ── Feature catalog with tier requirements ──────────────────────────────── */
const FEATURES = {
  neon_chalkboard:    { label:"Neon Chalkboard",        tier:"pro",      benefit:"The display that makes customers stop and look" },
  tap_list:           { label:"Tap List Boards",         tier:"pro",      benefit:"ABV, IBU, tasting notes — built for breweries" },
  happy_hour:         { label:"Happy Hour Scheduler",    tier:"pro",      benefit:"Prices switch automatically at 4pm — no staff needed" },
  meal_periods:       { label:"Meal Period Auto-Switch", tier:"rs",       benefit:"Breakfast to lunch to dinner — runs itself" },
  bilingual:          { label:"Bilingual Display",       tier:"rs",       benefit:"English + Chinese on the same board — out of the box" },
  ai_translation:     { label:"AI Menu Translation",     tier:"rs",       benefit:"Translate 50 items into Spanish for under $0.50" },
  quick_update:       { label:"Quick Update Mobile",     tier:"rs",       benefit:"86 an item from your phone in the kitchen — 2 taps" },
  pos_square:         { label:"Square Integration",      tier:"pro",      benefit:"Items sell out → board grays out in seconds" },
  pos_toast:          { label:"Toast Integration",       tier:"pro",      benefit:"Your POS and boards stay in perfect sync" },
  staff_app:          { label:"Staff Mobile App",        tier:"pro",      benefit:"Your whole team can update the board, anywhere" },
  ai_descriptions:    { label:"AI Menu Copy",            tier:"pro",      benefit:"Professional descriptions written in seconds" },
  analytics:          { label:"Full Analytics",          tier:"pro",      benefit:"See which items your boards are actually selling" },
  video_wall:         { label:"Video Wall",              tier:"pro",      benefit:"Span your menu across two screens as one canvas" },
  zone_builder:       { label:"Zone Layout Builder",     tier:"pro",      benefit:"Drag and drop your own layout — any arrangement" },
  ai_custom:          { label:"AI Display Builder",      tier:"business", benefit:"Describe your ideal board — AI builds it for you" },
  multi_location:     { label:"Multi-Location",          tier:"business", benefit:"Manage all your venues from one login" },
  white_label:        { label:"White-Label",             tier:"business", benefit:"Your brand, not Vennu's" },
};

/* ── Simulate current venue's tier ──────────────────────────────────────── */
const TIER_FEATURES = {
  starter:  ["meal_periods"],  // very limited for demo
  rs:       ["meal_periods","bilingual","ai_translation","quick_update"],
  pro:      Object.keys(FEATURES).filter(k=>FEATURES[k].tier!=="business"),
  business: Object.keys(FEATURES),
};

/* ── Menu data ───────────────────────────────────────────────────────────── */
const MENU = [
  { id:"s1", title:"Drinks", emoji:"🍸", items:[
    { id:"i1", name:"Old Fashioned",  desc:"Bourbon · bitters · orange", price:13.00, hhPrice:9.00,  available:true  },
    { id:"i2", name:"Aperol Spritz",  desc:"Aperol · prosecco · soda",   price:12.00, hhPrice:8.00,  available:true  },
    { id:"i3", name:"Hazy IPA",       desc:"Tropical & citrusy",         price:8.00,  hhPrice:5.00,  available:true  },
    { id:"i4", name:"Dark Porter",    desc:"Rich chocolate notes",        price:7.00,  hhPrice:4.50,  available:false },
  ]},
  { id:"s2", title:"Bar Bites", emoji:"🍟", items:[
    { id:"i5", name:"Truffle Fries",  desc:"Parmesan & herbs",           price:9.00,  hhPrice:6.00,  available:true  },
    { id:"i6", name:"Chicken Wings",  desc:"Choice of 3 sauces",         price:14.00, hhPrice:10.00, available:true  },
  ]},
];

/* ── Helpers ─────────────────────────────────────────────────────────────── */
const fmt = n=>`$${Number(n).toFixed(2)}`;
const inputSt = { background:V.elev, border:`1px solid ${V.border}`, borderRadius:8,
  padding:"8px 12px", color:V.text, fontSize:13, outline:"none", width:"100%" };

function Toggle({ value, onChange, color=V.amber }) {
  return (
    <div onClick={()=>onChange(!value)} style={{ width:38,height:21,borderRadius:11,
      cursor:"pointer", transition:"all 0.2s",
      background:value?color:V.elev,
      border:`1px solid ${value?color:V.border}`,
      boxShadow:value?`0 0 8px ${color}50`:"none",
      position:"relative", flexShrink:0 }}>
      <div style={{ width:15,height:15,borderRadius:"50%",background:"#fff",
        position:"absolute",top:2,left:value?20:2,transition:"left 0.18s",
        boxShadow:"0 1px 3px rgba(0,0,0,0.4)" }} />
    </div>
  );
}

function Toast({ msg, onDone }) {
  useEffect(()=>{const t=setTimeout(onDone,2500);return()=>clearTimeout(t);},[]);
  return (
    <div style={{ position:"fixed",bottom:24,right:24,zIndex:9999,
      background:`linear-gradient(135deg,${V.sage},#3a9e6d)`,color:"#fff",
      borderRadius:12,padding:"11px 20px",fontWeight:600,fontSize:13,
      boxShadow:"0 8px 28px rgba(0,0,0,0.5)",display:"flex",alignItems:"center",
      gap:9,animation:"toastIn 0.25s ease" }}>
      <span>✓</span>{msg}
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   UPGRADE COMPONENTS — The non-invasive upgrade system
════════════════════════════════════════════════════════════════════════════ */

/**
 * Tier badge — shown next to locked nav items and feature labels.
 * Small, informational, never alarming.
 */
function TierBadge({ tier }) {
  const t = TIERS[tier];
  if (!t) return null;
  return (
    <span style={{ fontSize:9, padding:"1px 6px", borderRadius:4,
      background:`${t.color}18`, color:t.color,
      border:`1px solid ${t.color}35`, fontWeight:700,
      letterSpacing:"0.04em", flexShrink:0 }}>
      {t.label.toUpperCase()}
    </span>
  );
}

/**
 * Locked nav item — visible but dimmed, with tier badge.
 * Users can see the feature exists; clicking shows a gentle explanation.
 */
function LockedNavItem({ icon, label, featureKey, onLockedClick }) {
  const feat = FEATURES[featureKey];
  return (
    <div onClick={()=>onLockedClick(featureKey)}
      className="lock-hover"
      style={{ display:"flex",alignItems:"center",gap:10,padding:"9px 12px",
        borderRadius:8,cursor:"pointer",opacity:0.5,
        border:"2px solid transparent",
        userSelect:"none" }}>
      <span style={{ fontSize:15 }}>{icon}</span>
      <span style={{ fontSize:13,fontWeight:400,color:V.muted,flex:1 }}>{label}</span>
      <TierBadge tier={feat?.tier} />
    </div>
  );
}

/**
 * Locked section — replaces a locked panel's content.
 * Shows what the feature does with a single soft CTA.
 * Never blocking — always shows context about what they'd get.
 */
function LockedSection({ featureKey, onUpgrade }) {
  const feat = FEATURES[featureKey];
  if (!feat) return null;
  const tier = TIERS[feat.tier];

  return (
    <div className="fade-up" style={{ display:"flex",flexDirection:"column",
      alignItems:"center",justifyContent:"center",padding:"48px 32px",
      textAlign:"center",gap:16 }}>
      {/* Soft lock icon */}
      <div style={{ width:52,height:52,borderRadius:16,
        background:`${tier.color}12`,border:`1px solid ${tier.color}30`,
        display:"flex",alignItems:"center",justifyContent:"center",fontSize:22 }}>
        🔓
      </div>
      <div>
        <div style={{ fontFamily:"'Playfair Display'",fontWeight:700,fontSize:20,
          color:V.text,marginBottom:6 }}>{feat.label}</div>
        <div style={{ fontSize:14,color:V.muted,maxWidth:360,lineHeight:1.7 }}>
          {feat.benefit}
        </div>
      </div>
      <button className="btn" onClick={()=>onUpgrade(featureKey)}
        style={{ padding:"10px 24px",borderRadius:9,
          background:`${tier.color}20`,color:tier.color,
          border:`1px solid ${tier.color}40`,
          fontWeight:700,fontSize:13,cursor:"pointer" }}>
        Unlock with {tier.label} · ${tier.price}/mo
      </button>
      <div style={{ fontSize:11,color:V.muted }}>
        14-day free trial · cancel anytime
      </div>
    </div>
  );
}

/**
 * Inline feature hint — appears at the bottom of a panel when a related
 * locked feature would be relevant. One at a time, dismissible.
 */
function FeatureHint({ featureKey, onDismiss, onUpgrade }) {
  const feat = FEATURES[featureKey];
  if (!feat) return null;
  const tier = TIERS[feat.tier];

  return (
    <div className="hint-slide upgrade-shimmer"
      style={{ display:"flex",alignItems:"center",gap:12,
        padding:"12px 16px",borderRadius:10,
        background:`${tier.color}08`,
        border:`1px solid ${tier.color}25`,
        position:"relative" }}>
      {/* Color accent bar */}
      <div style={{ position:"absolute",left:0,top:0,bottom:0,width:3,
        borderRadius:"10px 0 0 10px",background:tier.color,opacity:0.6 }} />
      <div style={{ fontSize:16,flexShrink:0 }}>✨</div>
      <div style={{ flex:1,minWidth:0 }}>
        <div style={{ fontSize:13,fontWeight:600,color:V.text }}>{feat.label}</div>
        <div style={{ fontSize:12,color:V.muted,marginTop:1 }}>{feat.benefit}</div>
      </div>
      <div style={{ display:"flex",gap:8,alignItems:"center",flexShrink:0 }}>
        <button className="btn" onClick={()=>onUpgrade(featureKey)}
          style={{ fontSize:12,padding:"5px 12px",borderRadius:7,
            background:`${tier.color}20`,color:tier.color,
            border:`1px solid ${tier.color}40`,fontWeight:700,cursor:"pointer" }}>
          Learn more
        </button>
        <button className="btn" onClick={onDismiss}
          style={{ fontSize:14,background:"transparent",color:V.muted,
            cursor:"pointer",padding:"4px 6px",lineHeight:1 }}>
          ×
        </button>
      </div>
    </div>
  );
}

/**
 * Upgrade modal — slides up from bottom when user clicks a locked feature
 * or "Learn more". Full info but dismissible with one click outside.
 */
function UpgradeModal({ featureKey, currentTier, onClose, onUpgrade }) {
  const feat = FEATURES[featureKey];
  if (!feat) return null;
  const tier = TIERS[feat.tier];
  const ct = TIERS[currentTier];

  // Features unlocked at this tier
  const newFeatures = Object.entries(FEATURES)
    .filter(([,f])=>f.tier===feat.tier)
    .map(([,f])=>f);

  return (
    <>
      {/* Backdrop — click to close */}
      <div onClick={onClose} style={{ position:"fixed",inset:0,zIndex:100,
        background:"rgba(0,0,0,0.5)",backdropFilter:"blur(2px)" }} />
      {/* Sheet */}
      <div style={{ position:"fixed",bottom:0,left:0,right:0,zIndex:101,
        background:V.surf,borderTop:`2px solid ${tier.color}40`,
        borderRadius:"20px 20px 0 0",padding:"28px 32px 36px",
        animation:"slideDown 0.25s ease",
        boxShadow:`0 -20px 60px rgba(0,0,0,0.5), 0 0 0 1px ${tier.color}20` }}>

        {/* Handle */}
        <div style={{ width:40,height:4,borderRadius:2,background:V.border,
          margin:"0 auto 20px" }} />

        <div style={{ display:"flex",alignItems:"flex-start",gap:24,
          maxWidth:700,margin:"0 auto" }}>
          {/* Icon */}
          <div style={{ width:64,height:64,borderRadius:18,flexShrink:0,
            background:`${tier.color}15`,border:`1px solid ${tier.color}30`,
            display:"flex",alignItems:"center",justifyContent:"center",fontSize:28 }}>
            ✨
          </div>

          {/* Content */}
          <div style={{ flex:1 }}>
            <div style={{ display:"flex",alignItems:"center",gap:10,marginBottom:6 }}>
              <div style={{ fontFamily:"'Playfair Display'",fontWeight:700,fontSize:22 }}>
                {feat.label}
              </div>
              <TierBadge tier={feat.tier} />
            </div>
            <div style={{ fontSize:14,color:V.muted,lineHeight:1.7,marginBottom:16 }}>
              {feat.benefit}
            </div>

            {/* What else you get */}
            <div style={{ fontSize:11,fontWeight:700,color:V.muted,
              textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:8 }}>
              Everything in {tier.label}
            </div>
            <div style={{ display:"flex",flexWrap:"wrap",gap:6,marginBottom:20 }}>
              {newFeatures.map(f=>(
                <div key={f.label} style={{ fontSize:12,padding:"4px 10px",borderRadius:6,
                  background:`${tier.color}12`,color:tier.color,
                  border:`1px solid ${tier.color}30`,fontWeight:500 }}>
                  {f.label}
                </div>
              ))}
            </div>

            {/* CTA */}
            <div style={{ display:"flex",alignItems:"center",gap:12 }}>
              <button className="btn" onClick={()=>onUpgrade(featureKey)}
                style={{ padding:"12px 28px",borderRadius:10,
                  background:tier.color,color:feat.tier==="business"?"#fff":"#000",
                  fontWeight:700,fontSize:14,cursor:"pointer",
                  boxShadow:`0 0 20px ${tier.color}40` }}>
                Upgrade to {tier.label} — ${tier.price}/mo
              </button>
              <div style={{ fontSize:12,color:V.muted }}>
                {ct && <span>You're on <strong style={{color:ct.color}}>{ct.label}</strong> · </span>}
                14-day free trial · no commitment
              </div>
              <button className="btn" onClick={onClose}
                style={{ marginLeft:"auto",fontSize:12,color:V.muted,
                  background:"transparent",cursor:"pointer",padding:"4px 10px" }}>
                Maybe later
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

/**
 * Sidebar upgrade nudge — rotates through most relevant locked features.
 * One sentence, one action. Lives quietly at the bottom of the sidebar.
 */
function SidebarNudge({ locked, onLearnMore, dismissed, onDismiss }) {
  const [idx, setIdx] = useState(0);
  const visible = locked.filter(k=>!dismissed.includes(k));

  useEffect(()=>{
    if (visible.length <= 1) return;
    const t = setInterval(()=>setIdx(i=>(i+1)%visible.length), 7000);
    return ()=>clearInterval(t);
  },[visible.length]);

  if (!visible.length) return null;
  const key = visible[idx % visible.length];
  const feat = FEATURES[key];
  if (!feat) return null;
  const tier = TIERS[feat.tier];

  return (
    <div style={{ margin:"0 10px 12px",borderRadius:10,overflow:"hidden",
      border:`1px solid ${tier.color}25`,position:"relative" }}>
      <div className="upgrade-shimmer" style={{ padding:"10px 12px",
        background:`${tier.color}08` }}>
        <div style={{ display:"flex",alignItems:"flex-start",gap:8 }}>
          <span style={{ fontSize:13,flexShrink:0 }}>✨</span>
          <div style={{ flex:1,minWidth:0 }}>
            <div style={{ fontSize:11,fontWeight:700,color:tier.color,marginBottom:3 }}>
              {feat.label}
            </div>
            <div style={{ fontSize:10,color:V.muted,lineHeight:1.4,marginBottom:8 }}>
              {feat.benefit}
            </div>
            <button className="btn" onClick={()=>onLearnMore(key)}
              style={{ fontSize:10,padding:"3px 9px",borderRadius:5,
                background:`${tier.color}20`,color:tier.color,
                border:`1px solid ${tier.color}40`,fontWeight:700,cursor:"pointer" }}>
              See how →
            </button>
          </div>
          <button className="btn" onClick={()=>onDismiss(key)}
            style={{ fontSize:12,color:V.muted,background:"transparent",
              cursor:"pointer",flexShrink:0,lineHeight:1 }}>×</button>
        </div>
        {visible.length > 1 && (
          <div style={{ display:"flex",gap:3,marginTop:8,justifyContent:"center" }}>
            {visible.map((_,i)=>(
              <div key={i} style={{ width:i===idx%visible.length?14:4,height:3,
                borderRadius:2,transition:"all 0.3s",
                background:i===idx%visible.length?tier.color:`${tier.color}40` }} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   MENU EDITOR — with inline hints for relevant locked features
════════════════════════════════════════════════════════════════════════════ */
function MenuEditor({ sections, setSections, features, showHint, shownHints, toast }) {
  const [expanded, setExpanded] = useState("s1");
  const [editId, setEditId] = useState(null);
  const [editData, setEditData] = useState({});

  const hasHH = features.includes("happy_hour");
  const hasPOS = features.includes("pos_square");

  const toggleAvail = (id) => {
    setSections(secs=>secs.map(s=>({
      ...s, items:s.items.map(i=>i.id===id?{...i,available:!i.available}:i)
    })));
    toast("Updated · Synced to screens");
  };

  const startEdit = (item) => { setEditId(item.id); setEditData({...item}); };
  const save = () => {
    setSections(secs=>secs.map(s=>({
      ...s,items:s.items.map(i=>i.id===editId?{...editData}:i)
    })));
    setEditId(null);
    toast("Saved · Synced to screens");
  };

  return (
    <div style={{ display:"flex",flexDirection:"column",gap:10 }}>
      {sections.map(sec=>(
        <div key={sec.id} style={{ background:V.surf,borderRadius:12,overflow:"hidden",
          border:`1px solid ${V.border}` }}>
          <div onClick={()=>setExpanded(expanded===sec.id?null:sec.id)}
            style={{ display:"flex",alignItems:"center",padding:"13px 18px",
              cursor:"pointer",borderBottom:expanded===sec.id?`1px solid ${V.border}`:"none",
              background:expanded===sec.id?V.elev:"transparent" }}>
            <span style={{ fontSize:18,marginRight:10 }}>{sec.emoji}</span>
            <span style={{ fontFamily:"'Playfair Display'",fontWeight:600,fontSize:16,flex:1 }}>
              {sec.title}
            </span>
            <span style={{ fontSize:12,color:V.muted }}>{sec.items.length} items</span>
            <span style={{ color:V.muted,marginLeft:10,fontSize:11 }}>
              {expanded===sec.id?"▲":"▼"}
            </span>
          </div>

          {expanded===sec.id && (
            <div style={{ padding:"8px 12px 12px" }}>
              {sec.items.map(item=>(
                <div key={item.id} style={{ padding:"9px 8px",borderRadius:8,marginBottom:3,
                  background:editId===item.id?V.elev:"transparent",
                  border:editId===item.id?`1px solid ${V.amberBord}`:"1px solid transparent",
                  opacity:item.available?1:0.5,transition:"all 0.15s" }}>
                  {editId===item.id ? (
                    <div style={{ display:"flex",flexDirection:"column",gap:9 }}>
                      <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr 1fr",gap:9 }}>
                        <div>
                          <div style={{ fontSize:10,fontWeight:600,color:V.muted,
                            textTransform:"uppercase",letterSpacing:"0.06em",marginBottom:5 }}>Name</div>
                          <input value={editData.name}
                            onChange={e=>setEditData(d=>({...d,name:e.target.value}))}
                            style={inputSt} />
                        </div>
                        <div>
                          <div style={{ fontSize:10,fontWeight:600,color:V.muted,
                            textTransform:"uppercase",letterSpacing:"0.06em",marginBottom:5 }}>Price</div>
                          <input type="number" value={editData.price}
                            onChange={e=>setEditData(d=>({...d,price:parseFloat(e.target.value)}))}
                            style={inputSt} />
                        </div>
                        <div>
                          <div style={{ fontSize:10,fontWeight:600,color:V.muted,
                            textTransform:"uppercase",letterSpacing:"0.06em",marginBottom:5,
                            display:"flex",alignItems:"center",gap:6 }}>
                            HH Price
                            {!hasHH && <TierBadge tier="pro" />}
                          </div>
                          <input type="number" value={editData.hhPrice}
                            onChange={e=>setEditData(d=>({...d,hhPrice:parseFloat(e.target.value)}))}
                            style={{ ...inputSt,opacity:hasHH?1:0.4 }}
                            disabled={!hasHH}
                            title={!hasHH?"Requires Pro — set up Happy Hour pricing":"Happy Hour price"} />
                        </div>
                      </div>
                      <div style={{ display:"flex",gap:8,justifyContent:"flex-end" }}>
                        <button className="btn" onClick={()=>setEditId(null)}
                          style={{ padding:"7px 14px",borderRadius:7,background:V.elev,
                            color:V.muted,fontSize:12,fontWeight:600,cursor:"pointer" }}>
                          Cancel
                        </button>
                        <button className="btn" onClick={save}
                          style={{ padding:"7px 14px",borderRadius:7,background:V.amber,
                            color:"#000",fontSize:12,fontWeight:700,cursor:"pointer" }}>
                          Save & Sync
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div style={{ display:"flex",alignItems:"center",gap:10 }}>
                      <div style={{ flex:1,minWidth:0 }}>
                        <div style={{ fontWeight:600,fontSize:14 }}>{item.name}</div>
                        <div style={{ fontSize:12,color:V.muted }}>{item.desc}</div>
                      </div>
                      <div style={{ textAlign:"right",minWidth:70 }}>
                        <div style={{ fontWeight:700,color:V.amber }}>{fmt(item.price)}</div>
                        {hasHH && <div style={{ fontSize:11,color:`${V.amber}70` }}>
                          HH: {fmt(item.hhPrice)}
                        </div>}
                      </div>
                      <button className="btn" onClick={()=>toggleAvail(item.id)}
                        style={{ fontSize:11,padding:"3px 9px",borderRadius:5,
                          background:item.available?"#22c55e20":"#ef444420",
                          color:item.available?"#22c55e":"#ef4444",
                          border:"none",cursor:"pointer",fontWeight:600 }}>
                        {item.available?"Live":"Off"}
                      </button>
                      <button className="btn" onClick={()=>startEdit(item)}
                        style={{ background:"transparent",border:"none",
                          color:V.muted,cursor:"pointer",fontSize:14 }}>✏️</button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      ))}

      {/* POS hint — contextually relevant when editing a menu */}
      {!hasPOS && !shownHints.includes("pos_square") && (
        <FeatureHint featureKey="pos_square"
          onDismiss={()=>showHint("pos_square","dismiss")}
          onUpgrade={()=>showHint("pos_square","upgrade")} />
      )}
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   SCHEDULE TAB — with locked happy hour section
════════════════════════════════════════════════════════════════════════════ */
function ScheduleTab({ features, onLockedClick, toast }) {
  const hasMeal = features.includes("meal_periods");
  const hasHH   = features.includes("happy_hour");
  const [mealEnabled, setMealEnabled] = useState(true);

  const MEALS = [
    { id:"m1",name:"Breakfast",icon:"☀️",start:"06:00",end:"11:00",active:false,color:"#f0a500" },
    { id:"m2",name:"Lunch",    icon:"🌤", start:"11:00",end:"15:00",active:true, color:"#22c55e" },
    { id:"m3",name:"Dinner",   icon:"🌙",start:"17:00",end:"22:00",active:false,color:"#4ab3d4" },
  ];

  return (
    <div style={{ display:"flex",flexDirection:"column",gap:14 }}>
      {/* Meal periods */}
      <div style={{ background:V.surf,border:`1px solid ${V.border}`,borderRadius:12,
        overflow:"hidden" }}>
        <div style={{ padding:"14px 18px",borderBottom:`1px solid ${V.border}`,
          display:"flex",alignItems:"center",gap:10 }}>
          <span style={{ fontFamily:"'Playfair Display'",fontWeight:600,fontSize:16 }}>
            Meal Periods
          </span>
          {!hasMeal && <TierBadge tier="rs" />}
        </div>

        {hasMeal ? (
          <div style={{ padding:"14px 18px",display:"flex",flexDirection:"column",gap:10 }}>
            {MEALS.map(m=>(
              <div key={m.id} style={{ display:"flex",alignItems:"center",gap:12,
                padding:"12px 14px",borderRadius:9,
                background:m.active?`${m.color}10`:V.elev,
                border:`1px solid ${m.active?m.color+"30":V.border}` }}>
                <span style={{ fontSize:18 }}>{m.icon}</span>
                <div style={{ flex:1 }}>
                  <div style={{ fontWeight:600,fontSize:14,color:m.active?m.color:V.text }}>
                    {m.name}
                  </div>
                  <div style={{ fontFamily:"'DM Mono'",fontSize:11,color:V.muted }}>
                    {m.start} – {m.end}
                  </div>
                </div>
                <Toggle value={m.active} onChange={()=>toast(`${m.name} period updated`)}
                  color={m.color} />
              </div>
            ))}
          </div>
        ) : (
          <div onClick={()=>onLockedClick("meal_periods")}
            style={{ padding:"20px 18px",cursor:"pointer",opacity:0.7 }}>
            <div style={{ display:"flex",flexDirection:"column",gap:8 }}>
              {["Breakfast · 6am–11am","Lunch · 11am–3pm","Dinner · 5pm–close"].map(m=>(
                <div key={m} style={{ padding:"10px 14px",borderRadius:8,
                  background:V.elev,border:`1px solid ${V.border}`,
                  fontSize:13,color:V.muted,filter:"blur(0.3px)" }}>
                  {m}
                </div>
              ))}
            </div>
            <div style={{ marginTop:12,display:"flex",alignItems:"center",gap:8 }}>
              <span style={{ fontSize:13,color:V.muted }}>
                Auto-switch your menu throughout the day
              </span>
              <TierBadge tier="rs" />
            </div>
          </div>
        )}
      </div>

      {/* Happy hour */}
      <div style={{ background:V.surf,border:`1px solid ${V.border}`,borderRadius:12,
        overflow:"hidden" }}>
        <div style={{ padding:"14px 18px",borderBottom:`1px solid ${V.border}`,
          display:"flex",alignItems:"center",gap:10 }}>
          <span style={{ fontFamily:"'Playfair Display'",fontWeight:600,fontSize:16 }}>
            Happy Hour
          </span>
          {!hasHH && <TierBadge tier="pro" />}
        </div>

        {hasHH ? (
          <div style={{ padding:"14px 18px" }}>
            <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:12,marginBottom:12 }}>
              {[{l:"Start Time",v:"16:00"},{l:"End Time",v:"19:00"}].map(f=>(
                <div key={f.l}>
                  <div style={{ fontSize:10,fontWeight:600,color:V.muted,
                    textTransform:"uppercase",letterSpacing:"0.06em",marginBottom:5 }}>{f.l}</div>
                  <input type="time" defaultValue={f.v}
                    style={{ ...inputSt,colorScheme:"dark" }} />
                </div>
              ))}
            </div>
            <button className="btn" onClick={()=>toast("Happy hour saved!")}
              style={{ padding:"10px",borderRadius:8,background:V.amber,color:"#000",
                fontWeight:700,fontSize:13,border:"none",cursor:"pointer",width:"100%" }}>
              Save Happy Hour
            </button>
          </div>
        ) : (
          <div onClick={()=>onLockedClick("happy_hour")}
            style={{ padding:"20px 18px",cursor:"pointer" }}>
            <div style={{ padding:"14px",borderRadius:9,background:V.elev,
              border:`1px solid ${V.border}`,opacity:0.6,filter:"blur(0.3px)",
              display:"flex",gap:12 }}>
              <div style={{ flex:1 }}>
                <div style={{ fontSize:12,fontWeight:600,color:V.muted }}>Start · 4:00 PM</div>
                <div style={{ fontSize:12,fontWeight:600,color:V.muted }}>End · 7:00 PM</div>
              </div>
              <div style={{ fontSize:12,color:V.muted }}>Mon–Fri</div>
            </div>
            <div style={{ marginTop:12,display:"flex",alignItems:"center",gap:8 }}>
              <span style={{ fontSize:13,color:V.muted }}>
                Prices switch automatically — no staff needed
              </span>
              <TierBadge tier="pro" />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   DISPLAYS TAB
════════════════════════════════════════════════════════════════════════════ */
function DisplaysTab({ features, onLockedClick, shownHints, showHint }) {
  const LAYOUTS = [
    { id:"photo",      label:"Photo Grid",      emoji:"📸", key:null,           desc:"QSR & photo menus" },
    { id:"diner",      label:"Classic Diner",   emoji:"📋", key:null,           desc:"Text-only, clean" },
    { id:"chalkboard", label:"Neon Chalkboard", emoji:"🪩", key:"neon_chalkboard",desc:"Bars & restaurants" },
    { id:"taplist",    label:"Tap Strips",      emoji:"🍺", key:"tap_list",      desc:"Brewery & bar" },
    { id:"zone",       label:"Zone Builder",    emoji:"⊞",  key:"zone_builder",  desc:"Custom arrangement" },
    { id:"ai",         label:"AI Builder",      emoji:"✨",  key:"ai_custom",     desc:"Describe → generate" },
  ];

  const [active, setActive] = useState("photo");

  return (
    <div style={{ display:"flex",flexDirection:"column",gap:14 }}>
      <div style={{ display:"grid",gridTemplateColumns:"repeat(3,1fr)",gap:10 }}>
        {LAYOUTS.map(l=>{
          const locked = l.key && !features.includes(l.key);
          const tier = l.key ? FEATURES[l.key]?.tier : null;
          return (
            <div key={l.id}
              onClick={()=>locked?onLockedClick(l.key):setActive(l.id)}
              style={{ padding:"14px",borderRadius:10,cursor:"pointer",
                background:active===l.id?V.amberDim:V.surf,
                border:`1px solid ${active===l.id?V.amberBord:V.border}`,
                opacity:locked?0.6:1,transition:"all 0.15s",
                position:"relative" }}>
              <div style={{ fontSize:22,marginBottom:6 }}>{l.emoji}</div>
              <div style={{ fontWeight:600,fontSize:13,marginBottom:2,
                display:"flex",alignItems:"center",gap:7,flexWrap:"wrap" }}>
                {l.label}
                {locked && <TierBadge tier={tier} />}
              </div>
              <div style={{ fontSize:11,color:V.muted }}>{l.desc}</div>
              {locked && (
                <div style={{ position:"absolute",inset:0,borderRadius:10,
                  display:"flex",alignItems:"center",justifyContent:"center",
                  background:"rgba(12,14,18,0.3)" }}>
                  <span style={{ fontSize:16,opacity:0.6 }}>🔓</span>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Zone builder hint */}
      {!features.includes("zone_builder") && !shownHints.includes("zone_builder") && (
        <FeatureHint featureKey="zone_builder"
          onDismiss={()=>showHint("zone_builder","dismiss")}
          onUpgrade={()=>showHint("zone_builder","upgrade")} />
      )}
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   INTEGRATIONS TAB
════════════════════════════════════════════════════════════════════════════ */
function IntegrationsTab({ features, onLockedClick }) {
  const INTEGRATIONS = [
    { id:"pos_square",  label:"Square",  emoji:"◻", desc:"Sync menu, auto-86 on sellout", key:"pos_square"  },
    { id:"pos_toast",   label:"Toast",   emoji:"🍞", desc:"Full-service POS sync",          key:"pos_toast"   },
    { id:"pos_clover",  label:"Clover",  emoji:"🍀", desc:"Mid-market POS coverage",        key:"pos_clover"  },
    { id:"ai_desc",     label:"Claude AI",emoji:"✨", desc:"Auto-write menu descriptions",  key:"ai_descriptions"},
    { id:"multilang",   label:"AI Translation",emoji:"🌏",desc:"Translate menus automatically",key:"ai_translation"},
  ];

  return (
    <div style={{ display:"flex",flexDirection:"column",gap:10 }}>
      {INTEGRATIONS.map(integ=>{
        const locked = !features.includes(integ.key);
        const tier = FEATURES[integ.key]?.tier;
        return (
          <div key={integ.id}
            onClick={locked?()=>onLockedClick(integ.key):undefined}
            style={{ display:"flex",alignItems:"center",gap:14,padding:"14px 18px",
              borderRadius:10,background:V.surf,border:`1px solid ${V.border}`,
              cursor:locked?"pointer":"default",
              opacity:locked?0.65:1,transition:"opacity 0.15s" }}>
            <div style={{ width:40,height:40,borderRadius:10,flexShrink:0,
              background:locked?V.elev:V.amberDim,
              border:`1px solid ${locked?V.border:V.amberBord}`,
              display:"flex",alignItems:"center",justifyContent:"center",fontSize:18 }}>
              {integ.emoji}
            </div>
            <div style={{ flex:1 }}>
              <div style={{ display:"flex",alignItems:"center",gap:8,marginBottom:3 }}>
                <span style={{ fontWeight:600,fontSize:14 }}>{integ.label}</span>
                {locked && <TierBadge tier={tier} />}
              </div>
              <div style={{ fontSize:12,color:V.muted }}>{integ.desc}</div>
            </div>
            {locked ? (
              <span style={{ fontSize:12,color:V.muted,padding:"5px 12px",
                borderRadius:7,background:V.elev,border:`1px solid ${V.border}` }}>
                Unlock →
              </span>
            ) : (
              <span style={{ fontSize:12,color:V.green,padding:"5px 12px",
                borderRadius:7,background:"#22c55e15",border:"1px solid #22c55e30",
                fontWeight:700 }}>
                Connect
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}

/* ════════════════════════════════════════════════════════════════════════════
   MAIN APP
════════════════════════════════════════════════════════════════════════════ */
// Simulate being on "Restaurant Starter" — change this to see different experiences
const CURRENT_TIER = "rs";

const NAV_ITEMS = [
  { id:"menu",         icon:"🍽",  label:"Menu Editor",   feature:null            },
  { id:"schedule",     icon:"🕐",  label:"Scheduling",    feature:null            },
  { id:"displays",     icon:"📺",  label:"Displays",      feature:null            },
  { id:"pos",          icon:"🔗",  label:"POS & AI",      feature:null            },
  { id:"analytics",    icon:"📊",  label:"Analytics",     feature:"analytics"     },
  { id:"mobile",       icon:"📱",  label:"Mobile App",    feature:"staff_app"     },
  { id:"multilocal",   icon:"🗺",  label:"Multi-Location",feature:"multi_location"},
];

export default function VennuAdmin() {
  const [tab, setTab]         = useState("menu");
  const [sections, setSections] = useState(MENU);
  const [toast, setToast]     = useState(null);
  const [modal, setModal]     = useState(null);        // featureKey or null
  const [dismissed, setDismissed] = useState([]);      // dismissed sidebar nudges
  const [shownHints, setShownHints] = useState([]);    // dismissed inline hints

  const features = TIER_FEATURES[CURRENT_TIER] || [];
  const tierInfo = TIERS[CURRENT_TIER];

  // All locked features
  const lockedFeatures = Object.keys(FEATURES).filter(k=>!features.includes(k));

  const showToast = useCallback(msg=>setToast(msg),[]);

  const handleLockedClick = (featureKey) => setModal(featureKey);

  const handleHint = (featureKey, action) => {
    if (action==="dismiss") setShownHints(h=>[...h,featureKey]);
    if (action==="upgrade") setModal(featureKey);
  };

  const handleUpgrade = (featureKey) => {
    setModal(null);
    showToast(`Opening upgrade to ${TIERS[FEATURES[featureKey]?.tier]?.label}...`);
  };

  return (
    <>
      <style>{STYLES}</style>
      <div style={{ display:"flex",height:"100vh",overflow:"hidden",background:V.bg }}>

        {/* ── Sidebar ── */}
        <div style={{ width:224,background:V.surf,borderRight:`1px solid ${V.border}`,
          display:"flex",flexDirection:"column",flexShrink:0 }}>
          {/* Logo + tier */}
          <div style={{ padding:"18px 20px 14px",borderBottom:`1px solid ${V.border}` }}>
            <div style={{ fontFamily:"'Playfair Display'",fontWeight:700,fontSize:24,
              color:V.amber,marginBottom:2 }}>vennu</div>
            <div style={{ display:"flex",alignItems:"center",gap:7 }}>
              <div style={{ width:6,height:6,borderRadius:"50%",
                background:tierInfo?.color,
                boxShadow:`0 0 5px ${tierInfo?.color}` }} />
              <span style={{ fontSize:11,color:tierInfo?.color,fontWeight:600 }}>
                {tierInfo?.label}
              </span>
              <span style={{ fontSize:11,color:V.muted }}>
                · ${tierInfo?.price}/mo
              </span>
            </div>
          </div>

          {/* Nav */}
          <div style={{ padding:"10px 10px",flex:1,overflowY:"auto" }}>
            {NAV_ITEMS.map(n=>{
              const locked = n.feature && !features.includes(n.feature);
              if (locked) {
                return (
                  <LockedNavItem key={n.id} icon={n.icon} label={n.label}
                    featureKey={n.feature} onLockedClick={handleLockedClick} />
                );
              }
              return (
                <button key={n.id} className="btn nav-btn" onClick={()=>setTab(n.id)}
                  style={{ display:"flex",alignItems:"center",gap:10,width:"100%",
                    padding:"10px 12px",borderRadius:8,border:"none",cursor:"pointer",
                    background:tab===n.id?V.amberDim:"transparent",
                    borderLeft:tab===n.id?`2px solid ${V.amber}`:"2px solid transparent",
                    color:tab===n.id?V.amber:V.muted,
                    fontSize:13,fontWeight:tab===n.id?600:400,
                    marginBottom:2,textAlign:"left",fontFamily:"'Outfit'" }}>
                  <span style={{ fontSize:15 }}>{n.icon}</span>
                  {n.label}
                </button>
              );
            })}
          </div>

          {/* Sidebar nudge — rotates through locked features */}
          <SidebarNudge
            locked={lockedFeatures}
            dismissed={dismissed}
            onDismiss={k=>setDismissed(d=>[...d,k])}
            onLearnMore={k=>setModal(k)}
          />

          {/* Upgrade CTA — only if not Business */}
          {CURRENT_TIER !== "business" && (
            <div style={{ padding:"12px 14px",borderTop:`1px solid ${V.border}` }}>
              <button className="btn" onClick={()=>setModal(
                lockedFeatures.find(k=>FEATURES[k]?.tier==="pro") || lockedFeatures[0]
              )}
                style={{ width:"100%",padding:"10px",borderRadius:9,
                  background:V.amberDim,color:V.amber,
                  border:`1px solid ${V.amberBord}`,
                  fontWeight:700,fontSize:12,cursor:"pointer" }}>
                View upgrade options
              </button>
            </div>
          )}
        </div>

        {/* ── Main panel ── */}
        <div style={{ flex:1,display:"flex",flexDirection:"column",overflow:"hidden" }}>
          {/* Top bar */}
          <div style={{ padding:"14px 24px",borderBottom:`1px solid ${V.border}`,
            display:"flex",alignItems:"center",gap:12,flexShrink:0,background:V.surf }}>
            <div style={{ fontFamily:"'Playfair Display'",fontWeight:600,fontSize:18 }}>
              {NAV_ITEMS.find(n=>n.id===tab)?.label || "Vennu"}
            </div>
            <div style={{ marginLeft:"auto",display:"flex",gap:10,alignItems:"center" }}>
              <div style={{ fontSize:11,color:V.green,display:"flex",
                alignItems:"center",gap:5 }}>
                <div style={{ width:6,height:6,borderRadius:"50%",
                  background:V.green,boxShadow:`0 0 5px ${V.green}` }}
                  className="pulse-dot" />
                2 screens live
              </div>
              <button className="btn"
                onClick={()=>showToast("Content pushed to all screens!")}
                style={{ padding:"8px 16px",borderRadius:8,background:V.amber,
                  color:"#000",fontWeight:700,fontSize:12,cursor:"pointer" }}>
                Push to Screens
              </button>
            </div>
          </div>

          {/* Content */}
          <div style={{ flex:1,overflowY:"auto",padding:"20px 24px" }}>
            {tab==="menu" && (
              <MenuEditor sections={sections} setSections={setSections}
                features={features} showHint={handleHint}
                shownHints={shownHints} toast={showToast} />
            )}
            {tab==="schedule" && (
              <ScheduleTab features={features}
                onLockedClick={handleLockedClick} toast={showToast} />
            )}
            {tab==="displays" && (
              <DisplaysTab features={features}
                onLockedClick={handleLockedClick}
                shownHints={shownHints} showHint={handleHint} />
            )}
            {tab==="pos" && (
              <IntegrationsTab features={features}
                onLockedClick={handleLockedClick} />
            )}
            {tab==="analytics" && (
              <LockedSection featureKey="analytics" onUpgrade={handleUpgrade} />
            )}
            {tab==="mobile" && (
              <LockedSection featureKey="staff_app" onUpgrade={handleUpgrade} />
            )}
            {tab==="multilocal" && (
              <LockedSection featureKey="multi_location" onUpgrade={handleUpgrade} />
            )}
          </div>
        </div>
      </div>

      {/* Upgrade modal */}
      {modal && (
        <UpgradeModal featureKey={modal} currentTier={CURRENT_TIER}
          onClose={()=>setModal(null)} onUpgrade={handleUpgrade} />
      )}

      {toast && <Toast msg={toast} onDone={()=>setToast(null)} />}
    </>
  );
}
