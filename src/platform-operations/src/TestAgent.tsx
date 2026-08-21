import { useEffect, useState, type FormEvent } from "react";
import type { PlatformOperationsConfiguration } from "./config";
import { cancelTestAgentRun, loadTestAgentRun, loadTestAgentRuns, startTestAgentRun, type TestAgentRun } from "./api";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string };
const examples = [
  "Test whether a new bar customer can create a Saturday-night tap board, publish it, and understand what appears on screen.",
  "Explore the Menus builder. Try realistic editing mistakes and report anything broken or confusing.",
  "Create a cinema session-times board and verify the published result from the customer's point of view."
];

export default function TestAgent({ configuration, apiKey }: Props) {
  const [runs, setRuns] = useState<TestAgentRun[]>([]);
  const [selected, setSelected] = useState<TestAgentRun>();
  const [mission, setMission] = useState(examples[0]);
  const [startUrl, setStartUrl] = useState(configuration.backOfficeBaseUrl);
  const [maxActions, setMaxActions] = useState(20);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();

  const refreshList = () => loadTestAgentRuns(configuration, apiKey).then(setRuns).catch(() => undefined);
  useEffect(() => { refreshList(); }, []); // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => {
    if (!selected || !["queued", "running"].includes(selected.status)) return;
    const controller = new AbortController();
    const timer = window.setInterval(() => loadTestAgentRun(configuration, apiKey, selected.id, controller.signal)
      .then(run => { setSelected(run); setRuns(current => [run, ...current.filter(item => item.id !== run.id)]); })
      .catch(() => undefined), 1500);
    return () => { controller.abort(); window.clearInterval(timer); };
  }, [apiKey, configuration, selected?.id, selected?.status]);

  const start = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try { const run = await startTestAgentRun(configuration, apiKey, mission, startUrl, maxActions); setSelected(run); setRuns(current => [run, ...current]); }
    catch (reason) { setError(reason instanceof Error ? reason.message : "Unable to start the AI test."); }
    finally { setBusy(false); }
  };

  const cancel = async () => { if (!selected) return; await cancelTestAgentRun(configuration, apiKey, selected.id); };
  const lastShot = selected ? [...selected.events].reverse().find(event => event.screenshotBase64)?.screenshotBase64 : undefined;

  return <section className="test-agent-page">
    <div className="test-agent-intro">
      <div><span className="experiment-pill">Prototype experiment</span><h2>Give the AI a mission. Watch it test Vennue.</h2><p>The agent sees the real product, chooses its own Playwright actions, adapts when the workflow changes, and returns evidence—not just a pass/fail badge.</p></div>
      <div className="agent-orb" aria-hidden="true"><span>AI</span></div>
    </div>

    <div className="test-agent-grid">
      <form className="mission-card" onSubmit={start}>
        <div className="panel-heading"><div><small>Mission control</small><h3>What should the agent prove?</h3></div><span className="agent-state ready">Ready</span></div>
        <label htmlFor="mission">Testing mission</label>
        <textarea id="mission" value={mission} maxLength={4000} onChange={event => setMission(event.target.value)} required rows={7} />
        <div className="mission-examples">{examples.map((example, index) => <button type="button" key={example} onClick={() => setMission(example)}>Example {index + 1}</button>)}</div>
        <div className="mission-options"><label>Start URL<input type="url" value={startUrl} onChange={event => setStartUrl(event.target.value)} required /></label><label>Action limit<input type="number" min="1" max="30" value={maxActions} onChange={event => setMaxActions(Number(event.target.value))} /></label></div>
        {error && <p className="test-agent-error" role="alert">{error}</p>}
        <button className="launch-agent" disabled={busy || !mission.trim()}>{busy ? "Starting agent…" : "Launch AI test"}<span aria-hidden="true">→</span></button>
        <p className="mission-safety">Runs are restricted to the experiment configuration, stop at the action limit, and keep all state in memory.</p>
      </form>

      <div className="live-run-card">
        <div className="panel-heading"><div><small>Live browser</small><h3>{selected ? selected.mission : "No mission running"}</h3></div>{selected && <span className={`agent-state ${selected.status}`}>{selected.status}</span>}</div>
        {selected ? <>
          <div className="run-meter"><div style={{ width: `${Math.max(4, selected.actionsCompleted / selected.maxActions * 100)}%` }} /></div>
          <div className="browser-frame">{lastShot ? <img src={`data:image/jpeg;base64,${lastShot}`} alt="Latest screenshot captured by the AI test agent" /> : <div className="browser-empty"><span>◌</span><p>Browser is starting…</p></div>}</div>
          <div className="run-facts"><span><strong>{selected.actionsCompleted}</strong> actions</span><span><strong>{selected.events.length}</strong> events</span><span><strong>{selected.maxActions}</strong> limit</span></div>
          {["queued", "running"].includes(selected.status) && <button className="stop-agent" type="button" onClick={cancel}>Stop run</button>}
          {(selected.assessment || selected.error) && <div className="agent-assessment"><small>Agent assessment</small><p>{selected.assessment ?? selected.error}</p></div>}
        </> : <div className="empty-live"><div className="radar" /><h3>The browser will appear here</h3><p>Launch a mission to watch each decision and inspect the evidence as it arrives.</p></div>}
      </div>
    </div>

    <div className="run-history"><div className="panel-heading"><div><small>Evidence trail</small><h3>Recent runs</h3></div><button type="button" onClick={refreshList}>Refresh</button></div>
      {runs.length === 0 ? <p className="empty-history">Completed and interrupted missions will remain here until the API restarts.</p> : <div className="run-list">{runs.map(run => <button type="button" className={selected?.id === run.id ? "selected" : ""} onClick={() => setSelected(run)} key={run.id}><span className={`run-dot ${run.status}`} /><span><strong>{run.mission}</strong><small>{new Date(run.createdUtc).toLocaleString()} · {run.actionsCompleted} actions</small></span><em>{run.status}</em></button>)}</div>}
    </div>
  </section>;
}
