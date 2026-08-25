# Controlled local-model qualification plan

Models, in fixed order: `qwen3.5:9b-q4_K_M`, `qwen3-coder:30b`, `gpt-oss:20b`, and `devstral:24b`. Each receives one official multi-file coding run and one official code-review/planning run, for eight official runs total. The execution configuration is OpenCode 1.18.21 using the local Ollama provider, context length 65536, unchanged defaults and tool permissions, sequential execution, fresh worktrees, and no manual repair or model-specific prompting.

Infrastructure failures before meaningful generation may be retried once from a fresh worktree, with the original transcript retained. Model failures, early stops, test failures, and missed findings are not retryable.

Coding is scored from fourteen external checks (70), public tests (10), scope discipline (10), and autonomous completion (10). Review is scored from ten seeded findings (70), prioritization (10), remediation (10), and focused test planning (10). Performance is a tiebreaker only after eligibility. The final report applies the stated role thresholds for fast worker, primary developer, and planner/reviewer.
