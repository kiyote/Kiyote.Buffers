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
| Method            | Mean       | Error    | StdDev   | Allocated |
|------------------ |-----------:|---------:|---------:|----------:|
| Add_Float         |   335.4 us | 22.23 us | 20.79 us |         - |
| Add_Int           |   317.7 us | 60.44 us | 56.54 us |       2 B |
| AddTo_Float       |   698.0 us | 39.05 us | 36.53 us |       4 B |
| AddTo_Int         |   681.7 us | 29.07 us | 27.20 us |       4 B |
| Subtract_Float    |   259.5 us |  9.02 us |  7.04 us |       2 B |
| Subtract_Int      |   289.4 us | 35.91 us | 33.59 us |       2 B |
| SubtractTo_Float  |   832.7 us | 39.68 us | 37.11 us |       4 B |
| SubtractTo_Int    |   813.5 us | 36.46 us | 34.11 us |       4 B |
| Max_Float         |   704.5 us |  6.47 us |  5.41 us |       4 B |
| Max_Int           |   200.2 us | 14.58 us | 12.18 us |       2 B |
| Min_Float         |   705.3 us | 12.67 us | 10.58 us |       4 B |
| Min_Int           |   207.9 us | 20.92 us | 19.57 us |       1 B |
| MinMax_Float      |   931.7 us | 25.61 us | 23.96 us |       4 B |
| MinMax_Int        |   304.3 us | 11.95 us | 11.17 us |       2 B |
| Multiply_Float    |   260.8 us | 20.37 us | 19.06 us |       2 B |
| Multiply_Int      |   309.8 us | 38.53 us | 36.04 us |       2 B |
| MultiplyTo_Float  |   677.4 us | 21.73 us | 20.33 us |       4 B |
| MultiplyTo_Int    |   642.7 us | 17.05 us | 13.31 us |       4 B |
| Divide_Float      |   388.4 us | 67.34 us | 59.69 us |       1 B |
| Divide_Int        | 3,859.2 us | 43.73 us | 38.77 us |      31 B |
| DivideTo_Float    |   801.0 us | 26.73 us | 23.69 us |       4 B |
| DivideTo_Int      | 3,711.1 us | 26.79 us | 23.75 us |      16 B |
| Normalize_Float   |   815.8 us | 19.02 us | 15.89 us |       4 B |
| Normalize_Int     |   222.3 us |  8.92 us |  7.45 us |       1 B |
| NormalizeTo_Float | 1,503.2 us | 38.88 us | 32.47 us |       8 B |
| NormalizeTo_Int   |   943.6 us | 24.70 us | 20.63 us |       4 B |
