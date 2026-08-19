import React from "react";
import ReactDOM from "react-dom/client";
import Home from "./Home";
import Restaurants from "./Restaurants";
import CorporateComms from "./CorporateComms";
import "@fontsource/caveat/400.css";
import "@fontsource/caveat/700.css";
import "@fontsource/kalam/400.css";
import "@fontsource/kalam/700.css";
import "@fontsource/patrick-hand/400.css";
import "@fontsource/pacifico/400.css";
import "@fontsource/righteous/400.css";
import "./styles.css";

// A tiny, dependency-free path switch - this is a handful of static marketing
// pages, not an app with client-side navigation state to preserve, so plain
// full-page <a href> links between them (as authored in each page) are the
// simplest correct choice over pulling in a router.
const page = window.location.pathname.replace(/\/$/, "");
const Page = page === "/restaurants" ? Restaurants : page === "/corporate-comms" ? CorporateComms : Home;

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <Page />
  </React.StrictMode>
);
