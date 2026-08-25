# qwen3-coder:30b — coding result

One official attempt, fixture `9cffcdc6830e2590b3d78fba6aea80433a62fca6`, model digest `06c1097efce0431c2045fe7b2e5108366e43bee1b4603a7aded8f21689e90bca`.

- Duration: 300,780,826,839 ns; exit 0; GPU samples show 100% GPU at context 65536.
- Hidden checks: 14/14 passed.
- Public suite: failed 1/4 because its visible insufficient-inventory case used quantity 20, which contradicts the fixed requirement that values above 10 raise `InvalidHoldRequest` first. This is a fixture defect, preserved without repair or rerun.
- Scope: only `venue_hold/service.py` changed; no prohibited files changed.
- Score: 90/100 (70 hidden + 0 public due to the contradictory suite + 10 scope + 10 autonomous completion).
