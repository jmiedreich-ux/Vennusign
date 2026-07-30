import type { DisplayContent } from '../displayContent.mjs';

export default function PromotionBanner({ content }: { content: DisplayContent }) {
  const promotion = content.promotion;
  if (!promotion || (!promotion.title && !promotion.body)) return null;

  return <aside className="promotion-banner" aria-live="polite" data-promotion-id={promotion.id}>
    {promotion.title ? <strong>{promotion.title}</strong> : null}
    {promotion.body ? <span>{promotion.body}</span> : null}
  </aside>;
}
