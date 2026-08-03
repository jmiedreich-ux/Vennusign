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

  if (!request) return null;
  const confirmationMatches = request.typedConfirmation === undefined || confirmation === request.typedConfirmation;
  const titleId = "destructive-review-title";
  const descriptionId = "destructive-review-description";

  return <dialog
    ref={dialog}
    className={`destructive-review-dialog destructive-review-dialog--${request.tone ?? "danger"}`}
    aria-labelledby={titleId}
    aria-describedby={descriptionId}
    onCancel={event => { event.preventDefault(); onDecision(false); }}
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
        <button type="button" autoFocus onClick={() => onDecision(false)}>Cancel</button>
        <button className="danger" type="submit" disabled={!confirmationMatches}>{request.confirmLabel}</button>
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
    setPending({ request, resolve });
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
