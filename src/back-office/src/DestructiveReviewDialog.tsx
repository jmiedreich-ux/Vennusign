import { useCallback, useEffect, useRef, useState, type ReactNode } from "react";

export type DestructiveReviewRequest = Readonly<{
  title: string;
  consequence: string;
  confirmLabel: string;
  typedConfirmation?: string;
  tone?: "danger" | "caution";
}>;

type PendingReview = Readonly<{
  request: DestructiveReviewRequest;
  resolve: (confirmed: boolean) => void;
}>;

function DestructiveReviewDialog({ pending, onDecision }: Readonly<{
  pending?: PendingReview;
  onDecision: (confirmed: boolean) => void;
}>) {
  const dialog = useRef<HTMLDialogElement>(null);
  const [confirmation, setConfirmation] = useState("");
  const request = pending?.request;

  useEffect(() => {
    setConfirmation("");
    if (request && dialog.current && !dialog.current.open) dialog.current.showModal();
  }, [request]);

  /**
   * showModal() is supposed to confine Tab to the dialog, but focus was observed
   * escaping after two presses. A destructive confirmation that leaks focus lets a
   * keyboard user operate the page behind it, so the cycle is enforced explicitly.
   */
  const trapFocus = useCallback((event: React.KeyboardEvent<HTMLDialogElement>) => {
    if (event.key !== "Tab" || !dialog.current) return;
    const focusable = Array.from(
      dialog.current.querySelectorAll<HTMLElement>(
        'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
      )
    ).filter(element => element.offsetParent !== null || element === document.activeElement);
    if (focusable.length === 0) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement as HTMLElement | null;

    if (event.shiftKey && (active === first || !dialog.current.contains(active))) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && (active === last || !dialog.current.contains(active))) {
      event.preventDefault();
      first.focus();
    }
  }, []);

  if (!request) return null;
  const confirmationMatches = request.typedConfirmation === undefined || confirmation === request.typedConfirmation;
  const titleId = "destructive-review-title";
  const descriptionId = "destructive-review-description";

  return <dialog
    ref={dialog}
    className={`destructive-review-dialog destructive-review-dialog--${request.tone ?? "danger"}`}
    data-testid="destructive-review-dialog"
    data-tone={request.tone ?? "danger"}
    aria-labelledby={titleId}
    aria-describedby={descriptionId}
    onCancel={event => { event.preventDefault(); onDecision(false); }}
    onKeyDown={trapFocus}
  >
    <form method="dialog" onSubmit={event => { event.preventDefault(); if (confirmationMatches) onDecision(true); }}>
      <p className="destructive-review-dialog__eyebrow">Review impact</p>
      <h2 id={titleId}>{request.title}</h2>
      <p id={descriptionId}>{request.consequence}</p>
      {request.typedConfirmation !== undefined ? <label>
        Type <strong>{request.typedConfirmation}</strong> to confirm
        <input autoComplete="off" value={confirmation} onChange={event => setConfirmation(event.target.value)} />
      </label> : null}
      <div className="destructive-review-dialog__actions">
        <button type="button" data-testid="destructive-cancel" autoFocus onClick={() => onDecision(false)}>Cancel</button>
        <button className={request.tone === "caution" ? "caution" : "danger"} data-testid="destructive-confirm" type="submit" disabled={!confirmationMatches}>{request.confirmLabel}</button>
      </div>
    </form>
  </dialog>;
}

export function useDestructiveReview(): Readonly<{
  review: (request: DestructiveReviewRequest) => Promise<boolean>;
  reviewDialog: ReactNode;
}> {
  const [pending, setPending] = useState<PendingReview>();
  const pendingRef = useRef<PendingReview>();

  useEffect(() => { pendingRef.current = pending; }, [pending]);
  useEffect(() => () => { pendingRef.current?.resolve(false); }, []);

  const review = useCallback((request: DestructiveReviewRequest) => new Promise<boolean>(resolve => {
    pendingRef.current?.resolve(false);
    const next = { request, resolve };
    pendingRef.current = next;
    setPending(next);
  }), []);
  const decide = useCallback((confirmed: boolean) => {
    const current = pendingRef.current;
    if (!current) return;
    pendingRef.current = undefined;
    setPending(undefined);
    current.resolve(confirmed);
  }, []);

  return { review, reviewDialog: <DestructiveReviewDialog pending={pending} onDecision={decide} /> };
}
