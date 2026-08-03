import type { CustomerOnboardingSnapshot } from "./customerOnboardingApi";

const timelineSteps = [
  { progressKey: "account", routeKey: "account", label: "Account", description: "Secure customer identity" },
  { progressKey: "plan", routeKey: "plan", label: "Plan", description: "Trial or paid entitlement" },
  { progressKey: "venue", routeKey: "venue", label: "Venue", description: "Venue details saved" },
  { progressKey: "firstScreen", routeKey: "first-screen", label: "First Screen", description: "Physical display paired" },
  { progressKey: "goLive", routeKey: "go-live", label: "Go Live", description: "Player heartbeat Online" }
] as const;

export default function CustomerOnboardingTimeline({ onboarding }: { onboarding: CustomerOnboardingSnapshot }) {
  const completedCount = timelineSteps.filter(step => onboarding.progress[step.progressKey]).length;
  const complete = completedCount === timelineSteps.length;
  const current = timelineSteps.find(step => step.routeKey === onboarding.currentStep) ?? timelineSteps[0];

  return <section className="customer-timeline" aria-labelledby="customer-timeline-heading">
    <div className="customer-timeline__summary">
      <div>
        <span>Resumable setup</span>
        <h2 id="customer-timeline-heading">Opening progress</h2>
      </div>
      <p role="status" aria-live="polite"><strong>{completedCount} of {timelineSteps.length}</strong> steps complete</p>
    </div>
    <ol className="customer-timeline__steps" aria-label="Customer onboarding timeline">
      {timelineSteps.map((step, index) => {
        const completed = onboarding.progress[step.progressKey];
        const isCurrent = !completed && step.routeKey === onboarding.currentStep;
        const state = completed ? "Complete" : isCurrent ? "Current" : "Upcoming";
        return <li key={step.progressKey} data-state={state.toLowerCase()} aria-current={isCurrent ? "step" : undefined}>
          <span className="customer-timeline__number" aria-hidden="true">{completed ? "✓" : index + 1}</span>
          <span className="customer-timeline__copy"><strong>{step.label}</strong><small>{step.description}</small><em>{state}</em></span>
        </li>;
      })}
    </ol>
    <div className="customer-timeline__resume">
      <div>
        <strong>{complete ? "Your opening journey is complete." : `${current.label} is your current step.`}</strong>
        <span>Last saved <time dateTime={onboarding.updatedUtc}>{new Date(onboarding.updatedUtc).toLocaleString()}</time></span>
      </div>
      {!complete ? <a href="#onboarding-current-task">Continue {current.label}</a> : <span className="customer-timeline__complete">Online and ready</span>}
    </div>
  </section>;
}
