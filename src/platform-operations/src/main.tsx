import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import AdminThemeToggle from "./AdminThemeToggle";
import { initializeAdminTheme } from "../../back-office/src/adminTheme.mjs";

initializeAdminTheme();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <AdminThemeToggle />
    <App />
  </React.StrictMode>
);
