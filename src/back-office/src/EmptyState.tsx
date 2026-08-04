import type { ReactNode } from "react";
import SkyIcon, { type SkyIconName } from "./SkyIcon";

type Props = { icon: SkyIconName; title: string; message: string; action?: ReactNode };

export default function EmptyState({ icon, title, message, action }: Props) {
  return <div className="sky-empty-state">
    <span className="sky-empty-state__icon"><SkyIcon name={icon} size={24} /></span>
    <div><strong>{title}</strong><p>{message}</p></div>
    {action ? <div className="sky-empty-state__action">{action}</div> : null}
  </div>;
}
