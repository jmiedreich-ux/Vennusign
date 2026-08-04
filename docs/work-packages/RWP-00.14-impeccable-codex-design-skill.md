# RWP-00.14 — Project-Local Impeccable Codex Design Skill

## Tracking

- Issue: #486
- Mode: Sequential
- Branch: `rwp/00.14-impeccable-codex-skill`
- Dependency: approved 18-item remediation queue complete

## Accepted Scope

- Install the complete official Impeccable v4.0.4 Codex skill under `.agents/skills/impeccable`.
- Preserve its command playbooks, detector rules, scripts, agent definitions, Apache-2.0 license, and notice.
- Install the project-local Codex hook under `.codex/hooks.json`, resolving commands to the canonical `.agents/skills` location on POSIX and Windows.
- Require the skill for Vennusign UI work and document its relationship to the existing UI gap analysis and merge gates.
- Validate skill discovery, internal references, JSON syntax, Node compatibility, and a bounded hook/detector invocation.

## Exclusions

- No product runtime, UI, API, persistence, authorization, entitlement, schema, dependency, or CI workflow changes.
- No automatic creation of `PRODUCT.md`, `DESIGN.md`, or surface briefs; those require a later explicit Impeccable command against a concrete product surface.
- No activation of held RWP-13.06 / issue #466 and no Phase 14+ work.
- No Azure SQL, external-service, credentialed, hosted-infrastructure, container, device, signing/store, cross-system, or other integration-type tests.

## Acceptance Criteria

1. Codex can discover and load Impeccable from the project-local skill directory.
2. Every file referenced by the installed `SKILL.md` exists and the included scripts parse under the repository's supported Node runtime.
3. The advisory hook resolves to the installed skill on POSIX and Windows and exits without disrupting non-UI work.
4. `AGENTS.md` directs changed UI work through Impeccable while retaining the repository's UI/function gap analysis, affected-area validation, and exact-head review requirements.
5. Completion records preserve the held issue and Phase 14 pause.

## Validation

- Parse `.codex/hooks.json`.
- Verify the skill manifest version and every relative Markdown/script reference.
- Run Node syntax checks across installed `.mjs` and `.js` files.
- Invoke the context loader and advisory detector against a bounded existing frontend target without modifying product files.
- Run repository documentation/classification validation through GitHub Actions on the exact PR head.

## Proposed Completion

The official Impeccable v4.0.4 Codex skill and project hook are installed and governed as a reusable Vennusign UI-design capability. Product behavior is unchanged, the approved product queue remains complete, RWP-13.06 stays held, and Phase 14+ stays paused.

Local integrity validation passed: the hook JSON parses, all 31 manifest references resolve, all installed JavaScript modules parse, the context loader recognizes Vennusign's incumbent visual system, the bounded detector reports zero findings, and the advisory hook exits successfully. GitHub Actions run #932 passed the complete non-integration suite on installation head `103bbcab82344d641015281909e64c43c46d1621`. Integration-type tests remained skipped under the standing owner instruction. The final completion-record head requires a fresh exact-head Actions result before merge.
