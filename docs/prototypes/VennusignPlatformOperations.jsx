import { useState, useCallback } from "react";

/* ── Styles ──────────────────────────────────────────────────────────────── */
const STYLES = `
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=Syne:wght@600;700;800&family=DM+Mono:wght@400;500&family=DM+Sans:wght@300;400;500;600;700&display=swap');
  *, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
  body { background:#070910; font-family:'DM Sans',sans-serif; color:#e2e8f0; overflow:hidden; }
  ::-webkit-scrollbar { width:3px; height:3px; }
  ::-webkit-scrollbar-track { background:transparent; }
  ::-webkit-scrollbar-thumb { background:#2a2e3a; border-radius:2px; }
  input,select,textarea,button { font-family:'DM Sans',sans-serif; }
  input[type=checkbox] { accent-color:#f0a500; width:14px; height:14px; cursor:pointer; }

  @keyframes fadeUp  { from{opacity:0;transform:translateY(8px)} to{opacity:1;transform:translateY(0)} }
  @keyframes fadeIn  { from{opacity:0} to{opacity:1} }
  @keyframes pulse   { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.5;transform:scale(1.3)} }
  @keyframes toastIn { from{opacity:0;transform:translateY(12px)} to{opacity:1;transform:translateY(0)} }
  @keyframes spin    { to{transform:rotate(360deg)} }
  @keyframes countUp { from{opacity:0;transform:translateY(6px)} to{opacity:1;transform:translateY(0)} }

  .fade-up  { animation:fadeUp 0.3s ease forwards; }
  .fade-in  { animation:fadeIn 0.2s ease forwards; }
  .pulse    { animation:pulse 2s ease-in-out infinite; }
  .btn      { transition:all 0.15s ease; cursor:pointer; border:none; outline:none; }
  .btn:active { transform:scale(0.97); }
  .row-hover { transition:background 0.1s ease; }
  .row-hover:hover { background:rgba(240,165,0,0.04) !important; cursor:pointer; }
  .tag      { transition:all 0.12s ease; }
  .toggle-track { transition:all 0.2s ease; cursor:pointer; }
`;

/* ── Brand ───────────────────────────────────────────────────────────────── */
const C = {
  bg:"#070910", surf:"#0e1118", elev:"#141820", border:"#1e2330",
  borderHov:"#2e3445", amber:"#f0a500", amberDim:"#f0a50020",
  amberBord:"#f0a50035", green:"#22c55e", greenDim:"#22c55e18",
  red:"#ef4444", redDim:"#ef444418", sky:"#38bdf8", skyDim:"#38bdf818",
  purple:"#a855f7", purpleDim:"#a855f718", muted:"#8892a0",
  text:"#e2e8f0", textSoft:"#b8b3ab",
};

/* ── Seed Data ───────────────────────────────────────────────────────────── */
const ALL_FEATURES = [
  { id:"f01", key:"photo_grid",           label:"Photo Grid Layout",          cat:"Display"    },
  { id:"f02", key:"classic_diner",        label:"Classic Diner Layout",        cat:"Display"    },
  { id:"f03", key:"neon_chalkboard",      label:"Neon Chalkboard Layout",      cat:"Display"    },
  { id:"f04", key:"tap_list",             label:"Tap List Boards",             cat:"Display"    },
  { id:"f05", key:"theme_builder_basic",  label:"Theme Builder (Basic)",       cat:"Display"    },
  { id:"f06", key:"theme_builder_full",   label:"Theme Builder (Full)",        cat:"Display"    },
  { id:"f07", key:"zone_builder",         label:"Zone Layout Builder",         cat:"Display"    },
  { id:"f08", key:"meal_periods",         label:"Meal Period Scheduler",       cat:"Scheduling" },
  { id:"f09", key:"happy_hour",           label:"Happy Hour Scheduling",       cat:"Scheduling" },
  { id:"f10", key:"playlist_rotation",    label:"Playlist Rotation",           cat:"Scheduling" },
  { id:"f11", key:"emergency_broadcast",  label:"Emergency Broadcast",         cat:"Scheduling" },
  { id:"f12", key:"bilingual_display",    label:"Bilingual Display",           cat:"Language"   },
  { id:"f13", key:"ai_translation",       label:"AI Translation",              cat:"Language"   },
  { id:"f14", key:"multilang_admin",      label:"Multilingual Admin UI",       cat:"Language"   },
  { id:"f15", key:"quick_update_mobile",  label:"Quick Update Mobile View",    cat:"Mobile"     },
  { id:"f16", key:"staff_mobile_app",     label:"Staff Mobile App",            cat:"Mobile"     },
  { id:"f17", key:"pos_square",           label:"Square POS Integration",      cat:"POS"        },
  { id:"f18", key:"pos_toast",            label:"Toast POS Integration",       cat:"POS"        },
  { id:"f19", key:"pos_clover",           label:"Clover POS Integration",      cat:"POS"        },
  { id:"f20", key:"ai_descriptions",      label:"AI Menu Descriptions",        cat:"AI"         },
  { id:"f21", key:"ai_custom_builder",    label:"AI Custom Display Builder",   cat:"AI"         },
  { id:"f22", key:"html_editor",          label:"HTML/CSS Sandbox Editor",     cat:"AI"         },
  { id:"f23", key:"video_wall",           label:"Video Wall Support",          cat:"Screens"    },
  { id:"f24", key:"analytics_basic",      label:"Basic Analytics",             cat:"Analytics"  },
  { id:"f25", key:"analytics_full",       label:"Full Analytics + A/B",        cat:"Analytics"  },
  { id:"f26", key:"multi_location",       label:"Multi-Location Dashboard",    cat:"Enterprise" },
  { id:"f27", key:"white_label",          label:"White-Label",                 cat:"Enterprise" },
  { id:"f28", key:"sso_entra",            label:"Microsoft Entra ID SSO",      cat:"Enterprise" },
  { id:"f29", key:"combo_builder",        label:"Combo Meal Builder",          cat:"Content"    },
  { id:"f30", key:"allergen_badges",      label:"Allergen & Dietary Badges",   cat:"Content"    },
];

