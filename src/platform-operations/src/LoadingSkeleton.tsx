export default function LoadingSkeleton({ label, rows = 3 }: { label: string; rows?: number }) {
  return <div className="sky-loading-skeleton" role="status" aria-live="polite" aria-busy="true">
    <span className="sr-only">{label}</span>
    {Array.from({ length: rows }, (_, index) => <span className="sky-loading-skeleton__row" key={index} aria-hidden="true" />)}
  </div>;
}
