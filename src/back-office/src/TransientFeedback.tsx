import { useEffect, useRef } from "react";

type Props = { message: string; onDismiss: () => void; timeoutMs?: number };

export default function TransientFeedback({ message, onDismiss, timeoutMs = 7000 }: Props) {
  const dismissRef = useRef(onDismiss);
  dismissRef.current = onDismiss;

  useEffect(() => {
    const timer = window.setTimeout(() => dismissRef.current(), timeoutMs);
    return () => window.clearTimeout(timer);
  }, [message, timeoutMs]);

  return <div className="transient-feedback-region" aria-live="polite" aria-atomic="true">
    <div className="transient-feedback" role="status">
      <span aria-hidden="true">✓</span>
      <p>{message}</p>
      <button type="button" aria-label="Dismiss success message" onClick={onDismiss}>×</button>
    </div>
  </div>;
}
