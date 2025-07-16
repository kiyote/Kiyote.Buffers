![CI](https://github.com/kiyote/Kiyote.Buffers/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://github.com/kiyote/Kiyote.Buffers/blob/badges/.badges/main/coverage.svg?raw=true)

# Kiyote.Buffers



# Performance

## BufferOperators
| Method                          | Mean     | Error     | StdDev    | Allocated |
|-------------------------------- |---------:|----------:|----------:|----------:|
| Perform_OneInputBuffer          | 2.277 ms | 0.0048 ms | 0.0042 ms |         - |
| Perform_TwoIputBuffers          | 2.079 ms | 0.0090 ms | 0.0080 ms |         - |
| Perform_OneInputSourceReference | 2.018 ms | 0.0093 ms | 0.0078 ms |       2 B |
| Perform_OneInputTransformOutput | 2.029 ms | 0.0148 ms | 0.0131 ms |       2 B |

## ArrayBuffer
| Method                 | Mean     | Error     | StdDev    | Allocated |
|----------------------- |---------:|----------:|----------:|----------:|
| Perform_SingleInputSet | 1.356 ms | 0.0031 ms | 0.0024 ms |         - |
| Perform_SingleInputAdd | 4.279 ms | 0.0169 ms | 0.0141 ms |       5 B |
| Perform_TwoInputSet    | 1.829 ms | 0.0126 ms | 0.0118 ms |       1 B |
| Perform_TwoInputAdd    | 4.914 ms | 0.0130 ms | 0.0109 ms |       5 B |

## FlatArrayBuffer
| Method                 | Mean     | Error     | StdDev    | Allocated |
|----------------------- |---------:|----------:|----------:|----------:|
| Perform_SingleInputSet | 2.009 ms | 0.0059 ms | 0.0052 ms |         - |
| Perform_SingleInputAdd | 6.143 ms | 0.1048 ms | 0.0929 ms |       5 B |
| Perform_TwoInputSet    | 2.228 ms | 0.0040 ms | 0.0037 ms |       2 B |
| Perform_TwoInputAdd    | 6.372 ms | 0.0204 ms | 0.0181 ms |       5 B |
