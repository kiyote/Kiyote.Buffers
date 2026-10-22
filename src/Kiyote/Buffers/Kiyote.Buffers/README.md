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
services.AddFlatBuffers(); // or services.AddRaggedBuffers()
```

Then inject the `IBufferOperator` into your class and use it to perform operations on buffers.

`AddFlatBuffers()` and `AddRaggedBuffers()` each register an `IBufferFactory` and an `IBufferOperator` only
when the service has not already been registered. Calling both chooses the factory registered first;
an application-provided factory or operator remains in place. `AddBufferOperators()` registers only
the operator. `AddBufferAnalyzers()` registers `IBufferAnalyzer` and, for convenience, the same default
`IBufferOperator` if one is not already registered. To select a different implementation, register
it before calling these methods.

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
                return (byte)((value + 1) % 255);
            },
            output
        );
    }
}
```

## ChunkedGrid

`Kiyote.Buffers.ChunkedGrids.ChunkedGrid<T>` (where `T : unmanaged`) is a sparse, unbounded 2D grid
stored as square, lazily allocated chunks (64x64 by default). The chunk size must be a power of two and
a multiple of the `Vector256<T>` lane count for a typed grid, so every chunk row can be processed with whole vectors.
`ChunkedGridLayout` alone stores only topology and accepts any power-of-two size from 1 to 4096;
`ChunkedGridLayer<T>` and the typed grids additionally require whole `Vector256<T>` rows.

A cell is populated only when its chunk is allocated and its presence bit is set, allowing irregular shapes.
`Column`/`Row`/`Width`/`Height` describe the chunk-aligned hull of allocated chunks; `TryGetPopulatedBounds`
computes the exact bounding box of the populated cells. Chunks are enumerated in ascending chunk index order.

```csharp
ChunkedGrid<float> grid = new();
grid[ 10, -3 ] = 1.5f;

foreach( GridChunk<float> chunk in grid.EnumerateChunks() ) {
    for( int r = 0; r < chunk.Size; r++ ) {
        Span<float> values = chunk.GetRowValues( r );
        ReadOnlySpan<ulong> presence = chunk.GetPresenceRow( r );
        for( int c = 0; c < chunk.Size; c += Vector256<float>.Count ) {
            Vector256<float> mask = PresenceMask.ExpandVector256<uint>( presence, c ).AsSingle();
            // ... Vector256.ConditionalSelect( mask, computed, original )
        }
    }
}
```

Other grids can be copied in with `Attach`, optionally converting values through an `ICellExtractor<TSource, T>`.
Chunks are allocated as required and overlapping populated cells are overwritten; call `HasOverlap` first
to detect overlap. Passing a grid to its own `Attach`, `HasOverlap` or `Detach` throws `ArgumentException`.

`Detach` reverses an `Attach` by unpopulating the same cells (with the same extractor, if one was used).
call `Compact` afterwards to release empty chunks
and shrink the hull.

For stencil operations that read across chunk borders (diffusion, blurs, etc.), use `EnumerateNeighbourhoods`
or `TryGetNeighbourhood`. Each `GridChunkNeighbourhood<T>` resolves the eight surrounding chunks once and
accepts local coordinates from `-Size` to `2 * Size - 1`: `GetRowValues( -1 )` / `GetRowValues( Size )` return
the adjacent halo rows as spans, `CopyColumn( -1, ... )` / `CopyColumn( Size, ... )` gather the halo columns,
and `TryGetValue` reads any single cell in the neighbourhood. `CopyPadded` / `CopyPaddedRow` gather the chunk
and a halo into contiguous scratch (see below).

## ChunkedGridLayout and ChunkedGridLayer

`ChunkedGridLayout` is the topology on its own: which chunks are allocated and which cells are populated, with no
values. Any number of `ChunkedGridLayer<T>` instances, of any unmanaged `T`, can share one layout, so a single
occupancy carries many kinds of data.

```csharp
ChunkedGridLayout layout = new( 32 );
ChunkedGridLayer<float> pressure = new( layout );
ChunkedGridLayer<Terrain> terrain = new( layout ); // a byte-sized enum

layout.SetPopulatedRun( 0, 0, 100 );               // populates cells (0..99, 0)
pressure.GetReference( 10, 0 ) = 1.5f;
terrain.TrySet( 10, 0, Terrain.Water );
```

Every allocated chunk has a dense index in `[0, ChunkCapacity)`. An index is stable while its chunk stays allocated,
so it can key per-chunk side arrays or cached `( chunkIndex, localIndex )` pairs from `TryLocate`. `RemoveChunk` and
`Compact` free indices for reuse, and every allocation or removal changes `Version`. A reused index always starts
with no populated cells and `default` values in every layer. Index-based access avoids coordinate lookups entirely:

