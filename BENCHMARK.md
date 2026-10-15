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
| Method                                  | ChunkSize | Mean          | Error        | StdDev       | Allocated |
|---------------------------------------- |---------- |--------------:|-------------:|-------------:|----------:|
| Compose_NewGrid                         | 32        |  32,973.35 ns | 3,778.205 ns | 3,534.135 ns |  140840 B |
| Attach_Aligned                          | 32        |   5,990.63 ns |   322.087 ns |   301.281 ns |         - |
| Attach_Unaligned                        | 32        |  15,405.91 ns |   542.249 ns |   452.803 ns |         - |
| Attach_StructExtractor_Unaligned        | 32        |  28,060.58 ns |   578.837 ns |   513.124 ns |         - |
| Attach_FuncExtractor_Unaligned          | 32        |  46,788.43 ns | 1,802.490 ns | 1,686.051 ns |         - |
| HasOverlap_Unaligned_NoOverlap          | 32        |   9,485.93 ns |   231.140 ns |   216.209 ns |         - |
| Detach_Aligned                          | 32        |     770.67 ns |    11.848 ns |    10.503 ns |         - |
| Detach_Unaligned                        | 32        |   9,692.55 ns |   133.930 ns |   125.278 ns |         - |
| Compact_NothingToRemove                 | 32        |     120.60 ns |     3.165 ns |     2.806 ns |         - |
| Indexer_Read                            | 32        | 179,072.20 ns | 5,416.306 ns | 4,801.414 ns |       1 B |
| Indexer_Write                           | 32        |  43,236.91 ns | 1,434.070 ns | 1,341.430 ns |         - |
| TryGet_Populated                        | 32        | 191,319.74 ns | 3,729.075 ns | 3,488.179 ns |       1 B |
| EnumerateChunks_MaskedVectorSum         | 32        |  63,446.78 ns | 1,545.785 ns | 1,445.928 ns |         - |
| EnumerateNeighbourhoods_VerticalStencil | 32        |  19,391.95 ns |   430.551 ns |   402.738 ns |         - |
| EnumerateNeighbourhoods_CopyColumns     | 32        |   6,574.42 ns |   124.850 ns |   116.785 ns |         - |
| Compose_NewGrid                         | 64        |  26,056.49 ns | 2,346.944 ns | 2,080.505 ns |  205312 B |
| Attach_Aligned                          | 64        |   4,712.07 ns |   143.632 ns |   134.354 ns |         - |
| Attach_Unaligned                        | 64        |   9,352.25 ns |   315.987 ns |   295.574 ns |         - |
| Attach_StructExtractor_Unaligned        | 64        |  21,538.63 ns |   506.927 ns |   474.179 ns |         - |
| Attach_FuncExtractor_Unaligned          | 64        |  40,038.28 ns |   774.762 ns |   724.712 ns |         - |
| HasOverlap_Unaligned_NoOverlap          | 64        |   3,142.43 ns |    48.817 ns |    43.275 ns |         - |
| Detach_Aligned                          | 64        |     250.25 ns |     3.188 ns |     2.662 ns |         - |
| Detach_Unaligned                        | 64        |   5,724.28 ns |   201.237 ns |   178.391 ns |         - |
| Compact_NothingToRemove                 | 64        |      57.37 ns |     2.023 ns |     1.892 ns |         - |
| Indexer_Read                            | 64        | 154,152.43 ns | 2,330.592 ns | 2,066.009 ns |       1 B |
| Indexer_Write                           | 64        |  40,857.02 ns |   703.108 ns |   657.688 ns |         - |
| TryGet_Populated                        | 64        | 178,238.85 ns | 3,244.430 ns | 2,876.102 ns |       1 B |
| EnumerateChunks_MaskedVectorSum         | 64        |  90,682.16 ns | 1,527.486 ns | 1,428.812 ns |       1 B |
| EnumerateNeighbourhoods_VerticalStencil | 64        |  19,532.13 ns |   947.225 ns |   839.690 ns |         - |
| EnumerateNeighbourhoods_CopyColumns     | 64        |   3,179.90 ns |   112.083 ns |   104.842 ns |         - |
| Compose_NewGrid                         | 128       |  20,533.39 ns |   863.418 ns |   807.642 ns |  271408 B |
| Attach_Aligned                          | 128       |   4,391.14 ns |   244.107 ns |   228.338 ns |         - |
| Attach_Unaligned                        | 128       |   6,445.47 ns |   114.711 ns |   101.688 ns |         - |
| Attach_StructExtractor_Unaligned        | 128       |  19,871.11 ns |   369.495 ns |   345.626 ns |         - |
| Attach_FuncExtractor_Unaligned          | 128       |  37,545.15 ns | 1,369.703 ns | 1,281.221 ns |         - |
| HasOverlap_Unaligned_NoOverlap          | 128       |   2,093.87 ns |    76.785 ns |    71.825 ns |         - |
| Detach_Aligned                          | 128       |     193.80 ns |     3.454 ns |     2.885 ns |         - |
| Detach_Unaligned                        | 128       |   3,383.39 ns |    49.380 ns |    46.190 ns |         - |
| Compact_NothingToRemove                 | 128       |      49.59 ns |     0.962 ns |     0.899 ns |         - |
| Indexer_Read                            | 128       | 120,719.06 ns | 2,243.911 ns | 2,098.955 ns |       1 B |
| Indexer_Write                           | 128       |  43,638.47 ns | 1,331.057 ns | 1,179.947 ns |         - |
| TryGet_Populated                        | 128       | 156,359.59 ns | 6,029.623 ns | 5,640.113 ns |       1 B |
| EnumerateChunks_MaskedVectorSum         | 128       | 152,059.80 ns | 6,126.096 ns | 5,730.354 ns |       1 B |
| EnumerateNeighbourhoods_VerticalStencil | 128       |  18,412.53 ns |   687.420 ns |   643.013 ns |         - |
| EnumerateNeighbourhoods_CopyColumns     | 128       |   1,235.47 ns |    42.429 ns |    39.689 ns |         - |

