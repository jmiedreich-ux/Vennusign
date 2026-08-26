import { ClipboardPaste } from "lucide-react";

/**
 * The ways a menu gets into Vennusign, in one component.
 *
 * Decision 17 puts these on the Menus home permanently rather than in a signup
 * wizard, and decision 30 has every route converge on the same review and
 * confirm. So there is one list of routes and two places it is drawn: full-page
 * on an empty shelf, and inside a dialog once there are menus. Same routes, same
 * copy, same order — a second implementation is how the two drift.
 *
 * **Only routes that exist are drawn.** Photo, spreadsheet and POS are not built.
 * `README.md`'s M1a already settles what an absent route looks like, for POS:
 * "when it is not, there is no trace of it — decision 4". No disabled cards, no
 * "coming soon". `menuAddRoutes` is a list precisely so the others append here
 * when they are real, and the layout does not need redesigning to take them.
 */
export type MenuAddRoute = {
  key: string;
  title: string;
  blurb: string;
  icon: JSX.Element;
  /** The route drawn first and highlighted. Exactly one route carries it. */
  leads: boolean;
};

export const menuAddRoutes: readonly MenuAddRoute[] = Object.freeze([
  {
    key: "paste",
    title: "Paste it in",
    blurb: "Copy your menu from a document, a spreadsheet or an email. We read the headings and prices — there is no format to learn.",
    icon: <ClipboardPaste aria-hidden="true" />,
    leads: true
  }
]);

type Props = {
  /** `page` is the empty shelf; `dialog` is the same list inside Add a menu. */
  variant: "page" | "dialog";
  onChoose: (routeKey: string) => void;
  onStartBlank: () => void;
  onCancel?: () => void;
  busy?: boolean;
  error?: string | null;
};

export default function MenuAddRoutes({ variant, onChoose, onStartBlank, onCancel, busy = false, error }: Props) {
  return (
    <div className={`add-routes add-routes--${variant}`} data-testid="menu-add-routes">
      <h1 className="add-routes__title" id="add-routes-title">Let's get your menu in.</h1>
      <p className="add-routes__lead">Pick whatever's easiest. You can fix anything later.</p>

      <div className="add-routes__cards" data-route-count={menuAddRoutes.length}>
        {menuAddRoutes.map(route => (
          <button
            key={route.key}
            type="button"
            className={`add-routes__card${route.leads ? " add-routes__card--leads" : ""}`}
            data-testid={`add-route-${route.key}`}
            disabled={busy}
            onClick={() => onChoose(route.key)}
          >
            <span className="add-routes__mark">{route.icon}</span>
            <strong>{route.title}</strong>
            <small>{route.blurb}</small>
          </button>
        ))}
      </div>

      {/*
        Blank is a link and not a card, per README.md's M1a empty state. It is the
        route you take when you have nothing to bring, and drawing it as a peer of
        the import routes is how it became the only one anybody could reach.
      */}
      <button
        type="button"
        className="add-routes__blank"
        data-testid="add-route-blank"
        disabled={busy}
        onClick={onStartBlank}
      >
        {busy ? "Creating…" : "or start from a blank board"}
      </button>

      {error ? <p className="add-routes__error" role="alert" data-testid="add-route-error">{error}</p> : null}

      {onCancel ? (
        <div className="add-routes__actions">
          <button type="button" className="action-secondary" onClick={onCancel} disabled={busy}>Cancel</button>
        </div>
      ) : null}
    </div>
  );
}
