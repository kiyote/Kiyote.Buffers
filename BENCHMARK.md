```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10
```

## BufferOperator
| Method                          | Mean     | Error     | StdDev    | Allocated |
|-------------------------------- |---------:|----------:|----------:|----------:|
| Perform_OneInputBuffer          | 1.320 ms | 0.0374 ms | 0.0292 ms |         - |
| Perform_TwoIputBuffers          | 1.892 ms | 0.0699 ms | 0.0654 ms |       4 B |
| Perform_OneInputSourceReference | 1.295 ms | 0.0447 ms | 0.0418 ms |       4 B |
| Perform_OneInputTransformOutput | 1.306 ms | 0.0715 ms | 0.0669 ms |       4 B |

## NumericBufferOperator
| Method          | Mean       | Error     | StdDev   | Allocated |
|---------------- |-----------:|----------:|---------:|----------:|
| Add_Float       |   309.0 us |  58.06 us | 54.31 us |         - |
| Add_Int         |   266.6 us |  11.40 us | 10.11 us |       1 B |
| Subtract_Float  |   289.6 us |  39.10 us | 34.66 us |       1 B |
| Subtract_Int    |   278.9 us |   6.53 us |  5.45 us |       1 B |
| Max_Float       |   717.2 us |  33.26 us | 29.48 us |       3 B |
| Max_Int         |   286.1 us |  19.40 us | 16.20 us |       1 B |
| Min_Float       |   755.3 us |  44.42 us | 41.55 us |       3 B |
| Min_Int         |   237.2 us |  37.84 us | 35.39 us |       1 B |
| MinMax_Float    |   792.9 us |  11.94 us |  9.33 us |       3 B |
| MinMax_Int      |   229.9 us |  10.45 us |  8.73 us |       1 B |
| Multiply_Float  |   255.8 us |   9.93 us |  9.29 us |       1 B |
| Multiply_Int    |   296.6 us |  20.00 us | 15.61 us |       1 B |
| Divide_Float    |   410.3 us |  17.62 us | 14.71 us |       3 B |
| Divide_Int      | 3,705.4 us | 119.68 us | 99.94 us |      12 B |
| Normalize_Float |   806.1 us |  24.49 us | 22.90 us |       3 B |
| Normalize_Int   |   266.5 us |  39.49 us | 36.94 us |       1 B |

