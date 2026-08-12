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
services.AddNumericBuffers();
````

Then inject the `INumericBufferOperator` into your class and use it to perform operations on buffers.

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
        IBuffer<float> buffer = _bufferFactory.Create( 100, 100, 0 );
        // Fill the buffer with whatever

        _bufferOperator.Normalize( buffer );
        // Now the buffer will contain normalized values between 0 and 1
    }
}
```
