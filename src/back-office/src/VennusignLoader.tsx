/**
 * The one loading state Vennusign uses.
 *
 * A screen waking up: the display powers on, menu lines arrive, a price lands,
 * and light sweeps the glass. It shows the product doing its job rather than a
 * generic spinner, and it reads as deliberate at any duration - which matters,
 * because a cold start on a shared plan can take tens of seconds.
 *
 * Two variants, because loading is two different situations:
 *   modal   the whole page is waiting and there is nothing else to look at
 *   inline  one region is waiting inside a page that has already rendered
 *
 * Never use `modal` for a region. Covering a rendered page to load part of it
 * hides work the person can already see and act on.
 */
type LoaderVariant = "modal" | "inline";

export default function VennusignLoader({
  message,
  variant = "inline"
}: {
  message: string;
  variant?: LoaderVariant;
}) {
  // aria-live is deliberately polite and the text is the real message: the
  // animation is decorative, so screen readers get the sentence, not the scenery.
  const art = <div className="vennu-loader__art" aria-hidden="true">
    <div className="vennu-loader__screen">
      <span className="vennu-loader__row" />
      <span className="vennu-loader__row" />
      <span className="vennu-loader__row" />
      <span className="vennu-loader__price" />
    </div>
    <div className="vennu-loader__neck" />
    <div className="vennu-loader__base" />
  </div>;

  if (variant === "modal") {
    return <div className="vennu-loader vennu-loader--modal" role="status" aria-live="polite" aria-busy="true">
      <div className="vennu-loader__card">{art}<p className="vennu-loader__caption">{message}</p></div>
    </div>;
  }

  return <div className="vennu-loader vennu-loader--inline" role="status" aria-live="polite" aria-busy="true">
    {art}<p className="vennu-loader__caption">{message}</p>
  </div>;
}
