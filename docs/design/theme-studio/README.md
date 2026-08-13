# Vennue Theme Studio

**Status:** Working design bundle; owner review in progress  
**Last updated:** 2026-08-13  
**Live workflow mock:** https://vennue-theme-studio-draft.jmiedreich.chatgpt.site  
**Published mock revision:** Sites version 13; source checkpoint `35c33d1`

## Purpose

This folder preserves the current Theme Studio design decisions, mock reference, and owner-testing handoff. It is the restart point for continuing the design without reconstructing prior conversations.

This bundle documents a working mock. It does not authorize production implementation or replace the Menus design authority under `docs/design/approved/menus/`.

## Documents

- [decisions.md](decisions.md) — current product and interaction decisions.
- [owner-test-handoff.md](owner-test-handoff.md) — verified behavior, remaining tests, and the exact next action.

## Current design outcome

Theme Studio creates and edits reusable visual definitions for structured content. The current mock uses the Menu data model and demonstrates:

- a theme library and Create new flow;
- display surface setup;
- starting from a tested theme, generated structure, or a blank canvas;
- optional hand-holding guidance for a blank build;
- direct canvas placement of components and fields;
- Settings, Variants, Test, diagnostics, save, and Menu Builder handoff states;
- a save/publish boundary that never implies a screen changed merely because a theme was saved.

The owner has verified the guided blank-build path through the automatic transition from Appearance to Variants. Testing resumes at the Sold out variant step.

## Scope boundary

The mock is a design artifact, not the production Theme Studio implementation. Production work must separately reconcile:

- the shared canvas/player rendering engine;
- theme-definition contracts and persistence;
- static image and live/hybrid output modes;
- clipping and required-content validation;
- permissions, tiers, accessibility, responsive behavior, persistence, and recovery;
- the existing Menus feature plan and approved design bundle.
