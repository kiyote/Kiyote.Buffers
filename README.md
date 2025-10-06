![CI](https://github.com/kiyote/Kiyote.Buffers/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://github.com/kiyote/Kiyote.Buffers/blob/badges/.badges/main/coverage.svg?raw=true)

# Kiyote.Buffers



# Performance

## BufferOperator
| Method                          | Mean     | Error     | StdDev    | Allocated |
|-------------------------------- |---------:|----------:|----------:|----------:|
| Perform_OneInputBuffer          | 1.499 ms | 0.0434 ms | 0.0406 ms |         - |
| Perform_TwoIputBuffers          | 2.004 ms | 0.0728 ms | 0.0646 ms |       8 B |
| Perform_OneInputSourceReference | 1.381 ms | 0.0670 ms | 0.0560 ms |       4 B |
| Perform_OneInputTransformOutput | 1.494 ms | 0.0603 ms | 0.0564 ms |       4 B |


## NumericBufferOperator
| Method          | Mean       | Error     | StdDev   | Allocated |
|---------------- |-----------:|----------:|---------:|----------:|
| Add_Float       |   342.5 us |  32.17 us | 28.52 us |         - |
| Add_Int         |   337.1 us |  30.30 us | 28.34 us |       1 B |
| Subtract_Float  |   306.8 us |  10.96 us |  8.56 us |       1 B |
| Subtract_Int    |   361.4 us |  30.04 us | 25.08 us |       1 B |
| Max_Float       |   766.1 us |  31.99 us | 29.92 us |       3 B |
| Max_Int         |   321.0 us | 102.33 us | 90.71 us |       1 B |
| Min_Float       |   830.6 us |  28.95 us | 24.17 us |       6 B |
| Min_Int         |   328.1 us |  29.26 us | 24.44 us |       1 B |
| MinMax_Float    |   920.6 us |  34.32 us | 30.43 us |       6 B |
| MinMax_Int      |   320.1 us |  21.38 us | 17.85 us |       1 B |
| Multiply_Float  |   413.6 us |  42.41 us | 37.59 us |       1 B |
| Multiply_Int    |   502.0 us |  51.38 us | 45.55 us |       1 B |
| Divide_Float    |   449.5 us |  57.37 us | 53.66 us |       1 B |
| Divide_Int      | 3,761.6 us |  78.57 us | 73.49 us |      12 B |
| Normalize_Float |   864.6 us |  21.32 us | 17.80 us |         - |
| Normalize_Int   |   270.3 us |  11.26 us | 10.53 us |       1 B |
