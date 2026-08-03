# Vennu Session Handoff

## Work Package
- ID: Issue-389
- Status: In Review
- Execution mode: Collaborative

## Git State
- Branch: `issue/389-super-admin-key-helper`
- Issue: #389
- Pull request: #390
- CI state: pending

## Completed
- Added `scripts/set-super-admin-key.ps1`.
- It generates a random 256-bit key, stores `SuperAdmin__ApiKey` in the current Windows user environment, and copies the value to the clipboard.
- It prints guidance only and never displays or commits the generated key.
- Parser validation and local execution passed; the generated value is currently on the clipboard.

## Exact Next Action
- Close and reopen Vennu Development Control, restart API, open `http://localhost:5173`, and paste the clipboard value.

## Do Not Redo or Reverse
- Do not print, commit, or archive the generated key.
- Do not replace the random generator with a fixed development credential.
