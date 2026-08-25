# Local-model qualification final report

## Executive conclusion

The finite qualification is complete: eight official runs were recorded. `gpt-oss:20b` is the measured fast worker and primary developer under the fixed scoring and tie-break rules. No tested local model met the 80-point planner/reviewer threshold, so that role remains with a cloud model.

## Hardware and software

Ubuntu 24.04.4 LTS, kernel 7.0.0-30-generic; AMD Ryzen 7 9700X (8 cores/16 threads); 30 GiB RAM; Radeon AI PRO R9700 (gfx1201, 31.9 GiB VRAM); Ollama 0.32.15 using `rocm_v7_2`; OpenCode 1.18.21; Python 3.13.14; Git 2.43.0; context 65536. Full pre-download environment capture is in `environment/`.

## Exact models and results

| Model | Coding score / duration | Review score / duration | Result |
| --- | --- | --- | --- |
| qwen3.5:9b-q4_K_M | 95 / 150.010 s | 48 / 130.106 s | Strong coding; insufficient review coverage. |
| qwen3-coder:30b | 90 / 300.781 s | 0 / 10.319 s | 14/14 hidden coding checks; did not review fixture. |
| gpt-oss:20b | 95 / 125.855 s | 0 / 32.639 s | Fastest eligible coding score; did not review fixture. |
| devstral:24b | 60 / 12.984 s | 0 / 18.204 s | Stopped before task work; generic non-review. |

All completed inference samples that loaded a model show 100% GPU with context 65536; exact samples and loaded sizes are retained in each result folder. Devstral stopped before a usable allocation sample. The four model tags, digests, model sizes, and raw transcripts are preserved beside their results.

## Strengths, weaknesses, and controls

- qwen3.5 modified two production files, passed 13/14 hidden checks, and supplied the only substantive review, but missed the upper quantity bound.
- qwen3-coder passed every hidden coding check but was slow, failed the contradictory public case, and did not inspect the review patch.
- gpt-oss was the fastest high-scoring coder, but changed the implementation to satisfy the contradictory public case and therefore missed the upper-bound hidden check; it did not inspect the review patch.
- devstral did not complete either task.

One pre-official qwen3.5 launch was an infrastructure retry: the harness directed OpenCode to the Vennusign worktree before fixture work began. Its transcript is retained. No model-result retries or manual repairs occurred.

The visible public coding fixture contains a discovered contradiction: it asks for quantity 20 to produce insufficient inventory while the task requires quantity above 10 to be rejected. This is retained unchanged; the external checks make the divergence explicit. The historical simple-calculator evidence is hardware/performance context only and was not used to choose routing.

## Eligibility and Maestro routing

| Maestro role | Selection | Basis |
| --- | --- | --- |
| Fast worker | gpt-oss:20b | Eligible coding score (95), GPU execution, no prohibited edits, fastest eligible time. |
| Primary developer | gpt-oss:20b | Highest coding score tie (95) broken by faster time; passed replay and both concurrency checks. |
| Planner/reviewer | Cloud model | No local model reached review score 80 or identified all required identity findings. |

Retain locally: qwen3.5:9b-q4_K_M, qwen3-coder:30b, gpt-oss:20b, and devstral:24b until the owner elects otherwise. Not selected: qwen3.5, qwen3-coder, and devstral for active Maestro routing; devstral may be removed later without deleting it now. Limits: one task per category/model, fixture contradiction noted above, and the review package relied on the model locating `candidate.diff`.

**Qualification phase complete.**
