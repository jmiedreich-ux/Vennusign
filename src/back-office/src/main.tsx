import React from "react";
import ReactDOM from "react-dom/client";
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
