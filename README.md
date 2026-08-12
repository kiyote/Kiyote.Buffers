![CI](https://github.com/kiyote/Kiyote.Buffers/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://raw.githubusercontent.com/kiyote/Kiyote.Buffers/badges/.badges/main/coverage.svg)

# Kiyote.Buffers

Provides IBufferOperator to perform actions on large buffers.  `BufferOperator` provides a generic
implementation to allow any custom operation to be crafted as part of the `Perform` call.

In the `Kiyote.Buffers.Numerics` an implementation that provides common operations on buffers of numeric types
is provided.  This includes operations such as `Sum`, `Average`, `Min`, `Max`, and more.
