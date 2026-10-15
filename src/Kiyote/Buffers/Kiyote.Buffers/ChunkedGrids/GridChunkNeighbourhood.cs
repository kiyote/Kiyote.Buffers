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

	private readonly ChunkMap<T> _map;
	private readonly NeighbourChunks<T> _chunks;
	private readonly int _buffer;

	internal GridChunkNeighbourhood(
		ChunkMap<T> map,
		scoped in NeighbourChunks<T> chunks,
		int buffer
	) {
		_map = map;
		_chunks = chunks;
		_buffer = buffer;
	}

	public int Size => _map.ChunkSize;

	public GridChunk<T> Center => _map.CreateView( _chunks[ NeighbourChunks<T>.CenterIndex ]!, _buffer );

	public bool HasNeighbour(
		int dx,
		int dy
	) {
		return Get( dx, dy ) is not null;
	}

	public bool TryGetNeighbour(
		int dx,
		int dy,
		out GridChunk<T> chunk
	) {
		GridChunkStorage<T>? storage = Get( dx, dy );
		if( storage is null ) {
			chunk = default;
			return false;
		}
		chunk = _map.CreateView( storage, _buffer );
		return true;
	}

	/// <summary>
	/// Gets the values of a local row, or an empty span if the chunk holding
	/// that row is not allocated.
	/// </summary>
	public Span<T> GetRowValues(
		int localRow
	) {
		GridChunkStorage<T>? storage = Resolve( 0, localRow, out _, out int row );
		return storage is null
			? []
			: storage.Buffers[ _buffer ].AsSpan( row << _map.ChunkShift, _map.ChunkSize );
	}

	/// <summary>
	/// Gets the presence words of a local row, or an empty span if the chunk
	/// holding that row is not allocated.
	/// </summary>
	public Span<ulong> GetPresenceRow(
		int localRow
	) {
		GridChunkStorage<T>? storage = Resolve( 0, localRow, out _, out int row );
		return storage is null
			? []
			: storage.Presence.AsSpan( row * _map.WordsPerRow, _map.WordsPerRow );
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
		int size = _map.ChunkSize;
		int words = _map.WordsPerRow;
		ArgumentOutOfRangeException.ThrowIfLessThan( values.Length, size );
		ArgumentOutOfRangeException.ThrowIfLessThan( presence.Length, words );

		GridChunkStorage<T>? storage = Resolve( localColumn, 0, out int column, out _ );
		presence[ ..words ].Clear();
		if( storage is null ) {
			values[ ..size ].Clear();
			return false;
		}

		T[] source = storage.Buffers[ _buffer ];
		int shift = _map.ChunkShift;
		int word = column >> 6;
		ulong bit = 1UL << ( column & 63 );
		for( int r = 0; r < size; r++ ) {
			values[ r ] = source[ ( r << shift ) + column ];
			if( ( storage.Presence[ ( r * words ) + word ] & bit ) != 0 ) {
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
		GridChunkStorage<T>? storage = Resolve( localColumn, localRow, out int column, out int row );
		if( storage is not null && _map.IsBitSet( storage, column, row ) ) {
			value = storage.Buffers[ _buffer ][ ( row << _map.ChunkShift ) + column ];
			return true;
		}
		value = default;
		return false;
	}

	public bool IsPopulated(
		int localColumn,
		int localRow
	) {
		GridChunkStorage<T>? storage = Resolve( localColumn, localRow, out int column, out int row );
		return storage is not null && _map.IsBitSet( storage, column, row );
	}

	private GridChunkStorage<T>? Get(
		int dx,
		int dy
	) {
		ArgumentOutOfRangeException.ThrowIfLessThan( dx, -1 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( dx, 1 );
		ArgumentOutOfRangeException.ThrowIfLessThan( dy, -1 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( dy, 1 );
		return _chunks[ ( ( dy + 1 ) * 3 ) + dx + 1 ];
	}

	private GridChunkStorage<T>? Resolve(
		int localColumn,
		int localRow,
		out int column,
		out int row
	) {
		int size = _map.ChunkSize;
		ArgumentOutOfRangeException.ThrowIfLessThan( localColumn, -size );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localColumn, 2 * size );
		ArgumentOutOfRangeException.ThrowIfLessThan( localRow, -size );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, 2 * size );
		int shift = _map.ChunkShift;
		int mask = _map.ChunkMask;
		column = localColumn & mask;
		row = localRow & mask;
		return _chunks[ ( ( ( localRow >> shift ) + 1 ) * 3 ) + ( localColumn >> shift ) + 1 ];
	}
}
