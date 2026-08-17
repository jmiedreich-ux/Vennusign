# Handoff: Vennusign customer authentication

## Overview

How a customer signs in, and how MFA is enforced, across Google, Apple, and
"Sign in with Vennusign" (all via Microsoft Entra External ID), plus the
dev/stage exemption used for testing. Replaces the ad hoc mix in the existing
custom `CustomerAuthentication` system as the design authority going forward.

**Read `decisions.md` first.** Eleven numbered decisions, settled with the
owner. They are written as rules, not descriptions; the mockup below
illustrates them but the decisions govern where the two disagree.

## About the design file

`Login Hi-Fi.html` is a **design reference**, not production code to copy. Four
states, shown side by side:

1. Returning visitor — remembered method, one prominent button (decision 11)
2. First visit / "more ways to sign in" expanded — all three providers, email
   link demoted below a divider (decisions 2, 10, 11)
3. MFA step-up — TOTP code entry, the path everyone except Passkey users takes
   (decision 6)
4. Dev/stage — MFA off, automatic sign-in, config-gated so it structurally
   cannot reach `app` (decisions 7, 8)

Unlike the menus bundle's `.dc.html` files, this is plain HTML/CSS — it does
not depend on the menus bundle's design-tool export format
(`support.js`/`x-dc`), which this session did not have access to. It reuses
`sky-ui-tokens.css` (copied from the menus bundle, same source of truth) so it
stays visually consistent with the rest of the product rather than introducing
a separate visual language for one screen.

The task is to **recreate this in the target codebase** — most likely
`src/back-office/` and wherever the customer-facing login surface lives —
using the product's existing component patterns, not by copying this file's
inline styles directly.

## Fidelity

High — real spacing, type, and color from `sky-ui-tokens.css`, both the light
palette and the `midnight` dark palette (state 4 uses midnight, matching a
dev-tool aesthetic distinct from the customer-facing states). Icons for
Google/Apple/Vennusign are illustrative placeholders, not final asset exports.

## Open from decisions.md

The Migration note in `decisions.md` is explicitly not decided: whether the
existing `src/Vennu.Data/Services/Customer*.cs` /
`src/Vennu.Api/CustomerAuthentication/` code is replaced, kept as a fallback,
or partially reused is implementation scope for whoever picks this up, not
settled here.