```csharp
foreach( int chunk in layout.EnumerateChunks() ) {
	Span<float> values = pressure.GetValues( chunk );              // ChunkSize * ChunkSize, row-major
	ReadOnlySpan<int> neighbours = layout.GetNeighbours( chunk ); // 3x3, -1 where not allocated
}
```

Stencils can gather a chunk plus a halo from its neighbours into contiguous scratch, so the inner loop reads every
neighbour at a fixed offset. Regions whose chunk is not allocated are written as `default`:

```csharp
int stride = layout.ChunkSize + 2;
float[] scratch = new float[ ChunkPadding.GetPaddedLength( layout.ChunkSize, 1 ) ];
foreach( GridChunkNeighbourhood<float> n in pressure.EnumerateNeighbourhoods() ) {
	n.CopyPadded( 1, scratch );
	// local cell (c, r) is scratch[ ( r + 1 ) * stride + c + 1 ]; north is -stride, east is +1
}
```

`ChunkPadding.Gather` does the same over caller-provided spans of any element type. `ReadPresenceBits` reads up to
64 presence bits of a row across chunk borders, for example to find cells whose north neighbour is unpopulated:

```csharp
ulong cells = layout.ReadPresenceBits( chunk, 0, r, 32 );
ulong north = layout.ReadPresenceBits( chunk, 0, r - 1, 32 );
ulong exposed = cells & ~north;
```

Layers store values independently of presence: unpopulating a cell does not reset its value, and `GetValues`,
`GetRowValues` can write unpopulated cells; `GetReference` throws for unpopulated cells. The layout holds no reference to its layers; a layer
catches up with chunk allocations lazily and is collected when no longer referenced. `SwapStorage` exchanges the
values of two layers over the same layout in O(1). Neither type is thread-safe.

## DoubleBufferedChunkedGrid

`DoubleBufferedChunkedGrid<T>` keeps two value buffers (`Current` and `Alternate`) over one shared topology:
each chunk owns a single presence mask and two value arrays, so the buffers cannot get out of sync.

- Topology only changes through `Attach`, `Detach`, `Compact` and `RemoveChunk`. Enumerators throw if it changes mid-iteration.
- `Attach` writes the `Current` buffer only. New chunks start zeroed in both buffers.
- `TrySet` writes one buffer and only for cells that are already populated.
- `Swap()` exchanges the buffers in O(1). `CopyCurrentToAlternate` and `CopyAlternateToCurrent` copy all values.

```csharp
foreach( BufferedNeighbourhood<float> n in grid.EnumerateNeighbourhoods() ) {
	// read n.Alternate (previous state), write n.Current.Center.Values
}
grid.Swap();
```

## TripleBufferedChunkedGrid

`TripleBufferedChunkedGrid<T>` behaves like `DoubleBufferedChunkedGrid<T>` on its owning thread, and also lets
one other thread lease the most recently published buffer without blocking the simulation:

```csharp
// simulation thread
grid.Swap(); // publishes the buffer just written

// consumer thread
using( TripleBufferedLease<float> lease = grid.Acquire() ) {
	foreach( GridChunk<float> chunk in lease.EnumerateChunks() ) { /* read only */ }
}
```

`Swap` and `Acquire` are lock-free, and the simulation never writes the leased buffer. Treat the lease as
read-only. Topology and copy methods throw while a lease is outstanding; do not change topology or presence
through previously obtained writable spans while leased. Only one consumer may hold a lease at a time.

## Migration notes

- Buffer constructors and factories now require positive logical dimensions and reject unrepresentable storage;
  indexers reject coordinates outside logical rows and columns, including padding. Use validated `GetRowSpan`
  for row-wise access instead of indexing padded cells.
- Operators now reject null arguments and mismatched destination dimensions before writing. Catch
  `ArgumentNullException` or `InvalidOperationException` rather than relying on partial output.
- Grid coordinates, populated runs, extents, and padded scratch sizes now reject unrepresentable results rather
  than wrapping. Local chunk access rejects out-of-range coordinates; reserved presence bits in small chunks are
  ignored by derived queries. `Attach` copies populated cells only, not allocated empty chunks.
- Double- and triple-buffered grid `TryGet`/`TrySet` reject undefined `GridBuffer` values. Overlapping triple-buffer
  acquisitions and topology/copy changes while leased throw; copied or stale lease disposal cannot release a
  newer acquisition. Enumerators also reject topology changes when reading `Current`. Existing buffered views
  retain their physical buffer roles after `Swap`; fetch a new view for the new current buffer.
- Factory selection remains first-registration-wins: to override a default, register your factory or operator
  before `AddFlatBuffers()` or `AddRaggedBuffers()`. Replace old `AddBuffers()` examples with one of these methods.
