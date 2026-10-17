```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10
```

## RaggedArrayBuffer
| Method                 | Mean       | Error     | StdDev    | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|
| Perform_SingleInputSet |   644.6 us | 212.02 us | 198.32 us |         - |
| Perform_SingleInputAdd | 2,689.2 us | 122.48 us | 114.57 us |      12 B |
| Perform_TwoInputSet    |   868.9 us | 123.68 us | 115.69 us |       6 B |
| Perform_TwoInputAdd    | 3,113.7 us |  33.45 us |  29.65 us |      12 B |

## FlatArrayBuffer
| Method                 | Mean     | Error     | StdDev    | Allocated |
|----------------------- |---------:|----------:|----------:|----------:|
| Perform_SingleInputSet | 2.559 ms | 0.0284 ms | 0.0265 ms |      18 B |
| Perform_SingleInputAdd | 2.596 ms | 0.0600 ms | 0.0561 ms |      18 B |
| Perform_TwoInputSet    | 3.110 ms | 0.0597 ms | 0.0558 ms |      18 B |
| Perform_TwoInputAdd    | 3.097 ms | 0.0566 ms | 0.0501 ms |      17 B |

## BufferOperator
| Method                          | Mean       | Error     | StdDev    | Allocated |
|-------------------------------- |-----------:|----------:|----------:|----------:|
| Perform_OneInputBuffer          | 2,559.1 μs |  93.77 μs |  87.71 μs |      21 B |
| Perform_TwoIputBuffers          | 3,201.3 μs |  78.71 μs |  73.62 μs |      22 B |
| Perform_OneInputSourceReference | 1,184.8 μs |  82.44 μs |  77.11 μs |      11 B |
| Perform_OneInputTransformOutput |   888.5 μs | 169.96 μs | 158.98 μs |      11 B |

## RaggedNumericBuffer
| Method                 | Mean       | Error     | StdDev   | Allocated |
|----------------------- |-----------:|----------:|---------:|----------:|
| Perform_SingleInputSet |   561.9 us |  27.99 us | 24.81 us |         - |
| Perform_SingleInputAdd | 2,980.1 us |   6.46 us |  5.04 us |      12 B |
| Perform_TwoInputSet    |   847.2 us | 101.26 us | 94.72 us |       6 B |
| Perform_TwoInputAdd    | 2,641.5 us |  27.86 us | 24.70 us |      12 B |

## FlatNumericBuffer
| Method                 | Mean     | Error     | StdDev    | Allocated |
|----------------------- |---------:|----------:|----------:|----------:|
| Perform_SingleInputSet | 3.012 ms | 0.0200 ms | 0.0187 ms |      18 B |
| Perform_SingleInputAdd | 3.216 ms | 0.0924 ms | 0.0864 ms |      17 B |
| Perform_TwoInputSet    | 2.987 ms | 0.0766 ms | 0.0716 ms |      17 B |
| Perform_TwoInputAdd    | 2.848 ms | 0.0252 ms | 0.0224 ms |      18 B |

## NumericBufferOperator
| Method                       | Mean       | Error     | StdDev   | Allocated |
|----------------------------- |-----------:|----------:|---------:|----------:|
| Add_Float                    |   274.4 us |  24.66 us | 23.07 us |         - |
| Add_Int                      |   246.8 us |   7.90 us |  7.00 us |       2 B |
| Add_FloatToDestination       |   661.2 us |  12.83 us | 12.00 us |       4 B |
| Add_IntToDestination         |   639.6 us |  12.80 us | 11.35 us |       4 B |
| Subtract_Float               |   371.3 us |  23.08 us | 20.46 us |       2 B |
| Subtract_Int                 |   365.1 us |  46.59 us | 43.58 us |       2 B |
| Subtract_FloatToDestination  |   675.0 us |  28.28 us | 23.62 us |       4 B |
| Subtract_IntToDestination    |   664.5 us |  28.86 us | 27.00 us |       4 B |
| Max_Float                    |   697.8 us |   9.66 us |  8.57 us |       4 B |
| Max_Int                      |   187.4 us |   6.67 us |  6.24 us |       2 B |
| Min_Float                    |   726.6 us |  32.83 us | 29.10 us |       4 B |
| Min_Int                      |   267.4 us |  28.12 us | 24.93 us |       2 B |
| MinMax_Float                 |   910.0 us |  63.24 us | 59.15 us |       4 B |
| MinMax_Int                   |   226.5 us |  16.68 us | 13.93 us |       2 B |
| Multiply_Float               |   258.0 us |  12.97 us | 11.50 us |       2 B |
| Multiply_Int                 |   270.0 us |   7.64 us |  5.97 us |       2 B |
| Multiply_FloatToDestination  |   663.2 us |  17.80 us | 14.87 us |       4 B |
| Multiply_IntToDestination    |   632.2 us |  24.30 us | 18.97 us |       4 B |
| Divide_Float                 |   424.1 us |  32.55 us | 27.18 us |       2 B |
| Divide_Int                   | 3,632.3 us |  67.92 us | 63.53 us |      16 B |
| Divide_FloatToDestination    |   676.0 us |  36.65 us | 34.28 us |       4 B |
| Divide_IntToDestination      | 3,608.1 us |  16.02 us | 14.20 us |      16 B |
| Normalize_Float              |   794.4 us |  15.09 us | 13.38 us |       4 B |
| Normalize_Int                |   333.4 us |  61.24 us | 57.29 us |       1 B |
| Normalize_FloatToDestination | 1,437.1 us | 101.91 us | 95.32 us |       7 B |
| Normalize_IntToDestination   |   825.2 us |  52.85 us | 49.44 us |       4 B |
| ScaleToRange_Float           | 1,298.6 us |  39.07 us | 34.64 us |       8 B |
| ScaleToRange_Int             |   859.4 us |  61.51 us | 54.52 us |       4 B |


