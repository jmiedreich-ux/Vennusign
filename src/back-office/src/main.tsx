import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import CustomerOnboardingApp from "./CustomerOnboardingApp";

const customerEntryRoute = ["/signup", "/signin", "/onboarding"].includes(window.location.pathname.replace(/\/$/, ""));

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    {customerEntryRoute ? <CustomerOnboardingApp /> : <App />}
  </React.StrictMode>
);
