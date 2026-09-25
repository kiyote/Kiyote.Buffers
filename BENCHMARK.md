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
