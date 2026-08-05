# Track 0 Parallel Execution

Track 0 industry streams may run concurrently. Each industry RWP remains sequential within its own queue.

For shared living records, agents must follow `SHARED_FILE_WRITE_PROTOCOL.md`:

- queue semantic shared-record updates locally;
- batch them into one completion checkpoint whenever practical;
- acquire only a short transactional write window;
- wait and retry when another agent is writing;
- refresh the latest default-branch files before applying queued changes;
- reconcile rather than overwrite concurrent updates;
- release the write window immediately after publication or abandonment;
- do not stop an RWP merely because a shared file is briefly busy.

A genuine blocker exists only after the bounded wait is exhausted, semantic changes conflict irreconcilably, or default-branch changes invalidate the work.
