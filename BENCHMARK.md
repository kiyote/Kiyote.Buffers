```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10
```

## ArrayBuffer
| Method                 | Mean       | Error     | StdDev    | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|
| Perform_SingleInputSet |   857.3 μs | 364.12 μs | 340.60 μs |         - |
| Perform_SingleInputAdd | 2,678.5 μs |  90.97 μs |  85.09 μs |      16 B |
| Perform_TwoInputSet    |   939.8 μs | 141.78 μs | 132.62 μs |       8 B |
| Perform_TwoInputAdd    | 3,094.0 μs |  45.25 μs |  40.11 μs |      16 B |

## BufferOperator
| Method                          | Mean       | Error     | StdDev    | Allocated |
|-------------------------------- |-----------:|----------:|----------:|----------:|
| Perform_OneInputBuffer          | 2,559.1 μs |  93.77 μs |  87.71 μs |      21 B |
| Perform_TwoIputBuffers          | 3,201.3 μs |  78.71 μs |  73.62 μs |      22 B |
| Perform_OneInputSourceReference | 1,184.8 μs |  82.44 μs |  77.11 μs |      11 B |
| Perform_OneInputTransformOutput |   888.5 μs | 169.96 μs | 158.98 μs |      11 B |

## NumericBuffer
| Method                 | Mean       | Error     | StdDev    | Allocated |
|----------------------- |-----------:|----------:|----------:|----------:|
| Perform_SingleInputSet |   610.9 μs |  27.14 μs |  25.38 μs |       6 B |
| Perform_SingleInputAdd | 3,004.5 μs |  97.73 μs |  91.41 μs |      22 B |
| Perform_TwoInputSet    |   999.0 μs | 107.45 μs | 100.51 μs |      11 B |
| Perform_TwoInputAdd    | 3,085.4 μs |  82.06 μs |  76.76 μs |      22 B |

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
