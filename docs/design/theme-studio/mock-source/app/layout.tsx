import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Vennue Theme Studio Draft",
  description: "Interactive product-design draft for Vennue Theme Studio and its Template Repair Agent.",
  other: {
    "codex-preview": "development",
  },
  icons: {
    icon: "/favicon.svg",
    shortcut: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        <div hidden dangerouslySetInnerHTML={{__html: `<!--
THESIS: The shared renderer is the workbench; the bright signage surface stays visually dominant instead of dissolving into dashboard cards.
OWN-WORLD: Layered charcoal instrument panels, restrained cool blue bindings, precise amber diagnostics, compact square controls, and one warm-white publication canvas.
STORY: Create structured content, inspect its rules, stress-test real variation, repair transparently, then publish a deterministic version.
FIRST VIEWPORT: A narrow tool rail and structured-component tray frame a large exact menu canvas; the selected repeater, inspector, and actionable diagnostics are visible together.
FORM: Broadcast control-room workbench, user-pinned direction; seed unavailable because the external seed request was blocked.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, and DESIGN.md
-->`}} />
        {children}
      </body>
    </html>
  );
}
