import { useState } from "react";
import { applyAdminTheme, readAdminTheme, type AdminTheme } from "../../back-office/src/adminTheme.mjs";
import SkyIcon from "./SkyIcon";

export default function AdminThemeToggle() {
  const [theme, setTheme] = useState<AdminTheme>(() => readAdminTheme());
  const midnight = theme === "midnight";

  const toggle = () => {
    const next = applyAdminTheme(midnight ? "sky" : "midnight");
    setTheme(next);
  };

  return <button className="admin-theme-toggle" type="button" aria-pressed={midnight} onClick={toggle}>
    <SkyIcon name={midnight ? "sun" : "moon"} size={18} />
    {midnight ? "Use Sky theme" : "Use Midnight theme"}
  </button>;
}
