export type SkyIconName = "check" | "close" | "screen" | "search" | "refresh" | "key";

const paths: Record<SkyIconName, React.ReactNode> = {
  check: <path d="m5 12 4 4L19 6" />,
  close: <><path d="m6 6 12 12" /><path d="M18 6 6 18" /></>,
  screen: <><rect x="3" y="4" width="18" height="13" rx="2" /><path d="M8 21h8M12 17v4" /></>,
  search: <><circle cx="11" cy="11" r="7" /><path d="m20 20-4-4" /></>,
  refresh: <><path d="M20 11a8 8 0 1 0-2.3 5.7" /><path d="M20 4v7h-7" /></>,
  key: <><circle cx="8" cy="15" r="4" /><path d="m11 12 8-8M15 8l3 3M17 6l2 2" /></>
};

export default function SkyIcon({ name, size = 20 }: { name: SkyIconName; size?: number }) {
  return <svg className="sky-icon" width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" focusable="false">
    {paths[name]}
  </svg>;
}
