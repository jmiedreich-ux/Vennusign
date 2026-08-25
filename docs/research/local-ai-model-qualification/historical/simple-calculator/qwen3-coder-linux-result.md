# qwen3-coder:30b Linux R9700 controlled comparison

## Comparison controls

- Task worktree: `../LocalAgentTest-task-001-qwen3-coder-30b-run`
- Starting commit: `5c1db53c2d0d9708d32a98b81b9f7e086d072470`
- Task/prompt/fixtures/tests/scoring: unchanged from the committed TASK-001 control package
- Prompt: copied exactly from `benchmark/PROMPT.txt`
- OpenCode invocation: same `opencode run --auto` workflow as the completed Linux qwen3.5 run
- Context length: `65536`
- Timed attempts: one
- OOM: no; therefore no 32,768-token supplemental run was performed
- Existing qwen3.5 baseline artifacts: unchanged (pre-run SHA-256 values recorded in the control transcript)

## Software and model

- Ollama model name: `qwen3-coder:30b`
- Ollama model digest: `06c1097efce0431c2045fe7b2e5108366e43bee1b4603a7aded8f21689e90bca`
- Model architecture: `qwen3moe` (30.5B parameters; 30B-A3B Instruct)
- Quantization: `Q4_K_M`
- Download/model size: `18 GB` shown by `ollama list` (`18,556,700,761` bytes from the Ollama model API)
- Ollama version: `0.32.15`
- OpenCode version: `1.18.21`
- Python version: `3.13.14`

## System and acceleration

- OS: Ubuntu 24.04.4 LTS
- Kernel: `7.0.0-30-generic`
- CPU: AMD Ryzen 7 9700X (8 cores / 16 threads)
- RAM: 30 GiB
- GPU: Radeon AI PRO R9700 (`1002:7551`; ROCm `gfx1201`; 31.9 GiB VRAM)
- Ollama runtime: `rocm_v7_2`, the same functional GPU selector used for the completed Linux qwen3.5 run
- GPU allocation evidence: sampled `ollama ps` during the run reported `qwen3-coder:30b`, `25 GB`, `100% GPU`, context `65536`. No CPU allocation was reported.

## Timed result

- Start time: `2026-08-24T23:38:11.841-04:00`
- Completion time: `2026-08-24T23:38:30.590-04:00`
- Wall-clock execution time: `18.750` seconds (`18,749,689,634` ns)
- Pass/fail: pass (the agent completed successfully; all unit tests passed)
- Unit-test result: 5/5 passed; `python -m unittest -v` completed in `0.000s`
- Files changed: tracked: `calculator.py` only; test execution also created untracked `__pycache__/`
- Tokens / throughput: unavailable; this OpenCode/Ollama invocation did not emit token counts or generation throughput.

## Relative performance

| Baseline | Time | Difference from qwen3-coder:30b |
| --- | ---: | ---: |
| Linux qwen3.5:9b | 16.410 s | qwen3-coder was 2.340 s slower (14.26%) |
| Windows original | 134.000 s | qwen3-coder was 115.250 s faster (86.01% less time; 7.147x faster) |

## Quality and verification

- Behavioral result: the generated code correctly calculates a percentage discount and validates the two out-of-range conditions; all supplied tests pass.
- Difference from the original reference: the model split the range validation into two `ValueError` messages and used an explicit `discount_amount` intermediate. The behavior is equivalent for the supplied tests, but the code is not byte-identical.
- Prescribed `benchmark/verify-result.sh` outcome: exit `1`, solely because it requires the exact reference implementation text. It confirmed the task and tests were retained before rejecting the non-identical implementation.
- Deviations/problems: none affecting the one-for-one task run. The only environment action was registering the new Ollama model identifier with the existing local OpenCode provider so OpenCode could select it; all task-facing controls were retained. No repair or rerun was performed.

## Artifacts

- OpenCode transcript: `results/opencode-qwen3-coder-30b-linux-r9700-20260824T2337-0400.log`
- GPU allocation samples: `results/ollama-ps-qwen3-coder-30b-linux-r9700-20260824T2337-0400.log`
- Verification output: `results/verify-qwen3-coder-30b-linux-r9700.log`
