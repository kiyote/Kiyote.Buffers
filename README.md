![CI](https://github.com/kiyote/Kiyote.Buffers/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://github.com/kiyote/Kiyote.Buffers/blob/badges/.badges/main/coverage.svg?raw=true)

# Kiyote.Buffers

Provides IBufferOperator to perform actions on large buffers.  `BufferOperator` provides a generic
implementation to allow any custom operation to be crafted as part of the `Perform` call.

In the `Kiyote.Buffers.Numerics` an implementation that provides common operations on buffers of numeric types
is provided.  This includes operations such as `Sum`, `Average`, `Min`, `Max`, and more.
