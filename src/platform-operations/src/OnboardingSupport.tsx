import { useEffect, useMemo, useState } from "react";
import { PlatformOperationsApiError, loadOnboardingSupport, type OnboardingSupportItem } from "./api";
import type { PlatformOperationsConfiguration } from "./config";

const steps = ["Account", "Plan", "Venue", "First Screen", "Go Live"] as const;

function completedSteps(item: OnboardingSupportItem) {
  return [true, Boolean(item.tierId), Boolean(item.venueId), Boolean(item.firstScreenId), item.firstScreenStatus === "online"];
}

function isStale(value: string) {
  return Date.now() - new Date(value).getTime() > 7 * 24 * 60 * 60 * 1000;
}

export default function OnboardingSupport({ configuration, apiKey }: { configuration: PlatformOperationsConfiguration; apiKey: string }) {
  const [search, setSearch] = useState("");
  const [items, setItems] = useState<OnboardingSupportItem[]>([]);
  const [selectedId, setSelectedId] = useState<string>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [refreshVersion, setRefreshVersion] = useState(0);
  const selected = useMemo(() => items.find(item => item.userId === selectedId) ?? items[0], [items, selectedId]);

  useEffect(() => {
    const controller = new AbortController();
    const timeout = window.setTimeout(() => {
      setLoading(true); setError(undefined);
      loadOnboardingSupport(configuration, apiKey, search, controller.signal)
        .then(values => { setItems(values); setSelectedId(current => values.some(item => item.userId === current) ? current : values[0]?.userId); })
        .catch(reason => { if (!(reason instanceof DOMException && reason.name === "AbortError")) setError(reason instanceof PlatformOperationsApiError ? reason.message : "Customer onboarding support is unavailable."); })
        .finally(() => setLoading(false));
    }, 250);
    return () => { window.clearTimeout(timeout); controller.abort(); };
  }, [apiKey, configuration, search, refreshVersion]);

  const copyContext = async () => {
    if (!selected) return;
    const complete = completedSteps(selected).filter(Boolean).length;
    try {
      await navigator.clipboard.writeText(`Customer: ${selected.customerEmail}\nOrganization: ${selected.organizationName ?? "Not created"}\nVenue: ${selected.venueName ?? "Not created"}\nPlan: ${selected.tierName ?? "Not selected"} (${selected.subscriptionStatus})\nProgress: ${complete}/5\nFirst screen: ${selected.firstScreenStatus}\nLast activity: ${selected.lastActivityUtc}`);
      setNotice("Support context copied without credentials or provider identifiers.");
    } catch {
      setNotice("Copy was blocked by this browser. No customer state was changed.");
    }
  };

  return <section className="onboarding-support" aria-labelledby="onboarding-support-heading">
    <div className="onboarding-support__heading"><div><p>Customer success · Platform Operations support</p><h2 id="onboarding-support-heading">Onboarding journeys</h2><span>Read-only operational context from persisted customer state. Customer forms remain in Back Office; this surface never enters or impersonates a customer workspace.</span></div><button type="button" onClick={() => { setNotice(undefined); setRefreshVersion(value => value + 1); }}>Refresh</button></div>
    <label className="onboarding-support__search" htmlFor="onboarding-search">Find customer, organization, or venue<input id="onboarding-search" maxLength={100} value={search} onChange={event => setSearch(event.target.value)} placeholder="Search by name or email…" /></label>
    {notice ? <p className="onboarding-support__notice" role="status">{notice}</p> : null}
    {loading ? <p className="state" role="status">Loading onboarding journeys…</p> : error ? <p className="state error" role="alert">{error}</p> : items.length === 0 ? <p className="state">No onboarding journeys match this search.</p> : <div className="onboarding-support__layout">
      <div className="onboarding-support__list" role="list" aria-label="Customer onboarding journeys">{items.map(item => <div role="listitem" key={item.userId}><button type="button" className={selected?.userId === item.userId ? "selected" : ""} onClick={() => { setSelectedId(item.userId); setNotice(undefined); }}><strong>{item.organizationName ?? item.customerName}</strong><span>{item.customerEmail}</span><small>{completedSteps(item).filter(Boolean).length}/5 · {item.subscriptionStatus}</small></button></div>)}</div>
      {selected ? <article className="onboarding-support__detail">
        <header><div><p>{selected.customerName}</p><h3>{selected.organizationName ?? "Organization not created"}</h3><span>{selected.customerEmail}</span></div><button type="button" onClick={copyContext}>Copy support context</button></header>
        <ol className="support-timeline" aria-label="Customer onboarding progress">{steps.map((step, index) => { const complete = completedSteps(selected)[index]; const current = !complete && completedSteps(selected).slice(0, index).every(Boolean); return <li key={step} data-state={complete ? "complete" : current ? "current" : "upcoming"} aria-current={current ? "step" : undefined}><strong>{step}</strong><span>{complete ? "Complete" : current ? "Current" : "Upcoming"}</span></li>; })}</ol>
        <dl className="onboarding-support__facts"><div><dt>Plan</dt><dd>{selected.tierName ?? "Not selected"}</dd></div><div><dt>Subscription</dt><dd>{selected.subscriptionStatus}</dd></div><div><dt>Trial ends</dt><dd>{selected.trialEndsAt ? new Date(selected.trialEndsAt).toLocaleString() : "Not applicable"}</dd></div><div><dt>Venue</dt><dd>{selected.venueName ?? "Not created"}</dd></div><div><dt>First screen</dt><dd>{selected.firstScreenName ?? "Not paired"} · {selected.firstScreenStatus.replace("-", " ")}</dd></div><div><dt>Last activity</dt><dd><time dateTime={selected.lastActivityUtc}>{new Date(selected.lastActivityUtc).toLocaleString()}</time>{isStale(selected.lastActivityUtc) ? " · Stale (over 7 days)" : " · Recent"}</dd></div></dl>
      </article> : null}
    </div>}
  </section>;
}