const INIT_TIERS = [
  {
    id:"t1", name:"Starter", slug:"starter", price:39, maxScreens:2,
    isPublic:true, isActive:true, stripeId:"price_starter",
    venueTypes:["generic","cafe","retail"],
    features:["f01","f02","f05","f24","f30"],
    color:"#64748b",
  },
  {
    id:"t2", name:"Restaurant Starter", slug:"restaurant_starter", price:49, maxScreens:1,
    isPublic:true, isActive:true, stripeId:"price_rs",
    venueTypes:["restaurant","qsr"],
    features:["f01","f02","f05","f08","f12","f13","f14","f15","f24","f29","f30"],
    color:"#22c55e",
  },
  {
    id:"t3", name:"Pro", slug:"pro", price:89, maxScreens:6,
    isPublic:true, isActive:true, stripeId:"price_pro",
    venueTypes:["restaurant","bar","qsr","brewery","cafe","foodhall"],
    features:["f01","f02","f03","f04","f05","f06","f08","f09","f10","f11",
              "f12","f13","f14","f15","f16","f17","f18","f19","f20","f23",
              "f24","f25","f29","f30"],
    color:"#f0a500",
  },
  {
    id:"t4", name:"Business", slug:"business", price:179, maxScreens:-1,
    isPublic:true, isActive:true, stripeId:"price_biz",
    venueTypes:["restaurant","bar","qsr","brewery","cafe","foodhall","hotel","stadium"],
    features: ALL_FEATURES.map(f=>f.id),
    color:"#a855f7",
  },
  {
    id:"t5", name:"Brewery Special", slug:"brewery_special", price:69, maxScreens:4,
    isPublic:false, isActive:true, stripeId:"price_brew",
    venueTypes:["brewery","bar"],
    features:["f01","f03","f04","f05","f06","f09","f10","f11","f15","f16","f23","f24","f25","f30"],
    color:"#38bdf8",
  },
];

const INIT_VENUES = [
  { id:"v1", name:"The Copper Still",  type:"bar",        tier:"t3", mrr:89,  screens:3, status:"active",  health:"online",  lastSeen:"2 min ago",  overrides:[{fid:"f21",enabled:true,reason:"Beta tester",expires:"2026-06-01"}] },
  { id:"v2", name:"Golden Dragon",     type:"restaurant", tier:"t2", mrr:49,  screens:1, status:"active",  health:"online",  lastSeen:"5 min ago",  overrides:[] },
  { id:"v3", name:"480 Brewing Co.",   type:"brewery",    tier:"t5", mrr:69,  screens:4, status:"active",  health:"online",  lastSeen:"12 min ago", overrides:[] },
  { id:"v4", name:"Mama's Kitchen",    type:"restaurant", tier:"t1", mrr:39,  screens:2, status:"active",  health:"offline", lastSeen:"3 days ago", overrides:[] },
  { id:"v5", name:"The Patio Bar",     type:"bar",        tier:"t4", mrr:179, screens:8, status:"active",  health:"online",  lastSeen:"Just now",   overrides:[{fid:"f22",enabled:false,reason:"Misuse",expires:null}] },
  { id:"v6", name:"Sunrise Cafe",      type:"cafe",       tier:"t1", mrr:39,  screens:1, status:"active",  health:"online",  lastSeen:"1 hr ago",   overrides:[] },
  { id:"v7", name:"Casa de Tacos",     type:"qsr",        tier:"t2", mrr:49,  screens:1, status:"active",  health:"online",  lastSeen:"30 min ago", overrides:[] },
  { id:"v8", name:"Stadium Bites",     type:"stadium",    tier:"t4", mrr:179, screens:12,status:"active",  health:"online",  lastSeen:"Just now",   overrides:[] },
  { id:"v9", name:"Sakura Ramen",      type:"restaurant", tier:"t2", mrr:49,  screens:1, status:"trialing",health:"online",  lastSeen:"4 hr ago",   overrides:[{fid:"f03",enabled:true,reason:"Trial upgrade",expires:"2026-05-01"}] },
  { id:"v10",name:"The Tap Room",      type:"brewery",    tier:"t3", mrr:89,  screens:2, status:"active",  health:"online",  lastSeen:"8 min ago",  overrides:[] },
];

const RECENT_EVENTS = [
  { id:"e1", type:"upgrade",   venue:"The Patio Bar",    msg:"Pro → Business",          time:"2 hr ago",  delta:"+$90/mo" },
  { id:"e2", type:"new",       venue:"Sakura Ramen",      msg:"New trial signup",         time:"4 hr ago",  delta:"+$49/mo" },
  { id:"e3", type:"override",  venue:"The Copper Still", msg:"AI Builder beta enabled",  time:"1 day ago", delta:"" },
  { id:"e4", type:"churn",     venue:"Old Town Grill",   msg:"Cancelled Starter",        time:"2 days ago",delta:"-$39/mo" },
  { id:"e5", type:"upgrade",   venue:"480 Brewing Co.",  msg:"Starter → Brewery Special",time:"3 days ago",delta:"+$30/mo" },
];

const VENUE_TYPES = ["restaurant","bar","qsr","cafe","brewery","foodhall","hotel","stadium","generic","retail"];
const CATS = [...new Set(ALL_FEATURES.map(f=>f.cat))];

const fmt = n => `$${Number(n).toFixed(0)}`;
const totalMRR = INIT_VENUES.filter(v=>v.status==="active").reduce((a,v)=>a+v.mrr,0);

/* ── Helpers ─────────────────────────────────────────────────────────────── */
const uid = () => Math.random().toString(36).slice(2,9);

function Toggle({ value, onChange, color=C.amber, size=36 }) {
  return (
    <div className="toggle-track" onClick={()=>onChange(!value)}
      style={{ width:size, height:size*0.6, borderRadius:size*0.3,
        background:value?color:C.elev, border:`1px solid ${value?color:C.border}`,
        boxShadow:value?`0 0 7px ${color}60`:"none",
        position:"relative", flexShrink:0 }}>
      <div style={{ width:size*0.42, height:size*0.42, borderRadius:"50%",
        background:"#fff", position:"absolute",
        top:size*0.08, left:value?size*0.48:size*0.06,
        transition:"left 0.18s", boxShadow:"0 1px 3px rgba(0,0,0,0.4)" }} />
    </div>
  );
}

function Toast({ msg, onDone }) {
  useState(()=>{ const t=setTimeout(onDone,2500); return()=>clearTimeout(t); });
  return (
    <div style={{ position:"fixed",bottom:24,right:24,zIndex:9999,
      background:`linear-gradient(135deg,${C.green},#16a34a)`,color:"#fff",
      borderRadius:10,padding:"11px 20px",fontWeight:600,fontSize:13,
      boxShadow:"0 8px 28px rgba(0,0,0,0.5)",display:"flex",alignItems:"center",
      gap:9,animation:"toastIn 0.25s ease" }}>
      <span>✓</span>{msg}
    </div>
  );
}

const labelSt = { display:"block",fontSize:10,fontWeight:600,color:C.muted,
  textTransform:"uppercase",letterSpacing:"0.07em",marginBottom:5 };
const inputSt = { background:C.elev,border:`1px solid ${C.border}`,borderRadius:7,
  padding:"8px 11px",color:C.text,fontSize:13,outline:"none",width:"100%" };

/* ── Stat Card ───────────────────────────────────────────────────────────── */
function StatCard({ label, value, sub, color=C.amber, icon }) {
  return (
    <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,
      padding:"16px 18px",flex:1,minWidth:0 }}>
      <div style={{ display:"flex",alignItems:"center",gap:8,marginBottom:10 }}>
        <span style={{ fontSize:16 }}>{icon}</span>
        <span style={{ fontSize:11,fontWeight:600,color:C.muted,
          textTransform:"uppercase",letterSpacing:"0.07em" }}>{label}</span>
      </div>
      <div style={{ fontFamily:"'DM Mono'",fontSize:28,fontWeight:500,
        color,lineHeight:1,marginBottom:4 }}>{value}</div>
      <div style={{ fontSize:12,color:C.muted }}>{sub}</div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════════
   TAB: DASHBOARD
