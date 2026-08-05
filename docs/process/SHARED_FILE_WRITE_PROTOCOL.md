# Shared-File Write Protocol

This protocol allows independent agents to work in parallel while preserving safe updates to shared repository records.

## Scope

The following files use short-lived transactional write access rather than whole-RWP ownership:

- `tracker/assignments.json`
- `PROJECT_STATUS.md`
- `ai/handoffs/current.md`
- `track0/CAPABILITY_MATRIX.md`

An RWP claim does not reserve these files for the full duration of the work.

## Queue Changes Locally

During research, drafting, validation, and industry-specific editing, each agent keeps a semantic pending-update queue for any required shared-record changes. The queue records intent, not stale full-file copies. Examples include marking an RWP complete, changing the exact next item, adding a capability delta, releasing a claim, or updating the current handoff.

Agents should consolidate all changes needed for the same completion checkpoint and perform one shared-record synchronization whenever practical.

## Transactional Write Window

Only when the shared changes are ready to publish may the agent enter a write window:

1. Request access only to the shared files that must change.
2. If another agent is writing any requested file, wait and retry instead of abandoning the RWP.
3. Use bounded backoff, normally 15 seconds, 30 seconds, then 60 seconds, continuing for up to 10 minutes before reporting a genuine lock problem.
4. Once access is available, refresh the latest versions from the default branch.
5. Reconcile the semantic pending-update queue onto the latest content. Never overwrite another agent's merged update with a stale full-file copy.
6. Apply the queued shared changes together, validate them, commit or publish them, and verify the resulting state.
7. Release access immediately after the write succeeds or is abandoned.

The write window covers only refresh, reconciliation, write, validation, publication, verification, and release. It must not include research, drafting, implementation, CI waiting, PR review, or unrelated work.

## Conflict and Recovery

Normal concurrent progress is not an ownership conflict. If a shared file changes before the agent writes, the agent refreshes and reapplies its semantic changes.

Stop only when:

- two agents attempt incompatible semantic changes that cannot be reconciled safely;
- the write window remains unavailable beyond the bounded wait period;
- the latest default-branch state invalidates the RWP assumptions; or
- another genuine repository or acceptance blocker exists.

A stale or abandoned write marker may be reclaimed after its bounded expiry when no active publication is occurring. Agents must never hold shared-file access while waiting for GitHub Actions or user input.

## Completion

Industry-specific work may proceed concurrently. Each RWP remains exclusively claimed, but shared records serialize only for their short final write windows. An agent that is waiting for a shared write window may continue safe non-conflicting preparation, but it must not begin a later dependent RWP until the current RWP is merged, verified, and released.
