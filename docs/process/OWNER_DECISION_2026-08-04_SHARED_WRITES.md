# Owner Decision — Parallel Shared Writes

Date: 2026-08-04

The owner approved short-lived transactional write windows for shared Track 0 records. Whole-RWP claims must not reserve shared living-record files for the duration of the RWP.

Agents must queue semantic changes, wait for brief write availability, refresh current default-branch content, reconcile all queued changes in one write checkpoint whenever practical, publish, verify, and release immediately. Brief contention is not a stop condition.

This decision governs the restarted Track 0 industry schedules.
