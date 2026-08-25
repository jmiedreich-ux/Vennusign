# Immutable fixture commits

- Coding fixture repository: `c2aeba2eeacafbabce54c7582c62dab10a2e3353`
- Review fixture repository: `e178f44`

The coding worktrees and review worktrees are created from these commits. The external hidden verifier is stored in `controls/hidden_verify.py`, outside every model worktree.

## Infrastructure correction before official attempt

The first launch was directed at the Vennusign worktree instead of the fixture and terminated before editing task code. It is preserved in `results/qwen3.5-9b/coding-infrastructure-failure-*` and is not an official attempt. The initial fixture also lacked `tests/__init__.py`, so `python -m unittest -v` discovered zero tests. The immutable corrected fixture used for all official coding attempts is `9cffcdc6830e2590b3d78fba6aea80433a62fca6`.
