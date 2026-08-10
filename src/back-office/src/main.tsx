import React from "react";
import ReactDOM from "react-dom/client";
// Self-hosted, not fetched from Google. The board engine names this family and
// deliberately does not depend on it: the engine must import nothing, so each
// application that draws a board loads the font itself. 400 and 600 are the two
// weights the board uses - descriptions and item names.
import "@fontsource/playfair-display/400.css";
import "@fontsource/playfair-display/600.css";
import "@fontsource/playfair-display/400-italic.css";
import App from "./App";
import AdminThemeToggle from "./AdminThemeToggle";
import CustomerOnboardingApp from "./CustomerOnboardingApp";
import { initializeAdminTheme } from "./adminTheme.mjs";

const customerEntryRoute = ["/signup", "/signin", "/onboarding"].includes(window.location.pathname.replace(/\/$/, ""));
initializeAdminTheme();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <AdminThemeToggle />
    {customerEntryRoute ? <CustomerOnboardingApp /> : <App />}
  </React.StrictMode>
);
