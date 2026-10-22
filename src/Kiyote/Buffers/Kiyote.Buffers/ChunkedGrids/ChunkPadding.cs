namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Gathers a chunk and an <c>halo</c>-cell border taken from its neighbours
/// into one contiguous, row-major block, so stencil kernels can read every
/// neighbour of a cell at a fixed offset.
/// </summary>
/// <remarks>
/// The padded block is <c>size + 2 * halo</c> elements square. Element
/// <c>( halo + r ) * ( size + 2 * halo ) + ( halo + c )</c> is local cell
/// (<c>c</c>, <c>r</c>) of the centre chunk, for <c>-halo &lt;= c, r &lt; size + halo</c>.
/// Regions whose chunk is not allocated are written as <see langword="default"/>.
/// <typeparamref name="T"/> is unconstrained, so callers can pad any element
/// type held in their own row-major arrays.
/// </remarks>
public static class ChunkPadding {

	/// <summary>
	/// Gets the number of elements in a padded block: <c>( size + 2 * halo )²</c>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is not positive, <paramref name="halo"/> is not in <c>[0, size]</c>,
	/// or the padded length cannot be represented as an <see cref="int"/>.
	/// </exception>
	public static int GetPaddedLength(
		int size,
		int halo
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( size );
		ArgumentOutOfRangeException.ThrowIfNegative( halo );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( halo, size );
		long stride = (long)size + ( 2L * halo );
		if( stride > 46340 ) {
			throw new ArgumentOutOfRangeException( nameof( size ), "The padded length exceeds Int32.MaxValue." );
		}
		return (int)( stride * stride );
	}

	/// <summary>
	/// Writes the padded block of <paramref name="source"/> into <paramref name="destination"/>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is not positive, <paramref name="halo"/> is not in
	/// <c>[0, size]</c>, or <paramref name="destination"/> is shorter than
	/// <see cref="GetPaddedLength"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <see cref="ChunkNeighbourSpans{T}.Centre"/> is empty, or a non-empty span
	/// does not hold <c>size * size</c> elements.
	/// </exception>
	public static void Gather<T>(
		int size,
		int halo,
		scoped in ChunkNeighbourSpans<T> source,
		Span<T> destination
	) {
		int length = GetPaddedLength( size, halo );
		ArgumentOutOfRangeException.ThrowIfLessThan( destination.Length, length );
		ValidateSource( size, in source );

		int stride = size + ( 2 * halo );
		for( int r = 0; r < stride; r++ ) {
			GatherRowCore( size, halo, r - halo, in source, destination.Slice( r * stride, stride ) );
		}
	}

	/// <summary>
	/// Writes one row of the padded block of <paramref name="source"/>,
	/// <c>size + 2 * halo</c> elements, into <paramref name="destination"/>.
	/// </summary>
	/// <param name="localRow">The row of the centre chunk, in <c>[-halo, size + halo)</c>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is not positive, <paramref name="halo"/> is not in
	/// <c>[0, size]</c>, <paramref name="localRow"/> is outside the padded block,
	/// or <paramref name="destination"/> is shorter than a padded row.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <see cref="ChunkNeighbourSpans{T}.Centre"/> is empty, or a non-empty span
	/// does not hold <c>size * size</c> elements.
	/// </exception>
	public static void GatherRow<T>(
		int size,
		int halo,
		int localRow,
		scoped in ChunkNeighbourSpans<T> source,
		Span<T> destination
	) {
		_ = GetPaddedLength( size, halo );
		ArgumentOutOfRangeException.ThrowIfLessThan( localRow, -halo );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, size + halo );
		ArgumentOutOfRangeException.ThrowIfLessThan( destination.Length, size + ( 2 * halo ) );
		ValidateSource( size, in source );

		GatherRowCore( size, halo, localRow, in source, destination );
	}

	private static void GatherRowCore<T>(
		int size,
		int halo,
		int localRow,
		scoped in ChunkNeighbourSpans<T> source,
		Span<T> destination
	) {
		int band;
		int row;
		if( localRow < 0 ) {
			band = 0;
			row = localRow + size;
		} else if( localRow >= size ) {
			band = 6;
			row = localRow - size;
		} else {
			band = 3;
			row = localRow;
		}
		int rowStart = row * size;
		if( halo != 0 ) {
			CopySegment( source[ band ], rowStart + size - halo, destination[ ..halo ] );
			CopySegment( source[ band + 2 ], rowStart, destination.Slice( halo + size, halo ) );
		}
		CopySegment( source[ band + 1 ], rowStart, destination.Slice( halo, size ) );
	}

	private static void CopySegment<T>(
		ReadOnlySpan<T> chunk,
		int start,
		Span<T> destination
	) {
		if( chunk.IsEmpty ) {
			destination.Clear();
		} else {
			chunk.Slice( start, destination.Length ).CopyTo( destination );
		}
	}

	private static void ValidateSource<T>(
		int size,
		scoped in ChunkNeighbourSpans<T> source
	) {
		if( source.Centre.IsEmpty ) {
			throw new ArgumentException( "The centre chunk must not be empty.", nameof( source ) );
		}
		int area = size * size;
		for( int i = 0; i < ChunkedGridLayout.NeighbourCount; i++ ) {
			int length = source[ i ].Length;
			if( length != 0 && length != area ) {
				throw new ArgumentException( $"Each chunk must be empty or hold {area} elements.", nameof( source ) );
			}
		}
	}
}
