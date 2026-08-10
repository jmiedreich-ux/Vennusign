import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from "@vitejs/plugin-basic-ssl";

export default defineConfig({
  plugins: [react(), basicSsl()],
  base: "/",
  // The board engine lives at src/board-engine/, outside this application, so
  // milestone 4's display player can consume the same component rather than
  // reimplement it. Node resolution walks up from the engine's own folder and
  // finds no node_modules there, so React is resolved from this project's root
  // instead - which is also what keeps it to ONE React. A second copy in the
  // bundle breaks hooks in ways that read as unrelated runtime bugs.
  resolve: { dedupe: ["react", "react-dom"] },
  server: { host: "localhost", port: 5174, https: true }
});
