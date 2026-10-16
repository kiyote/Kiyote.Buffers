namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A chunk together with its eight neighbours, addressed with local
/// coordinates in the range <c>-Size</c> to <c>2 * Size - 1</c> on each axis.
/// </summary>
/// <remarks>
/// Neighbours that are not allocated read as unpopulated. Useful for stencil
/// operations (diffusion, flow, edge detection) that read across chunk borders.
/// </remarks>
public readonly ref struct GridChunkNeighbourhood<T> where T : unmanaged {

	private readonly ChunkedGridLayout _layout;
	private readonly NeighbourChunks<T> _chunks;

	internal GridChunkNeighbourhood(
		ChunkedGridLayout layout,
		ChunkValues<T> values,
		int chunkIndex
	) {
		NeighbourChunks<T> chunks = default;
		for( int k = 0; k < NeighbourChunks<T>.Length; k++ ) {
			int index = layout.GetNeighbour( chunkIndex, k );
			chunks[ k ] = index == ChunkedGridLayout.NoChunk
				? new NeighbourChunk<T>( index, null, null )
				: new NeighbourChunk<T>( index, values.Get( index ), layout.GetPresenceArray( index ) );
		}
		_layout = layout;
		_chunks = chunks;
		ChunkIndex = chunkIndex;
	}

	public int Size => _layout.ChunkSize;

	/// <summary>
	/// The index of the centre chunk in its <see cref="ChunkedGridLayout"/>.
	/// </summary>
	public int ChunkIndex { get; }

	public GridChunk<T> Center => new( _layout, ChunkIndex, _chunks[ ChunkedGridLayout.CentreNeighbour ].Values! );

	public bool HasNeighbour(
		int dx,
		int dy
	) {
		return Get( dx, dy ).Values is not null;
	}

	public bool TryGetNeighbour(
		int dx,
		int dy,
		out GridChunk<T> chunk
	) {
		NeighbourChunk<T> neighbour = Get( dx, dy );
		if( neighbour.Values is null ) {
			chunk = default;
			return false;
		}
		chunk = new GridChunk<T>( _layout, neighbour.Index, neighbour.Values );
		return true;
	}

	/// <summary>
	/// Gets the values of a local row, or an empty span if the chunk holding
	/// that row is not allocated.
	/// </summary>
	public Span<T> GetRowValues(
		int localRow
	) {
		NeighbourChunk<T> chunk = Resolve( 0, localRow, out _, out int row );
		return chunk.Values is null
			? []
			: chunk.Values.AsSpan( row << _layout.ChunkShift, _layout.ChunkSize );
	}

	/// <summary>
	/// Gets the presence words of a local row, or an empty span if the chunk
	/// holding that row is not allocated.
	/// </summary>
	public Span<ulong> GetPresenceRow(
		int localRow
	) {
		NeighbourChunk<T> chunk = Resolve( 0, localRow, out _, out int row );
		int words = _layout.PresenceWordsPerRow;
		return chunk.Presence is null
			? []
			: chunk.Presence.AsSpan( row * words, words );
	}

	/// <summary>
	/// Copies a local column into <paramref name="values"/> and its presence
	/// (one bit per row) into <paramref name="presence"/>. Both are cleared
	/// and <see langword="false"/> returned if the chunk is not allocated.
	/// </summary>
	public bool CopyColumn(
		int localColumn,
		Span<T> values,
		Span<ulong> presence
	) {
		int size = _layout.ChunkSize;
		int words = _layout.PresenceWordsPerRow;
		ArgumentOutOfRangeException.ThrowIfLessThan( values.Length, size );
		ArgumentOutOfRangeException.ThrowIfLessThan( presence.Length, words );

		NeighbourChunk<T> chunk = Resolve( localColumn, 0, out int column, out _ );
		presence[ ..words ].Clear();
		if( chunk.Values is null ) {
			values[ ..size ].Clear();
			return false;
		}

		T[] source = chunk.Values;
		ulong[] sourcePresence = chunk.Presence!;
		int shift = _layout.ChunkShift;
		int word = column >> 6;
		ulong bit = 1UL << ( column & 63 );
		for( int r = 0; r < size; r++ ) {
			values[ r ] = source[ ( r << shift ) + column ];
			if( ( sourcePresence[ ( r * words ) + word ] & bit ) != 0 ) {
				presence[ r >> 6 ] |= 1UL << ( r & 63 );
			}
		}
		return true;
	}

	public bool TryGetValue(
		int localColumn,
		int localRow,
		out T value
	) {
		NeighbourChunk<T> chunk = Resolve( localColumn, localRow, out int column, out int row );
		if( chunk.Values is not null && _layout.IsBitSet( chunk.Presence!, column, row ) ) {
			value = chunk.Values[ ( row << _layout.ChunkShift ) + column ];
			return true;
		}
		value = default;
		return false;
	}

	public bool IsPopulated(
		int localColumn,
		int localRow
	) {
		NeighbourChunk<T> chunk = Resolve( localColumn, localRow, out int column, out int row );
		return chunk.Presence is not null && _layout.IsBitSet( chunk.Presence, column, row );
	}

	private NeighbourChunk<T> Get(
		int dx,
		int dy
	) {
		ArgumentOutOfRangeException.ThrowIfLessThan( dx, -1 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( dx, 1 );
		ArgumentOutOfRangeException.ThrowIfLessThan( dy, -1 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( dy, 1 );
		return _chunks[ ( ( dy + 1 ) * 3 ) + dx + 1 ];
	}

	private NeighbourChunk<T> Resolve(
		int localColumn,
		int localRow,
		out int column,
		out int row
	) {
		int size = _layout.ChunkSize;
		ArgumentOutOfRangeException.ThrowIfLessThan( localColumn, -size );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localColumn, 2 * size );
		ArgumentOutOfRangeException.ThrowIfLessThan( localRow, -size );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, 2 * size );
		int shift = _layout.ChunkShift;
		int mask = _layout.ChunkMask;
		column = localColumn & mask;
		row = localRow & mask;
		return _chunks[ ( ( ( localRow >> shift ) + 1 ) * 3 ) + ( localColumn >> shift ) + 1 ];
	}
}
