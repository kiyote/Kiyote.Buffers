# Performance gate (UPDATE_PLAN item 13)

Configuration: BenchmarkDotNet 0.15.8, MediumRun (15 iterations, 10 warmups, one launch), InProcessNoEmitToolchain, Release, .NET 10.0.13, Windows 11, Intel Core i7-9700K (8 physical cores). `-` in the allocation column means no managed allocation detected; 1–31 B measurements in otherwise allocation-free operations are sampling noise, not an assertion that each invocation allocates.

Baseline: commit `b32152c` (no production code has changed since). Current item adds benchmark coverage and runner registrations only. For production hot paths, compare against the committed baseline using the same runner, hardware, and input before accepting any future change; investigate consistent throughput slowdowns above 5% or an increase in allocated bytes. Across separate runs, host variability can exceed this threshold; repeat matched runs and report uncertainty rather than calling noise a regression.

| Scenario | Input | Mean | Allocated |
| --- | --- | ---: | ---: |
| Core IndexerRead | Flat / Ragged, 1000² | 2201 / 3663 us | ~0 B |
| Core RowSpanRead | Flat / Ragged, 1000² | 320 / 342 us | ~0 B |
| Analyzer IsSealed struct predicate | 100² / 1200² | 243 / 42465 us | ~0 / 333 B |
| Grid Attach_Aligned | chunks 32 / 64 / 128 | 4.111 / 5.718 / 5.367 us | 0 B |
| Grid Attach_Unaligned | chunks 32 / 64 / 128 | 16.979 / 10.409 / 6.151 us | 0 B |
| Grid vertical neighbourhood stencil | chunks 32 / 64 / 128 | 14.979 / 15.218 / 13.457 us | 0 B |
| Grid neighbourhood copy columns | chunks 32 / 64 / 128 | 4.926 / 2.521 / 1.051 us | 0 B |
| Grid Indexer_Read | chunks 32 / 64 / 128 | 171.98 / 160.95 / 138.22 us | ~0 B |
| Grid Indexer_Write | chunks 32 / 64 / 128 | 38.21 / 35.69 / 37.25 us | 0 B |
| Triple-buffer Swap_NoLease / Acquire_Dispose / Swap_WhileLeased | 64-cell chunks | 4.426 / 11.603 / 16.767 ns | 0 B |
| Numeric Create | Flat / Ragged, 1000² floats, initialized to 1 | 827 / 429 us | 4,001,377 / 4,032,066 B |
| Numeric AddToDestination | Flat / Ragged, 1000² floats | 607 / 202 us | ~0 B |
| Numeric Max | Flat / Ragged, 1000² floats | 482 / 327 us | ~0 B |
| Numeric NormalizeToDestination | Flat / Ragged, 1000² floats | 3096 / 4827 us | ~0 B |

The flat numeric constructor already allocates exactly one padded value array after initializing `_allocWidth`; the unused allocation described in the original review plan is not present. Flat storage uses fewer bytes but is slower to initialize with nonzero values on this host; Ragged vectorization outperforms Flat for Add and Max but not Normalize. No storage-layout or vectorization change is justified solely by these mixed measurements. In-process measurements share the host with the IDE; use multiple paired runs for release decisions.
