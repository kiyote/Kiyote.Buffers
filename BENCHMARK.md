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
| Method          | Mean       | Error    | StdDev   | Allocated |
|---------------- |-----------:|---------:|---------:|----------:|
| Add_Float       |   296.3 μs | 26.30 μs | 21.96 μs |       3 B |
| Add_Int         |   431.2 μs | 90.88 μs | 80.56 μs |       3 B |
| Subtract_Float  |   460.5 μs | 53.24 μs | 47.19 μs |       6 B |
| Subtract_Int    |   382.9 μs | 83.17 μs | 77.80 μs |       5 B |
| Max_Float       |   764.5 μs | 37.21 μs | 34.80 μs |       6 B |
| Max_Int         |   209.6 μs | 26.37 μs | 24.67 μs |       3 B |
| Min_Float       |   693.4 μs | 18.41 μs | 15.38 μs |       6 B |
| Min_Int         |   236.8 μs | 22.05 μs | 20.62 μs |       3 B |
| MinMax_Float    |   877.0 μs | 75.37 μs | 66.81 μs |       6 B |
| MinMax_Int      |   307.8 μs | 31.56 μs | 27.98 μs |       3 B |
| Multiply_Float  |   276.4 μs | 39.59 μs | 37.04 μs |       3 B |
| Multiply_Int    |   278.9 μs | 17.61 μs | 14.71 μs |       3 B |
| Divide_Float    |   386.7 μs | 38.01 μs | 31.74 μs |       3 B |
| Divide_Int      | 3,660.4 μs | 86.94 μs | 77.07 μs |      22 B |
| Normalize_Float |   846.5 μs | 80.81 μs | 71.63 μs |       6 B |
| Normalize_Int   |   315.2 μs | 33.51 μs | 31.35 μs |       2 B |

## BufferAnalyzer
| Method                       | Mean        | Error     | StdDev    | Allocated |
|----------------------------- |------------:|----------:|----------:|----------:|
| IsSealed_100x100_Delegate    |    141.7 us |   1.34 us |   1.19 us |         - |
| IsSealed_1000x1000_Delegate  | 16,552.2 us | 105.80 us |  88.35 us |     114 B |
| IsSealed_100x100_Predicate   |    129.5 us |   0.92 us |   0.82 us |       1 B |
| IsSealed_1000x1000_Predicate | 14,052.1 us | 266.46 us | 249.24 us |      57 B |


| Method                                 | Mean        | Error       | StdDev    | Gen0      | Gen1      | Gen2      | Allocated  |
|--------------------------------------- |------------:|------------:|----------:|----------:|----------:|----------:|-----------:|
| IsSealed_100x100_Delegate              |    179.0 us |     4.97 us |   4.65 us |         - |         - |         - |          - |
| IsSealed_1000x1000_Delegate            | 18,720.1 us |   307.23 us | 287.38 us |         - |         - |         - |      124 B |
| IsSealed_100x100_Predicate             |    184.5 us |     2.84 us |   2.66 us |         - |         - |         - |        1 B |
| IsSealed_1000x1000_Predicate           | 20,267.5 us |   382.77 us | 358.04 us |         - |         - |         - |      124 B |
| IsSealed_1200x1200_Predicate           | 29,815.6 us | 1,008.08 us | 893.63 us |         - |         - |         - |      124 B |
| TryGetSealedArea_1000x1000_Predicate   | 27,165.0 us |   619.14 us | 579.14 us | 1937.5000 | 1937.5000 | 1937.5000 | 25174108 B |
| TryVisitSealedArea_1000x1000_Predicate | 18,743.3 us |   109.33 us |  96.92 us |         - |         - |         - |      124 B |
