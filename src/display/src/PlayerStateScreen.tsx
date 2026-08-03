type PlayerStateScreenProps = {
  eyebrow: string;
  title: string;
  message: string;
  busy?: boolean;
  tone?: 'loading' | 'error';
  actionLabel?: string;
  onAction?: () => void;
};

export default function PlayerStateScreen({
  eyebrow,
  title,
  message,
  busy = false,
  tone = 'loading',
  actionLabel,
  onAction
}: PlayerStateScreenProps) {
  return (
    <main
      className={`player-state-screen player-state-screen--${tone}`}
      aria-busy={busy || undefined}
      role={tone === 'error' ? 'alert' : 'status'}
    >
      <section className="player-state-screen__panel">
        <span className="player-state-screen__mark" aria-hidden="true">
          <span className="player-state-screen__heartbeat" />
        </span>
        <p className="player-state-screen__eyebrow">{eyebrow}</p>
        <h1>{title}</h1>
        <p className="player-state-screen__message">{message}</p>
        {actionLabel && onAction ? (
          <button className="player-state-screen__action" type="button" onClick={onAction}>
            {actionLabel}
          </button>
        ) : null}
      </section>
    </main>
  );
}