# DoubleBufferedChunkedGrid
| Method                 | ChunkSize | Mean      | Error     | StdDev    | Allocated |
|----------------------- |---------- |----------:|----------:|----------:|----------:|
| Attach_Aligned         | 32        |  5.179 us | 0.0853 us | 0.0756 us |         - |
| Attach_Unaligned       | 32        | 14.974 us | 0.1998 us | 0.1669 us |         - |
| CopyCurrentToAlternate | 32        |  3.167 us | 0.1174 us | 0.1098 us |         - |
| TrySet_Populated       | 32        | 47.858 us | 1.1224 us | 1.0499 us |         - |
| Step_VerticalDiffusion | 32        | 31.094 us | 1.4546 us | 1.3606 us |         - |
| Attach_Aligned         | 64        |  4.360 us | 0.1514 us | 0.1416 us |         - |
| Attach_Unaligned       | 64        |  9.591 us | 0.3920 us | 0.3667 us |         - |
| CopyCurrentToAlternate | 64        |  5.470 us | 0.2960 us | 0.2769 us |         - |
| TrySet_Populated       | 64        | 47.076 us | 1.4858 us | 1.3898 us |         - |
| Step_VerticalDiffusion | 64        | 29.945 us | 1.1901 us | 1.0550 us |         - |
| Attach_Aligned         | 128       |  4.173 us | 0.1535 us | 0.1436 us |         - |
| Attach_Unaligned       | 128       |  6.600 us | 0.2080 us | 0.1844 us |         - |
| CopyCurrentToAlternate | 128       |  7.276 us | 0.2372 us | 0.2103 us |         - |
| TrySet_Populated       | 128       | 47.089 us | 2.3159 us | 2.1663 us |         - |
| Step_VerticalDiffusion | 128       | 29.245 us | 0.4578 us | 0.4059 us |         - |