# ChunkedGrid
| Method                                  | ChunkSize | Mean       | Error     | StdDev    | Allocated |
|---------------------------------------- |---------- |-----------:|----------:|----------:|----------:|
| Compose_NewGrid                         | 32        |  39.291 μs | 1.0642 μs | 0.9954 μs |  147776 B |
| Attach_Aligned                          | 32        |   4.628 μs | 0.1660 μs | 0.1553 μs |         - |
| Attach_Unaligned                        | 32        |  16.839 μs | 0.2782 μs | 0.2602 μs |         - |
| Attach_StructExtractor_Unaligned        | 32        |  28.258 μs | 0.2695 μs | 0.2521 μs |         - |
| Attach_FuncExtractor_Unaligned          | 32        |  34.466 μs | 1.0580 μs | 0.9896 μs |         - |
| HasOverlap_Unaligned_NoOverlap          | 32        |  10.703 μs | 0.2980 μs | 0.2488 μs |         - |
| Detach_Unaligned                        | 32        |  10.214 μs | 0.3359 μs | 0.3142 μs |         - |
| Indexer_Read                            | 32        | 193.597 μs | 6.0313 μs | 5.6417 μs |       1 B |
| Indexer_Write                           | 32        |  41.252 μs | 0.6620 μs | 0.5169 μs |         - |
| TryGet_Populated                        | 32        | 179.769 μs | 2.1864 μs | 2.0452 μs |       1 B |
| EnumerateChunks_MaskedVectorSum         | 32        |  54.761 μs | 0.8162 μs | 0.7635 μs |         - |
| EnumerateNeighbourhoods_VerticalStencil | 32        |  16.576 μs | 0.5806 μs | 0.5431 μs |         - |
| EnumerateNeighbourhoods_CopyColumns     | 32        |   5.033 μs | 0.1414 μs | 0.1322 μs |         - |
| Compose_NewGrid                         | 64        |  35.716 μs | 2.5294 μs | 2.3660 μs |  208736 B |
| Attach_Aligned                          | 64        |   4.167 μs | 0.1421 μs | 0.1329 μs |         - |
| Attach_Unaligned                        | 64        |  10.410 μs | 0.0902 μs | 0.0753 μs |         - |
| Attach_StructExtractor_Unaligned        | 64        |  21.854 μs | 0.5083 μs | 0.4755 μs |         - |
| Attach_FuncExtractor_Unaligned          | 64        |  28.208 μs | 0.8368 μs | 0.7828 μs |         - |
| HasOverlap_Unaligned_NoOverlap          | 64        |   3.459 μs | 0.0987 μs | 0.0923 μs |         - |
| Detach_Unaligned                        | 64        |   5.546 μs | 0.1790 μs | 0.1674 μs |         - |
| Indexer_Read                            | 64        | 171.591 μs | 4.6587 μs | 4.3577 μs |       1 B |
| Indexer_Write                           | 64        |  38.516 μs | 0.8339 μs | 0.7800 μs |         - |
| TryGet_Populated                        | 64        | 159.457 μs | 3.1720 μs | 2.9671 μs |       1 B |
| EnumerateChunks_MaskedVectorSum         | 64        |  80.744 μs | 2.1576 μs | 2.0182 μs |       1 B |
| EnumerateNeighbourhoods_VerticalStencil | 64        |  15.146 μs | 0.8865 μs | 0.8292 μs |         - |
| EnumerateNeighbourhoods_CopyColumns     | 64        |   2.307 μs | 0.0659 μs | 0.0617 μs |         - |
| Compose_NewGrid                         | 128       |  24.871 μs | 1.2040 μs | 1.1262 μs |  272288 B |
| Attach_Aligned                          | 128       |   4.028 μs | 0.2470 μs | 0.2310 μs |         - |
| Attach_Unaligned                        | 128       |   6.910 μs | 0.2011 μs | 0.1881 μs |         - |
| Attach_StructExtractor_Unaligned        | 128       |  19.147 μs | 0.3099 μs | 0.2899 μs |         - |
| Attach_FuncExtractor_Unaligned          | 128       |  26.000 μs | 1.3808 μs | 1.2916 μs |         - |
| HasOverlap_Unaligned_NoOverlap          | 128       |   2.308 μs | 0.0576 μs | 0.0538 μs |         - |
| Detach_Unaligned                        | 128       |   3.326 μs | 0.0690 μs | 0.0645 μs |         - |
| Indexer_Read                            | 128       | 136.725 μs | 3.2084 μs | 3.0011 μs |       1 B |
| Indexer_Write                           | 128       |  39.149 μs | 1.1908 μs | 0.9944 μs |         - |
| TryGet_Populated                        | 128       | 120.966 μs | 2.3481 μs | 2.1964 μs |       1 B |
| EnumerateChunks_MaskedVectorSum         | 128       | 112.916 μs | 3.6137 μs | 3.3803 μs |       1 B |
| EnumerateNeighbourhoods_VerticalStencil | 128       |  16.334 μs | 0.5858 μs | 0.5480 μs |         - |
| EnumerateNeighbourhoods_CopyColumns     | 128       |   1.000 μs | 0.0320 μs | 0.0300 μs |         - |

