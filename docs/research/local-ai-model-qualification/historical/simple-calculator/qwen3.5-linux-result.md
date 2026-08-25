# Linux R9700 benchmark result

## Controls

- Starting commit: `5c1db53c2d0d9708d32a98b81b9f7e086d072470`
- Reference solution: `44a69413852b8569f3d646d8b734291657b2ca86`
- Prompt copied exactly: yes
- Ollama version: `0.32.15`
- Model tag: `qwen3.5:9b-q4_K_M`
- Model digest, if available: `6488c96fa5fa` (Ollama model ID)
- Context length: `65536`
- OpenCode version: `1.18.21`
- Python version: `3.13.14`

## System

- OS: Ubuntu 24.04.4 LTS
- Kernel: `7.0.0-30-generic`
- CPU: AMD Ryzen 7 9700X (8 cores / 16 threads)
- GPU: Radeon AI PRO R9700 (`1002:7551`, Ollama `gfx1201`, 31.9 GiB VRAM)
- RAM: 30 GiB
- GPU acceleration evidence: `ollama ps` during and immediately after the run reported `PROCESSOR 100% GPU`, context `65536`; Ollama identified the discrete ROCm `gfx1201` device.

## Result

- Start time: `2026-08-24T22:49:34.210-04:00`
- Completion time: `2026-08-24T22:49:50.619-04:00`
- Elapsed seconds: `16.410`
- Reference elapsed seconds: `134`
- Comparison with reference: `117.590` seconds faster (`8.166×` faster; `87.75%` less elapsed time)
- Five tests passed: yes
- Only `calculator.py` changed: yes (tracked files; test execution also created untracked `__pycache__/`)
- Solution matched reference commit: no
- OpenCode output/transcript location: `results/opencode-task-001-linux-r9700-20260824T2247-0400.log`
- Notes: OpenCode completed successfully and the resulting implementation passes all five tests. The prescribed verification script exited `1` because it requires a byte-identical match to the reference; the generated expression was `price - (price * percent / 100)` instead of `price * (1 - percent / 100)`, and the `ValueError` message used `Percent` rather than `Percentage`. GPU discovery required the local Ollama runtime selector `rocm_v7_2` rather than `rocm`; this environment-only configuration correction was made before the timed run.
