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

	/// <summary>
	/// Writes the <c>( Size + 2 * halo )²</c> block centred on this chunk into
	/// <paramref name="values"/>, row-major with stride <c>Size + 2 * halo</c>.
	/// Element <c>( halo + r ) * ( Size + 2 * halo ) + ( halo + c )</c> is local
	/// cell (<c>c</c>, <c>r</c>). Regions whose chunk is not allocated are
	/// written as <see langword="default"/>; unpopulated cells of allocated
	/// chunks are copied as stored.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="halo"/> is not in <c>[0, Size]</c>, or <paramref name="values"/> is too short.
	/// </exception>
	public void CopyPadded(
		int halo,
		Span<T> values
	) {
		ChunkNeighbourSpans<T> spans = GetSpans();
		ChunkPadding.Gather( _layout.ChunkSize, halo, in spans, values );
	}

	/// <summary>
	/// Writes the <c>Size + 2 * halo</c> elements of one row of the block
	/// produced by <see cref="CopyPadded"/>; useful for rolling row-by-row stencils.
	/// </summary>
	/// <param name="localRow">The local row, in <c>[-halo, Size + halo)</c>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="halo"/> is not in <c>[0, Size]</c>, <paramref name="localRow"/>
	/// is outside the padded block, or <paramref name="values"/> is too short.
	/// </exception>
	public void CopyPaddedRow(
		int localRow,
		int halo,
		Span<T> values
	) {
		ChunkNeighbourSpans<T> spans = GetSpans();
		ChunkPadding.GatherRow( _layout.ChunkSize, halo, localRow, in spans, values );
	}

	/// <summary>
	/// Reads up to 64 presence bits of one row, relative to the centre chunk;
	/// see <see cref="ChunkedGridLayout.ReadPresenceBits"/>.
	/// </summary>
	/// <returns>Bit <c>i</c> is the presence of local column <c>localColumn + i</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">An argument is outside its range.</exception>
	public ulong ReadPresenceBits(
		int localColumn,
		int localRow,
		int count
	) {
		return _layout.ReadPresenceBits( ChunkIndex, localColumn, localRow, count );
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

	private ChunkNeighbourSpans<T> GetSpans() {
		return new ChunkNeighbourSpans<T>() {
			NorthWest = _chunks[ 0 ].Values,
			North = _chunks[ 1 ].Values,
			NorthEast = _chunks[ 2 ].Values,
			West = _chunks[ 3 ].Values,
			Centre = _chunks[ 4 ].Values,
			East = _chunks[ 5 ].Values,
			SouthWest = _chunks[ 6 ].Values,
			South = _chunks[ 7 ].Values,
			SouthEast = _chunks[ 8 ].Values
		};
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