# DoubleBufferedChunkedGrid
| Method                 | ChunkSize | Mean      | Error     | StdDev    | Allocated |
|----------------------- |---------- |----------:|----------:|----------:|----------:|
| Attach_Aligned         | 32        |  4.802 μs | 0.0386 μs | 0.0343 μs |         - |
| Attach_Unaligned       | 32        | 17.202 μs | 0.4467 μs | 0.4179 μs |         - |
| CopyCurrentToAlternate | 32        |  3.588 μs | 0.1349 μs | 0.1262 μs |         - |
| TrySet_Populated       | 32        | 39.186 μs | 0.6356 μs | 0.5945 μs |         - |
| Step_VerticalDiffusion | 32        | 24.549 μs | 0.2830 μs | 0.2508 μs |         - |
| Attach_Aligned         | 64        |  4.252 μs | 0.0566 μs | 0.0502 μs |         - |
| Attach_Unaligned       | 64        | 10.891 μs | 0.4198 μs | 0.3927 μs |         - |
| CopyCurrentToAlternate | 64        |  5.637 μs | 0.2102 μs | 0.1966 μs |         - |
| TrySet_Populated       | 64        | 38.551 μs | 0.4347 μs | 0.3854 μs |         - |
| Step_VerticalDiffusion | 64        | 24.711 μs | 0.4944 μs | 0.4625 μs |         - |
| Attach_Aligned         | 128       |  3.874 μs | 0.1619 μs | 0.1514 μs |         - |
| Attach_Unaligned       | 128       |  6.938 μs | 0.2889 μs | 0.2702 μs |         - |
| CopyCurrentToAlternate | 128       |  7.663 μs | 0.4079 μs | 0.3815 μs |         - |
| TrySet_Populated       | 128       | 35.297 μs | 1.0733 μs | 1.0040 μs |         - |
| Step_VerticalDiffusion | 128       | 24.413 μs | 0.7100 μs | 0.5928 μs |         - |

# ChunkedGridLayer
A fully populated 256x256 `ChunkedGridLayout` with one `float` layer. The stencils compute a 5-point average per
cell, reading neighbours either cell-by-cell through `GridChunkNeighbourhood.TryGetValue` or from a
`CopyPadded( 1, scratch )` block at fixed offsets.

| Method                       | ChunkSize | Mean         | Error      | StdDev     | Allocated |
|----------------------------- |---------- |-------------:|-----------:|-----------:|----------:|
| ElementWise_GetValues        | 32        |     8.007 μs |  0.3356 μs |  0.3139 μs |         - |
| Stencil_TryGetValue          | 32        | 1,098.672 μs | 23.6550 μs | 22.1269 μs |      13 B |
| Stencil_CopyPadded           | 32        |   175.672 μs |  4.3646 μs |  4.0826 μs |       2 B |
| ReadPresenceBits_VacancyMask | 32        |    30.489 μs |  1.4358 μs |  1.3431 μs |         - |
| ElementWise_GetValues        | 64        |     8.179 μs |  0.4246 μs |  0.3972 μs |         - |
| Stencil_TryGetValue          | 64        | 1,069.632 μs | 50.7525 μs | 47.4739 μs |      13 B |
| Stencil_CopyPadded           | 64        |   151.746 μs |  8.2445 μs |  7.7119 μs |       2 B |
| ReadPresenceBits_VacancyMask | 64        |    15.830 μs |  0.5284 μs |  0.4942 μs |         - |
