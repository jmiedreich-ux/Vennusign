# qwen3.5:9b-q4_K_M — coding result

Official attempt: one. An earlier launch was a documented harness failure before task-code work and is retained separately; this is the permitted infrastructure retry.

- Fixture commit: `9cffcdc6830e2590b3d78fba6aea80433a62fca6`
- Model digest: `6488c96fa5fa`
- Start/finish: 2026-08-25T00:18:25.154901028-04:00 / 2026-08-25T00:20:55.166308081-04:00
- Wall-clock duration: 150,010,362,503 ns
- Exit: 0; GPU sampling recorded 100% GPU, context 65536.
- Public tests: 4/4 passed.
- Hidden checks: 13/14 passed. The quantity-above-10 check failed.
- Scope: only `venue_hold/repository.py` and `venue_hold/service.py` changed; no prohibited files changed.
- Score: 95/100 (65 hidden + 10 public + 10 scope + 10 autonomous completion).

The generated diff, exact resulting production files, transcript, tests, GPU samples, and raw hidden-verifier JSON accompany this report.