══════════════════════════════════════════════════════════════════════════ */
function DashboardTab({ venues, tiers }) {
  const active = venues.filter(v=>v.status==="active");
  const online = venues.filter(v=>v.health==="online");
  const totalScreens = venues.reduce((a,v)=>a+v.screens,0);
  const mrr = active.reduce((a,v)=>a+v.mrr,0);

  const byTier = tiers.map(t=>({
    ...t, count: venues.filter(v=>v.tier===t.id).length,
    rev: venues.filter(v=>v.tier===t.id).reduce((a,v)=>a+v.mrr,0),
  }));

  return (
    <div className="fade-up" style={{ display:"flex",flexDirection:"column",gap:16 }}>
      {/* Stats row */}
      <div style={{ display:"flex",gap:12 }}>
        <StatCard label="Monthly Recurring Revenue" value={`$${mrr.toLocaleString()}`}
          sub={`$${(mrr*12).toLocaleString()} ARR`} color={C.amber} icon="💰" />
        <StatCard label="Active Venues" value={active.length}
          sub={`${venues.filter(v=>v.status==="trialing").length} trialing`} color={C.green} icon="🏪" />
        <StatCard label="Screens Live" value={online.reduce((a,v)=>a+v.screens,0)}
          sub={`${totalScreens} total registered`} color={C.sky} icon="📺" />
        <StatCard label="Avg Revenue/Venue" value={`$${Math.round(mrr/Math.max(active.length,1))}`}
          sub="per active venue/mo" color={C.purple} icon="📈" />
      </div>

      <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:16 }}>
        {/* Tier breakdown */}
        <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,padding:"16px 18px" }}>
          <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>Revenue by Tier</div>
          {byTier.filter(t=>t.isActive).map(t=>(
            <div key={t.id} style={{ marginBottom:10 }}>
              <div style={{ display:"flex",justifyContent:"space-between",marginBottom:5 }}>
                <div style={{ display:"flex",alignItems:"center",gap:8 }}>
                  <div style={{ width:8,height:8,borderRadius:"50%",background:t.color,
                    boxShadow:`0 0 5px ${t.color}` }} />
                  <span style={{ fontSize:13,fontWeight:600 }}>{t.name}</span>
                  <span style={{ fontSize:11,color:C.muted }}>{t.count} venues</span>
                </div>
                <span style={{ fontFamily:"'DM Mono'",fontSize:13,color:t.color }}>
                  ${t.rev}/mo
                </span>
              </div>
              <div style={{ height:4,background:C.elev,borderRadius:2,overflow:"hidden" }}>
                <div style={{ height:"100%",borderRadius:2,
                  width:`${mrr?Math.round((t.rev/mrr)*100):0}%`,
                  background:t.color,transition:"width 0.5s" }} />
              </div>
            </div>
          ))}
        </div>

        {/* Recent events */}
        <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,padding:"16px 18px" }}>
          <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>Recent Events</div>
          {RECENT_EVENTS.map(ev=>{
            const colors = { upgrade:C.green, new:C.sky, override:C.amber, churn:C.red };
            const icons  = { upgrade:"↑", new:"★", override:"⚡", churn:"↓" };
            const col = colors[ev.type]||C.muted;
            return (
              <div key={ev.id} style={{ display:"flex",alignItems:"center",gap:10,
                padding:"8px 0",borderBottom:`1px solid ${C.border}` }}>
                <div style={{ width:22,height:22,borderRadius:6,background:`${col}20`,
                  border:`1px solid ${col}40`,display:"flex",alignItems:"center",
                  justifyContent:"center",color:col,fontSize:11,fontWeight:700,flexShrink:0 }}>
                  {icons[ev.type]}
                </div>
                <div style={{ flex:1,minWidth:0 }}>
                  <span style={{ fontWeight:600,fontSize:12 }}>{ev.venue}</span>
                  <span style={{ color:C.muted,fontSize:12 }}> · {ev.msg}</span>
                </div>
                <div style={{ textAlign:"right",flexShrink:0 }}>
                  {ev.delta && <div style={{ fontFamily:"'DM Mono'",fontSize:11,color:col }}>{ev.delta}</div>}
                  <div style={{ fontSize:10,color:C.muted }}>{ev.time}</div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Screen health map */}
      <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,padding:"16px 18px" }}>
        <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>
          Screen Health — All Venues
        </div>
        <div style={{ display:"flex",flexWrap:"wrap",gap:8 }}>
          {venues.map(v=>
            Array.from({length:v.screens}).map((_,i)=>(
              <div key={`${v.id}-${i}`}
                title={`${v.name} · Screen ${i+1} · ${v.health}`}
                style={{ width:10,height:10,borderRadius:2,
                  background:v.health==="online"?C.green:C.red,
                  boxShadow:v.health==="online"?`0 0 4px ${C.green}80`:"none",
                  opacity:v.health==="online"?1:0.5 }} />
            ))
          )}
        </div>
        <div style={{ display:"flex",gap:16,marginTop:10 }}>
          <div style={{ fontSize:11,color:C.muted,display:"flex",alignItems:"center",gap:5 }}>
            <div style={{ width:8,height:8,borderRadius:2,background:C.green }} /> Online
          </div>
          <div style={{ fontSize:11,color:C.muted,display:"flex",alignItems:"center",gap:5 }}>
            <div style={{ width:8,height:8,borderRadius:2,background:C.red,opacity:0.5 }} /> Offline
          </div>
          <div style={{ marginLeft:"auto",fontFamily:"'DM Mono'",fontSize:11,color:C.muted }}>
            {online.reduce((a,v)=>a+v.screens,0)}/{totalScreens} screens online
          </div>
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════════
   TAB: TIERS
══════════════════════════════════════════════════════════════════════════ */
function TiersTab({ tiers, setTiers, venues, toast }) {
  const [selected, setSelected] = useState("t3");
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(null);

  const tier = tiers.find(t=>t.id===selected);
  const venueCount = venues.filter(v=>v.tier===selected).length;

  const startEdit = () => { setDraft({...tier,features:[...tier.features]}); setEditing(true); };
  const cancelEdit = () => { setEditing(false); setDraft(null); };

  const saveEdit = () => {
    setTiers(ts=>ts.map(t=>t.id===selected?{...draft}:t));
    setEditing(false); setDraft(null);
    toast("Tier saved · Feature changes live immediately");
  };

  const toggleFeature = (fid) => {
    setDraft(d=>{
      const has = d.features.includes(fid);
      return { ...d, features: has ? d.features.filter(f=>f!==fid) : [...d.features,fid] };
    });
  };

  const cloneTier = () => {
    const newT = { ...tier, id:uid(), name:`${tier.name} (Copy)`,
      slug:`${tier.slug}_copy`, isPublic:false, features:[...tier.features] };
    setTiers(ts=>[...ts, newT]);
    setSelected(newT.id);
    toast("Tier cloned");
  };

  const addTier = () => {
    const newT = { id:uid(), name:"New Tier", slug:"new_tier", price:0,
      maxScreens:1, isPublic:false, isActive:true, stripeId:"",
      venueTypes:[], features:[], color:"#64748b" };
    setTiers(ts=>[...ts,newT]);
    setSelected(newT.id);
    setDraft({...newT,features:[]});
    setEditing(true);
  };

  const cur = editing ? draft : tier;

  return (
    <div className="fade-up" style={{ display:"flex",gap:16,height:"100%" }}>
      {/* Tier list */}
      <div style={{ width:220,flexShrink:0,display:"flex",flexDirection:"column",gap:8 }}>
        {tiers.map(t=>(
          <div key={t.id} onClick={()=>{setSelected(t.id);setEditing(false);setDraft(null);}}
            className="row-hover" style={{ padding:"11px 14px",borderRadius:10,
              background:selected===t.id?`${t.color}15`:C.surf,
              border:`1px solid ${selected===t.id?t.color+"50":C.border}`,
              cursor:"pointer",transition:"all 0.15s" }}>
            <div style={{ display:"flex",alignItems:"center",gap:8,marginBottom:4 }}>
              <div style={{ width:8,height:8,borderRadius:"50%",background:t.color,
                boxShadow:`0 0 5px ${t.color}60`,flexShrink:0 }} />
              <span style={{ fontWeight:700,fontSize:13,
                color:selected===t.id?t.color:C.text }}>{t.name}</span>
            </div>
            <div style={{ display:"flex",justifyContent:"space-between",alignItems:"center" }}>
              <span style={{ fontFamily:"'DM Mono'",fontSize:12,color:C.amber }}>${t.price}/mo</span>
              <div style={{ display:"flex",gap:5 }}>
                {!t.isPublic && <span style={{ fontSize:9,padding:"1px 5px",borderRadius:3,
                  background:C.elev,color:C.muted,fontWeight:600 }}>PRIVATE</span>}
                {!t.isActive && <span style={{ fontSize:9,padding:"1px 5px",borderRadius:3,
                  background:C.redDim,color:C.red,fontWeight:600 }}>OFF</span>}
              </div>
            </div>
            <div style={{ fontSize:10,color:C.muted,marginTop:2 }}>
              {venues.filter(v=>v.tier===t.id).length} venues · {t.features.length} features
            </div>
          </div>
        ))}
        <button className="btn" onClick={addTier}
          style={{ padding:"10px",borderRadius:9,background:"transparent",
            border:`1px dashed ${C.border}`,color:C.muted,fontSize:12,cursor:"pointer" }}>
          + New Tier
        </button>
      </div>

      {/* Tier detail */}
      {cur && (
        <div style={{ flex:1,overflow:"auto",display:"flex",flexDirection:"column",gap:14 }}>
          {/* Header */}
          <div style={{ background:C.surf,border:`1px solid ${C.border}`,
            borderRadius:12,padding:"16px 20px" }}>
            <div style={{ display:"flex",alignItems:"flex-start",gap:16,marginBottom:14 }}>
              <div style={{ flex:1 }}>
                {editing ? (
                  <input value={draft.name}
                    onChange={e=>setDraft(d=>({...d,name:e.target.value}))}
                    style={{ ...inputSt,fontFamily:"'Syne'",fontWeight:700,fontSize:20,
                      marginBottom:8 }} />
                ) : (
                  <div style={{ fontFamily:"'Syne'",fontWeight:800,fontSize:22,
                    color:cur.color,marginBottom:4 }}>{cur.name}</div>
                )}
                <div style={{ display:"flex",gap:10,flexWrap:"wrap" }}>
                  {[{l:"Slug",v:cur.slug},{l:"Stripe ID",v:cur.stripeId||"—"},
                    {l:"Venues",v:venueCount},{l:"Features",v:cur.features.length}].map(m=>(
                    <div key={m.l} style={{ fontSize:11,color:C.muted }}>
                      <span style={{ color:C.muted }}>{m.l}: </span>
                      <span style={{ color:C.textSoft,fontFamily:"'DM Mono'" }}>{m.v}</span>
                    </div>
                  ))}
                </div>
              </div>
              <div style={{ display:"flex",gap:8,flexShrink:0 }}>
                {!editing ? (
                  <>
                    <button className="btn" onClick={startEdit}
                      style={{ padding:"7px 14px",borderRadius:7,background:C.elev,
                        color:C.amber,border:`1px solid ${C.amberBord}`,
                        fontSize:12,fontWeight:600,cursor:"pointer" }}>Edit</button>
                    <button className="btn" onClick={cloneTier}
                      style={{ padding:"7px 14px",borderRadius:7,background:C.elev,
                        color:C.muted,border:`1px solid ${C.border}`,
                        fontSize:12,fontWeight:600,cursor:"pointer" }}>Clone</button>
                  </>
                ) : (
                  <>
                    <button className="btn" onClick={cancelEdit}
                      style={{ padding:"7px 14px",borderRadius:7,background:C.elev,
                        color:C.muted,border:`1px solid ${C.border}`,
                        fontSize:12,fontWeight:600,cursor:"pointer" }}>Cancel</button>
                    <button className="btn" onClick={saveEdit}
                      style={{ padding:"7px 14px",borderRadius:7,background:C.amber,
                        color:"#000",fontSize:12,fontWeight:700,cursor:"pointer" }}>Save Tier</button>
                  </>
                )}
              </div>
            </div>

            {/* Config row */}
            <div style={{ display:"grid",gridTemplateColumns:"repeat(4,1fr)",gap:12 }}>
              {[
                { label:"Monthly Price ($)", field:"price", type:"number" },
                { label:"Max Screens (-1=∞)", field:"maxScreens", type:"number" },
                { label:"Tier Color", field:"color", type:"color" },
                { label:"Stripe Price ID", field:"stripeId", type:"text" },
              ].map(f=>(
                <div key={f.field}>
                  <span style={labelSt}>{f.label}</span>
                  {editing ? (
                    f.type==="color" ? (
                      <div style={{ display:"flex",gap:10,alignItems:"center" }}>
                        <div style={{ width:32,height:32,borderRadius:8,overflow:"hidden",
                          border:`1px solid ${C.border}`,background:draft.color }}>
                          <input type="color" value={draft.color}
                            onChange={e=>setDraft(d=>({...d,color:e.target.value}))}
                            style={{ width:"100%",height:"100%",padding:0,border:"none",cursor:"pointer" }} />
                        </div>
                        <span style={{ fontFamily:"'DM Mono'",fontSize:12,color:C.muted }}>{draft.color}</span>
                      </div>
                    ) : (
                      <input type={f.type} value={draft[f.field]}
                        onChange={e=>setDraft(d=>({...d,[f.field]:f.type==="number"?Number(e.target.value):e.target.value}))}
                        style={inputSt} />
                    )
                  ) : (
                    <div style={{ fontFamily:"'DM Mono'",fontSize:13,color:C.text,
                      padding:"7px 0" }}>
                      {f.field==="color"
                        ? <span style={{ color:cur.color }}>{cur.color}</span>
                        : cur[f.field]}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Toggles */}
            <div style={{ display:"flex",gap:20,marginTop:12 }}>
              {[{label:"Public (visible on pricing page)",field:"isPublic"},
                {label:"Active (available to assign)",field:"isActive"}].map(f=>(
                <div key={f.field} style={{ display:"flex",alignItems:"center",gap:8 }}>
                  <Toggle value={editing?draft[f.field]:cur[f.field]}
                    onChange={v=>editing&&setDraft(d=>({...d,[f.field]:v}))}
                    color={C.green} />
                  <span style={{ fontSize:12,color:C.muted }}>{f.label}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Feature matrix */}
          <div style={{ background:C.surf,border:`1px solid ${C.border}`,
            borderRadius:12,overflow:"hidden" }}>
            <div style={{ padding:"14px 20px",borderBottom:`1px solid ${C.border}`,
              display:"flex",alignItems:"center",gap:12 }}>
              <span style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14 }}>
                Feature Flags
              </span>
              <span style={{ fontSize:11,color:C.muted }}>
                {cur.features.length} / {ALL_FEATURES.length} enabled
              </span>
              {editing && (
                <div style={{ marginLeft:"auto",display:"flex",gap:8 }}>
                  <button className="btn" onClick={()=>setDraft(d=>({...d,features:ALL_FEATURES.map(f=>f.id)}))}
                    style={{ fontSize:11,padding:"4px 10px",borderRadius:5,background:C.elev,
                      color:C.muted,cursor:"pointer" }}>Enable All</button>
                  <button className="btn" onClick={()=>setDraft(d=>({...d,features:[]}))}
                    style={{ fontSize:11,padding:"4px 10px",borderRadius:5,background:C.elev,
                      color:C.muted,cursor:"pointer" }}>Clear All</button>
                </div>
              )}
            </div>
            {CATS.map(cat=>(
              <div key={cat}>
                <div style={{ padding:"8px 20px",background:C.elev,
                  borderBottom:`1px solid ${C.border}`,
                  fontSize:10,fontWeight:700,color:C.muted,
                  textTransform:"uppercase",letterSpacing:"0.08em" }}>
                  {cat}
                </div>
                {ALL_FEATURES.filter(f=>f.cat===cat).map(f=>{
                  const enabled = cur.features.includes(f.id);
                  return (
                    <div key={f.id} onClick={()=>editing&&toggleFeature(f.id)}
                      className="row-hover"
                      style={{ display:"flex",alignItems:"center",gap:14,
                        padding:"10px 20px",borderBottom:`1px solid ${C.border}`,
                        cursor:editing?"pointer":"default",
                        background:enabled?`${C.green}08`:"transparent" }}>
                      <div style={{ width:14,height:14,borderRadius:3,flexShrink:0,
                        background:enabled?C.green:C.elev,
                        border:`1px solid ${enabled?C.green:C.border}`,
                        display:"flex",alignItems:"center",justifyContent:"center" }}>
                        {enabled && <span style={{ color:"#fff",fontSize:9,fontWeight:700 }}>✓</span>}
                      </div>
                      <span style={{ fontSize:13,fontWeight:enabled?600:400,
                        color:enabled?C.text:C.muted,flex:1 }}>{f.label}</span>
                      <span style={{ fontFamily:"'DM Mono'",fontSize:10,color:C.muted }}>{f.key}</span>
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════════
   TAB: VENUES
══════════════════════════════════════════════════════════════════════════ */
function VenuesTab({ venues, setVenues, tiers, toast }) {
  const [search, setSearch] = useState("");
  const [filterTier, setFilterTier] = useState("all");
  const [filterStatus, setFilterStatus] = useState("all");
  const [detail, setDetail] = useState(null);
  const [newOverride, setNewOverride] = useState({ fid:"", enabled:true, reason:"", expires:"" });

  const filtered = venues.filter(v=>{
    const ms = search ? v.name.toLowerCase().includes(search.toLowerCase()) : true;
    const mt = filterTier==="all" ? true : v.tier===filterTier;
    const ms2 = filterStatus==="all" ? true : v.status===filterStatus;
    return ms && mt && ms2;
  });

  const detailVenue = detail ? venues.find(v=>v.id===detail) : null;
  const detailTier = detailVenue ? tiers.find(t=>t.id===detailVenue.tier) : null;

  const addOverride = () => {
    if (!newOverride.fid) return;
    setVenues(vs=>vs.map(v=>v.id===detail?{
      ...v, overrides:[...v.overrides,{...newOverride,id:uid()}]
    }:v));
    setNewOverride({fid:"",enabled:true,reason:"",expires:""});
    toast("Override applied · Takes effect immediately");
  };

  const removeOverride = (oid) => {
    setVenues(vs=>vs.map(v=>v.id===detail?{
      ...v, overrides:v.overrides.filter(o=>o.fid!==oid)
    }:v));
    toast("Override removed");
  };

  const changeTier = (vid, tid) => {
    setVenues(vs=>vs.map(v=>v.id===vid?{...v,tier:tid}:v));
    toast("Tier updated · Stripe will be notified");
  };

  const statusColor = s => s==="active"?C.green:s==="trialing"?C.amber:C.red;

  if (detailVenue) return (
    <div className="fade-up" style={{ display:"flex",flexDirection:"column",gap:14 }}>
      {/* Back + header */}
      <div style={{ display:"flex",alignItems:"center",gap:12 }}>
        <button className="btn" onClick={()=>setDetail(null)}
          style={{ padding:"6px 12px",borderRadius:7,background:C.surf,
            color:C.amber,border:`1px solid ${C.amberBord}`,fontSize:12,fontWeight:600,cursor:"pointer" }}>
          ← All Venues
        </button>
        <div style={{ fontFamily:"'Syne'",fontWeight:800,fontSize:20 }}>{detailVenue.name}</div>
        <div style={{ fontSize:11,padding:"3px 9px",borderRadius:5,fontWeight:700,
          background:`${statusColor(detailVenue.status)}18`,
          color:statusColor(detailVenue.status),border:`1px solid ${statusColor(detailVenue.status)}30` }}>
          {detailVenue.status.toUpperCase()}
        </div>
      </div>

      <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:14 }}>
        {/* Info */}
        <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,padding:"16px 20px" }}>
          <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>Venue Info</div>
          <div style={{ display:"grid",gridTemplateColumns:"1fr 1fr",gap:12 }}>
            {[{l:"Type",v:detailVenue.type},{l:"Screens",v:detailVenue.screens},
              {l:"MRR",v:`$${detailVenue.mrr}/mo`},{l:"Last Active",v:detailVenue.lastSeen},
              {l:"Health",v:detailVenue.health},{l:"Overrides",v:detailVenue.overrides.length}].map(m=>(
              <div key={m.l}>
                <div style={{ fontSize:10,fontWeight:600,color:C.muted,
                  textTransform:"uppercase",letterSpacing:"0.06em",marginBottom:3 }}>{m.l}</div>
                <div style={{ fontSize:13,fontWeight:600,color:C.text }}>{m.v}</div>
              </div>
            ))}
          </div>
          <div style={{ marginTop:14 }}>
            <span style={labelSt}>Subscription Tier</span>
            <select value={detailVenue.tier}
              onChange={e=>changeTier(detailVenue.id,e.target.value)}
              style={{ ...inputSt,cursor:"pointer" }}>
              {tiers.filter(t=>t.isActive).map(t=>(
                <option key={t.id} value={t.id}>{t.name} — ${t.price}/mo</option>
              ))}
            </select>
          </div>
        </div>

        {/* Feature resolution */}
        <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,
          padding:"16px 20px",overflow:"auto",maxHeight:320 }}>
          <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>
            Effective Features
          </div>
          {ALL_FEATURES.map(f=>{
            const overr = detailVenue.overrides.find(o=>o.fid===f.id);
            const tierHas = detailTier?.features.includes(f.id);
            const effective = overr ? overr.enabled : !!tierHas;
            const source = overr ? "override" : tierHas ? "tier" : "none";
            return (
              <div key={f.id} style={{ display:"flex",alignItems:"center",gap:10,
                padding:"5px 0",borderBottom:`1px solid ${C.border}` }}>
                <div style={{ width:12,height:12,borderRadius:3,flexShrink:0,
                  background:effective?C.green:C.elev,
                  border:`1px solid ${effective?C.green:C.border}` }}>
                  {effective && <div style={{ width:"100%",height:"100%",display:"flex",
                    alignItems:"center",justifyContent:"center",color:"#fff",fontSize:8 }}>✓</div>}
                </div>
                <span style={{ fontSize:11,flex:1,color:effective?C.text:C.muted }}>{f.label}</span>
                {source==="override" && (
                  <span style={{ fontSize:9,padding:"1px 5px",borderRadius:3,
                    background:C.amberDim,color:C.amber,fontWeight:700 }}>OVERRIDE</span>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Overrides */}
      <div style={{ background:C.surf,border:`1px solid ${C.border}`,borderRadius:12,padding:"16px 20px" }}>
        <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:14,marginBottom:14 }}>
          Feature Overrides ({detailVenue.overrides.length})
        </div>

        {detailVenue.overrides.length > 0 ? (
          <div style={{ marginBottom:14 }}>
            {detailVenue.overrides.map(o=>{
              const feat = ALL_FEATURES.find(f=>f.id===o.fid);
              return (
                <div key={o.fid} style={{ display:"flex",alignItems:"center",gap:12,
                  padding:"10px 0",borderBottom:`1px solid ${C.border}` }}>
                  <div style={{ width:16,height:16,borderRadius:4,flexShrink:0,
                    background:o.enabled?`${C.green}20`:`${C.red}20`,
                    border:`1px solid ${o.enabled?C.green:C.red}40`,
                    display:"flex",alignItems:"center",justifyContent:"center",
                    color:o.enabled?C.green:C.red,fontSize:10,fontWeight:700 }}>
                    {o.enabled?"✓":"✗"}
                  </div>
                  <div style={{ flex:1 }}>
                    <div style={{ fontSize:13,fontWeight:600 }}>{feat?.label||o.fid}</div>
                    <div style={{ fontSize:11,color:C.muted }}>
                      {o.reason}{o.expires?` · Expires ${o.expires}`:" · Permanent"}
                    </div>
                  </div>
                  <span style={{ fontSize:11,padding:"2px 8px",borderRadius:4,
                    background:o.enabled?C.greenDim:C.redDim,
                    color:o.enabled?C.green:C.red,fontWeight:700 }}>
                    {o.enabled?"UNLOCKED":"BLOCKED"}
                  </span>
                  <button className="btn" onClick={()=>removeOverride(o.fid)}
                    style={{ fontSize:11,padding:"3px 8px",borderRadius:5,background:C.elev,
                      color:C.muted,cursor:"pointer" }}>Remove</button>
                </div>
              );
            })}
          </div>
        ) : (
          <div style={{ fontSize:12,color:C.muted,marginBottom:14 }}>
            No overrides — venue uses tier defaults for all features.
          </div>
        )}

        {/* Add override */}
        <div style={{ padding:"14px",borderRadius:10,background:C.elev,
          border:`1px solid ${C.border}` }}>
          <div style={{ fontSize:12,fontWeight:700,color:C.amber,marginBottom:10 }}>
            + Add Override
          </div>
          <div style={{ display:"grid",gridTemplateColumns:"2fr 1fr 2fr 1fr",gap:10,alignItems:"end" }}>
            <div>
              <span style={labelSt}>Feature</span>
              <select value={newOverride.fid}
                onChange={e=>setNewOverride(o=>({...o,fid:e.target.value}))}
                style={{ ...inputSt,cursor:"pointer" }}>
                <option value="">Select feature...</option>
                {ALL_FEATURES.map(f=>(
                  <option key={f.id} value={f.id}>{f.label}</option>
                ))}
              </select>
            </div>
            <div>
              <span style={labelSt}>Action</span>
              <select value={newOverride.enabled}
                onChange={e=>setNewOverride(o=>({...o,enabled:e.target.value==="true"}))}
                style={{ ...inputSt,cursor:"pointer" }}>
                <option value="true">Unlock</option>
                <option value="false">Block</option>
              </select>
            </div>
            <div>
              <span style={labelSt}>Reason</span>
              <input value={newOverride.reason}
                onChange={e=>setNewOverride(o=>({...o,reason:e.target.value}))}
                placeholder="Beta tester, Bug bounty..."
                style={inputSt} />
            </div>
            <button className="btn" onClick={addOverride}
              style={{ padding:"8px 14px",borderRadius:7,background:C.amber,
                color:"#000",fontWeight:700,fontSize:12,cursor:"pointer" }}>
              Apply
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <div className="fade-up" style={{ display:"flex",flexDirection:"column",gap:12 }}>
      {/* Filters */}
      <div style={{ display:"flex",gap:10,alignItems:"center" }}>
        <input value={search} onChange={e=>setSearch(e.target.value)}
          placeholder="Search venues..."
          style={{ ...inputSt,width:220 }} />
        <select value={filterTier} onChange={e=>setFilterTier(e.target.value)}
          style={{ ...inputSt,width:180,cursor:"pointer" }}>
          <option value="all">All Tiers</option>
          {tiers.map(t=><option key={t.id} value={t.id}>{t.name}</option>)}
        </select>
        <select value={filterStatus} onChange={e=>setFilterStatus(e.target.value)}
          style={{ ...inputSt,width:140,cursor:"pointer" }}>
          <option value="all">All Status</option>
          <option value="active">Active</option>
          <option value="trialing">Trialing</option>
          <option value="cancelled">Cancelled</option>
        </select>
        <div style={{ marginLeft:"auto",fontSize:12,color:C.muted }}>
          {filtered.length} venues
        </div>
      </div>

      {/* Table */}
      <div style={{ background:C.surf,border:`1px solid ${C.border}`,
        borderRadius:12,overflow:"hidden" }}>
        {/* Head */}
        <div style={{ display:"grid",
          gridTemplateColumns:"2fr 1fr 1fr 80px 80px 90px 100px 60px",
          padding:"10px 18px",background:C.elev,borderBottom:`1px solid ${C.border}` }}>
          {["Venue","Type","Tier","MRR","Screens","Status","Health",""].map(h=>(
            <div key={h} style={{ fontSize:10,fontWeight:700,color:C.muted,
              textTransform:"uppercase",letterSpacing:"0.07em" }}>{h}</div>
          ))}
        </div>
        {filtered.map((v,i)=>{
          const tier = tiers.find(t=>t.id===v.tier);
          const sc = statusColor(v.status);
          return (
            <div key={v.id} className="row-hover"
              style={{ display:"grid",
                gridTemplateColumns:"2fr 1fr 1fr 80px 80px 90px 100px 60px",
                padding:"11px 18px",
                borderBottom:i<filtered.length-1?`1px solid ${C.border}`:"none",
                alignItems:"center" }}>
              <div>
                <div style={{ fontWeight:600,fontSize:13 }}>{v.name}</div>
                {v.overrides.length>0 && (
                  <span style={{ fontSize:9,padding:"1px 5px",borderRadius:3,
                    background:C.amberDim,color:C.amber,fontWeight:700 }}>
                    {v.overrides.length} OVERRIDE{v.overrides.length>1?"S":""}
                  </span>
                )}
              </div>
              <div style={{ fontSize:12,color:C.muted,textTransform:"capitalize" }}>{v.type}</div>
              <div style={{ display:"flex",alignItems:"center",gap:6 }}>
                <div style={{ width:6,height:6,borderRadius:"50%",
                  background:tier?.color||C.muted,flexShrink:0 }} />
                <span style={{ fontSize:12,color:tier?.color||C.muted }}>{tier?.name||"—"}</span>
              </div>
              <div style={{ fontFamily:"'DM Mono'",fontSize:13,color:C.amber }}>${v.mrr}</div>
              <div style={{ fontFamily:"'DM Mono'",fontSize:13 }}>{v.screens}</div>
              <div style={{ fontSize:11,padding:"2px 8px",borderRadius:4,width:"fit-content",
                background:`${sc}15`,color:sc,fontWeight:700 }}>{v.status}</div>
              <div style={{ display:"flex",alignItems:"center",gap:6 }}>
                <div style={{ width:6,height:6,borderRadius:"50%",
                  background:v.health==="online"?C.green:C.red,
                  boxShadow:v.health==="online"?`0 0 5px ${C.green}`:""}}
                  className={v.health==="online"?"pulse":""} />
                <span style={{ fontSize:11,color:v.health==="online"?C.green:C.red }}>
                  {v.health}
                </span>
              </div>
              <button className="btn" onClick={()=>setDetail(v.id)}
                style={{ fontSize:11,padding:"4px 10px",borderRadius:5,background:C.elev,
                  color:C.muted,cursor:"pointer",border:`1px solid ${C.border}` }}>
                Manage
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════════
   TAB: FEATURES
══════════════════════════════════════════════════════════════════════════ */
function FeaturesTab({ tiers, setTiers, toast }) {
  const [changed, setChanged] = useState({});

  const toggleCell = (tierId, fid) => {
    const key = `${tierId}:${fid}`;
    setTiers(ts=>ts.map(t=>{
      if (t.id!==tierId) return t;
      const has = t.features.includes(fid);
      return { ...t, features: has?t.features.filter(f=>f!==fid):[...t.features,fid] };
    }));
    setChanged(c=>({...c,[key]:true}));
  };

  const save = () => {
    setChanged({});
    toast(`Feature matrix saved · ${Object.keys(changed).length} changes applied to all venues`);
  };

  const activeTiers = tiers.filter(t=>t.isActive);

  return (
    <div className="fade-up" style={{ display:"flex",flexDirection:"column",gap:12 }}>
      <div style={{ display:"flex",alignItems:"center",gap:12 }}>
        <div style={{ fontFamily:"'Syne'",fontWeight:800,fontSize:18 }}>Feature Matrix</div>
        <div style={{ fontSize:12,color:C.muted }}>
          Click any cell to toggle · Changes apply to all venues on that tier immediately
        </div>
        {Object.keys(changed).length>0 && (
          <button className="btn" onClick={save}
            style={{ marginLeft:"auto",padding:"8px 18px",borderRadius:8,background:C.amber,
              color:"#000",fontWeight:700,fontSize:13,cursor:"pointer" }}>
            Save {Object.keys(changed).length} Changes
          </button>
        )}
      </div>

      <div style={{ background:C.surf,border:`1px solid ${C.border}`,
        borderRadius:12,overflow:"auto" }}>
        {/* Header row */}
        <div style={{ display:"grid",
          gridTemplateColumns:`200px repeat(${activeTiers.length},1fr)`,
          position:"sticky",top:0,zIndex:5,
          background:C.elev,borderBottom:`1px solid ${C.border}` }}>
          <div style={{ padding:"12px 16px",fontSize:10,fontWeight:700,color:C.muted,
            textTransform:"uppercase",letterSpacing:"0.07em" }}>Feature</div>
          {activeTiers.map(t=>(
            <div key={t.id} style={{ padding:"10px 8px",textAlign:"center" }}>
              <div style={{ fontFamily:"'Syne'",fontWeight:700,fontSize:12,color:t.color }}>
                {t.name}
              </div>
              <div style={{ fontFamily:"'DM Mono'",fontSize:10,color:C.muted }}>${t.price}/mo</div>
            </div>
          ))}
        </div>

        {/* Feature rows */}
        {CATS.map(cat=>(
          <div key={cat}>
            <div style={{ gridColumn:`1 / -1`,padding:"7px 16px",
              background:C.elev,borderTop:`1px solid ${C.border}`,
              borderBottom:`1px solid ${C.border}`,
              fontSize:10,fontWeight:700,color:C.amber,
              textTransform:"uppercase",letterSpacing:"0.08em",
              display:"grid",
              gridTemplateColumns:`200px repeat(${activeTiers.length},1fr)` }}>
              <div>{cat}</div>
            </div>
            {ALL_FEATURES.filter(f=>f.cat===cat).map(f=>(
              <div key={f.id}
                style={{ display:"grid",
                  gridTemplateColumns:`200px repeat(${activeTiers.length},1fr)`,
                  borderBottom:`1px solid ${C.border}` }}>
                <div style={{ padding:"9px 16px",fontSize:12,color:C.textSoft,
                  display:"flex",alignItems:"center" }}>
                  {f.label}
                </div>
                {activeTiers.map(t=>{
                  const enabled = t.features.includes(f.id);
                  const key = `${t.id}:${f.id}`;
                  const isDirty = changed[key];
                  return (
                    <div key={t.id} onClick={()=>toggleCell(t.id,f.id)}
                      className="row-hover"
                      style={{ display:"flex",alignItems:"center",justifyContent:"center",
                        padding:"8px",cursor:"pointer",
                        background:isDirty?`${C.amber}08`:"transparent",
                        borderLeft:`1px solid ${C.border}` }}>
                      <div style={{ width:20,height:20,borderRadius:4,
                        background:enabled?`${t.color}25`:C.elev,
                        border:`1px solid ${enabled?t.color:C.border}`,
                        display:"flex",alignItems:"center",justifyContent:"center",
                        transition:"all 0.15s" }}>
                        {enabled && <span style={{ color:t.color,fontSize:11,fontWeight:700 }}>✓</span>}
                      </div>
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════════════
   MAIN APP
══════════════════════════════════════════════════════════════════════════ */
const NAV = [
  { id:"dashboard", icon:"◎",  label:"Dashboard"     },
  { id:"tiers",     icon:"◈",  label:"Tiers"         },
  { id:"features",  icon:"⊞",  label:"Feature Matrix"},
  { id:"venues",    icon:"⬡",  label:"Venues"        },
];

export default function VennuPlatformOperations() {
  const [tab, setTab]       = useState("dashboard");
  const [tiers, setTiers]   = useState(INIT_TIERS);
  const [venues, setVenues] = useState(INIT_VENUES);
  const [toast, setToast]   = useState(null);

  const showToast = useCallback(msg => setToast(msg), []);
  const mrr = venues.filter(v=>v.status==="active").reduce((a,v)=>a+v.mrr,0);
  const online = venues.filter(v=>v.health==="online").length;

  return (
    <>
      <style>{STYLES}</style>
      <div style={{ display:"flex",height:"100vh",overflow:"hidden",background:C.bg }}>

        {/* Sidebar */}
        <div style={{ width:210,background:C.surf,borderRight:`1px solid ${C.border}`,
          display:"flex",flexDirection:"column",flexShrink:0 }}>
          {/* Logo */}
          <div style={{ padding:"18px 20px 14px",borderBottom:`1px solid ${C.border}` }}>
            <div style={{ display:"flex",alignItems:"baseline",gap:8 }}>
              <span style={{ fontFamily:"'Playfair Display'",fontWeight:700,fontSize:22,
                color:C.amber }}>vennu</span>
              <span style={{ fontSize:10,fontWeight:700,color:C.muted,
                textTransform:"uppercase",letterSpacing:"0.08em" }}>superadmin</span>
            </div>
            <div style={{ fontSize:10,color:C.muted,marginTop:4,fontFamily:"'DM Mono'" }}>
              v2 · internal only
            </div>
          </div>

          {/* Nav */}
          <div style={{ padding:"10px 10px",flex:1 }}>
            {NAV.map(n=>(
              <button key={n.id} className="btn" onClick={()=>setTab(n.id)}
                style={{ display:"flex",alignItems:"center",gap:10,width:"100%",
                  padding:"10px 12px",borderRadius:8,cursor:"pointer",
                  background:tab===n.id?C.amberDim:"transparent",
                  borderLeft:tab===n.id?`2px solid ${C.amber}`:"2px solid transparent",
                  color:tab===n.id?C.amber:C.muted,
                  fontSize:13,fontWeight:tab===n.id?600:400,
                  marginBottom:2,textAlign:"left",fontFamily:"'DM Sans'" }}>
                <span style={{ fontSize:15,fontFamily:"monospace" }}>{n.icon}</span>
                {n.label}
              </button>
            ))}
          </div>

          {/* System status */}
          <div style={{ padding:"12px 14px",borderTop:`1px solid ${C.border}` }}>
            <div style={{ fontSize:10,fontWeight:700,color:C.muted,textTransform:"uppercase",
              letterSpacing:"0.07em",marginBottom:8 }}>System</div>
            <div style={{ display:"flex",flexDirection:"column",gap:6 }}>
              {[
                { label:"MRR", value:`$${mrr.toLocaleString()}`, color:C.amber },
                { label:"Venues", value:`${venues.length} total`, color:C.text },
                { label:"Screens Online", value:`${online}/${venues.length}`, color:C.green },
                { label:"Tiers", value:`${tiers.filter(t=>t.isActive).length} active`, color:C.sky },
              ].map(s=>(
                <div key={s.label} style={{ display:"flex",justifyContent:"space-between" }}>
                  <span style={{ fontSize:11,color:C.muted }}>{s.label}</span>
                  <span style={{ fontFamily:"'DM Mono'",fontSize:11,color:s.color,fontWeight:500 }}>
                    {s.value}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Main */}
        <div style={{ flex:1,display:"flex",flexDirection:"column",overflow:"hidden" }}>
          {/* Top bar */}
          <div style={{ padding:"13px 24px",borderBottom:`1px solid ${C.border}`,
            display:"flex",alignItems:"center",gap:12,flexShrink:0,
            background:C.surf }}>
            <div style={{ fontFamily:"'Syne'",fontWeight:800,fontSize:16 }}>
              {NAV.find(n=>n.id===tab)?.label}
            </div>
            <div style={{ width:1,height:16,background:C.border }} />
            <div style={{ fontSize:12,color:C.muted }}>
              {tab==="tiers"    && `${tiers.length} tiers · ${tiers.filter(t=>t.isPublic).length} public`}
              {tab==="features" && `${ALL_FEATURES.length} feature flags across ${CATS.length} categories`}
              {tab==="venues"   && `${venues.length} venues · $${mrr.toLocaleString()}/mo MRR`}
              {tab==="dashboard"&& "Platform overview"}
            </div>
            <div style={{ marginLeft:"auto",display:"flex",alignItems:"center",gap:10 }}>
              <div style={{ display:"flex",alignItems:"center",gap:6 }}>
                <div style={{ width:6,height:6,borderRadius:"50%",background:C.green,
                  boxShadow:`0 0 5px ${C.green}` }} className="pulse" />
                <span style={{ fontSize:11,color:C.muted }}>All systems operational</span>
              </div>
              <div style={{ fontSize:11,color:C.muted,fontFamily:"'DM Mono'",
                background:C.elev,padding:"4px 10px",borderRadius:6,
                border:`1px solid ${C.border}` }}>
                admin@vennu.app
              </div>
            </div>
          </div>

          {/* Content */}
          <div style={{ flex:1,overflowY:"auto",padding:"20px 24px" }}>
            {tab==="dashboard" && <DashboardTab venues={venues} tiers={tiers} />}
            {tab==="tiers"     && <TiersTab tiers={tiers} setTiers={setTiers} venues={venues} toast={showToast} />}
            {tab==="features"  && <FeaturesTab tiers={tiers} setTiers={setTiers} toast={showToast} />}
            {tab==="venues"    && <VenuesTab venues={venues} setVenues={setVenues} tiers={tiers} toast={showToast} />}
          </div>
        </div>
      </div>
      {toast && <Toast msg={toast} onDone={()=>setToast(null)} />}
    </>
  );
}
