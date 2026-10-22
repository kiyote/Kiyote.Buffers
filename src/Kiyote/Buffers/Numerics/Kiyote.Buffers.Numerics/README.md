# Kiyote.Buffers.Numerics

Provides a INumericBufferOperator to perform actions on large numeric buffers.  These operations are
vectorized where possible for performance.

## Overview

This library was created to service the needs of large buffer operations in other libraries.  It offers a way
to reduce boilerplate code such that you only write the operation you wish to perform on the buffer and the
library will handle the rest.

## Getting Started

Register the factories and operators with the DI container:
```csharp
services.AddFlatNumericBuffers(); // or services.AddRaggedNumericBuffers()
```

Then inject the `INumericBufferOperator` into your class and use it to perform operations on buffers.

`AddFlatNumericBuffers()` and `AddRaggedNumericBuffers()` register default `INumericBufferFactory` and
`INumericBufferOperator` services only when no registration exists for each service. Calling both chooses
the first registered factory; an application-provided factory or operator takes precedence. Register a
custom service before calling these methods to substitute it. `AddNumericBuffers()` registers only the
operator, not a factory.

```csharp
public class MyClass
{
    private readonly INumericBufferFactory _bufferFactory;
    private readonly INumericBufferOperator _bufferOperator;

    public MyClass(
        INumericBufferFactory bufferFactory,
        INumericBufferOperator bufferOperator
    ) {
        _bufferFactory = bufferFactory;
        _bufferOperator = bufferOperator;
    }

    public void ProcessBuffer()
    {
        INumericBuffer<float> buffer = _bufferFactory.Create( 100, 100, 0f );
        // Fill the buffer with whatever

        _bufferOperator.Normalize( buffer );
        // Now the buffer will contain normalized values between 0 and 1
    }
}
```

## Migration notes

- `AddNumericBuffers()` registers only `INumericBufferOperator`. For injection of `INumericBufferFactory`,
  use `AddFlatNumericBuffers()` or `AddRaggedNumericBuffers()` as above. If both are called, the first factory
  registration wins; register a custom factory or operator first to override the defaults.
- Numeric buffers require positive logical dimensions; indexers reject padded or out-of-range coordinates.
  Destination-taking operations reject mismatched shapes before writing with `InvalidOperationException`.
- `Normalize` maps finite floating-point values into `[0, 1]`; for integral buffers, intermediate values map
  to zero and only the maximum maps to one. Constant inputs produce zero, including for out-of-place normalization
  and scaling. `Normalize` and `ScaleToRange` reject nonfinite inputs before writing and throw on unrepresentable
  ranges; scaling uses double intermediates and saturating destination conversion, which may lose precision on
  large integers. Callers depending on previous integer division, constant-source copying or partial writes
  should update their handling.

Before: `services.AddNumericBuffers();` followed by injection of `INumericBufferFactory` (not registered).
After: `services.AddFlatNumericBuffers();` or `services.AddRaggedNumericBuffers();` to register the factory
and operator together.
