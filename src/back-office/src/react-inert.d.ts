/*
 * React 18's typings predate the `inert` attribute; React 19 declares it. The
 * attribute has been in the DOM since 2023 and is what actually holds the regions
 * behind a scrim unreachable, so it is declared once here rather than cast away at
 * each use — a cast would hide the day the typings catch up and this becomes a
 * duplicate.
 *
 * Typed `"" | undefined` deliberately. React 18 forwards an unknown prop as a
 * string attribute, so `inert=""` sets it and `undefined` removes it, while a
 * boolean `false` would stringify to "false" — which the DOM reads as *present*
 * and would trap focus permanently.
 */
import "react";

declare module "react" {
  interface HTMLAttributes<T> {
    inert?: "" | undefined;
  }
}
