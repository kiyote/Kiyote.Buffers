# Kiyote.Buffers

Provides IBufferOperator to perform actions on large buffers.  `BufferOperator` provides a generic
implementation to allow any custom operation to be crafted as part of the `Perform` call.

## Overview

This library was created to service the needs of large buffer operations in other libraries.  It offers a way
to reduce boilerplate code such that you only write the operation you wish to perform on the buffer and the
library will handle the rest.

## Getting Started

Register the factories and operators with the DI container:
```csharp
services.AddBuffers();
````

Then inject the `IBufferOperator` into your class and use it to perform operations on buffers.

```csharp
public class MyClass
{
    private readonly IBufferFactory _bufferFactory;
    private readonly IBufferOperator _bufferOperator;

    public MyClass(
        IBufferFactory bufferFactory,
        IBufferOperator bufferOperator
    ) {
        _bufferFactory = bufferFactory;
        _bufferOperator = bufferOperator;
    }

    public void ProcessBuffer()
    {
        IBuffer<byte> input = _bufferFactory.Create( 100, 100, 0 );
        IBuffer<byte> output = _bufferFactory.Create( 100, 100, 0 );
        
        _bufferOperator.Perform(
            input,
            value => {
                return value + 1 % 255;
            },
            output
        );
    }
}
```
