namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// One <typeparamref name="T"/> value per cell of a shared
/// <see cref="ChunkedGridLayout"/>. Several layers of different element types
/// can share one layout, so a single topology carries many kinds of data.
/// </summary>
/// <remarks>
/// <para>
/// The layout holds no reference to its layers; an unreferenced layer is
/// simply collected. A layer catches up with chunks allocated or removed in
/// its layout at the start of every member, so the steady-state cost is a
/// single comparison. Newly allocated chunks, including reused chunk
/// indices, always start with every value <see langword="default"/>.
/// </para>
/// <para>
/// Values are stored independently of presence: unpopulating a cell does not
/// reset its value, and <see cref="GetValues"/>, <see cref="GetRowValues"/>
/// and <see cref="GetReference"/> can write unpopulated cells. Callers that
/// need unpopulated cells to hold <see langword="default"/> enforce it themselves.
/// </para>
/// <para>
/// Every row of a chunk is a whole number of
/// <see cref="System.Runtime.Intrinsics.Vector256{T}"/> lanes. Not thread-safe.
/// </para>
/// </remarks>
public sealed class ChunkedGridLayer<T> where T : unmanaged {

	private readonly ChunkValues<T> _values;

	/// <summary>
	/// Creates a layer over <paramref name="layout"/>; every existing value reads as <see langword="default"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="layout"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The layout's chunk size is not a multiple of the number of
	/// <typeparamref name="T"/> lanes in a <see cref="System.Runtime.Intrinsics.Vector256{T}"/>.
	/// </exception>
	public ChunkedGridLayer(
		ChunkedGridLayout layout
	) {
		ArgumentNullException.ThrowIfNull( layout );
		ChunkValues<T>.ValidateChunkSize( layout.ChunkSize, nameof( layout ) );
		Layout = layout;
		_values = new ChunkValues<T>( layout );
	}

	/// <summary>
	/// The layout whose topology this layer follows.
	/// </summary>
	public ChunkedGridLayout Layout { get; }

	/// <summary>
	/// The width and height of a chunk in cells.
	/// </summary>
	public int ChunkSize => Layout.ChunkSize;

	/// <summary>
	/// Gets the values of a chunk: <see cref="ChunkSize"/> rows of
	/// <see cref="ChunkSize"/> elements, row-major.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public Span<T> GetValues(
		int chunkIndex
	) {
		_values.Synchronize();
		return _values.Get( chunkIndex );
	}

	/// <summary>
	/// Gets the <see cref="ChunkSize"/> values of one row of a chunk.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkIndex"/> is not allocated, or <paramref name="localRow"/> is outside the chunk.
	/// </exception>
	public Span<T> GetRowValues(
		int chunkIndex,
		int localRow
	) {
		_values.Synchronize();
		T[] values = _values.Get( chunkIndex );
		ArgumentOutOfRangeException.ThrowIfNegative( localRow );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, ChunkSize );
		return values.AsSpan( localRow << Layout.ChunkShift, ChunkSize );
	}

	/// <summary>
	/// Gets a view of a chunk's values together with the layout's presence.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public GridChunk<T> GetChunk(
		int chunkIndex
	) {
		_values.Synchronize();
		return new GridChunk<T>( Layout, chunkIndex, _values.Get( chunkIndex ) );
	}

	/// <summary>
	/// Gets a chunk together with its eight neighbours, for stencil operations
	/// that read across chunk borders.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public GridChunkNeighbourhood<T> GetNeighbourhood(
		int chunkIndex
	) {
		_values.Synchronize();
		_ = _values.Get( chunkIndex );
		return new GridChunkNeighbourhood<T>( Layout, _values, chunkIndex );
	}

	/// <summary>
	/// Gets a reference to the value of a populated cell.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The cell is not populated.</exception>
	public ref T GetReference(
		int column,
		int row
	) {
		_values.Synchronize();
		if( !Layout.TryLocate( column, row, out int chunkIndex, out int localIndex ) ) {
			throw new ArgumentOutOfRangeException( nameof( column ), $"The cell ({column}, {row}) is not populated." );
		}
		return ref _values.Get( chunkIndex )[ localIndex ];
	}

	/// <summary>
	/// Gets the value of a populated cell.
	/// </summary>
	/// <returns><see langword="false"/>, with <paramref name="value"/> <see langword="default"/>, if the cell is not populated.</returns>
	public bool TryGet(
		int column,
		int row,
		out T value
	) {
		_values.Synchronize();
		if( Layout.TryLocate( column, row, out int chunkIndex, out int localIndex ) ) {
			value = _values.Get( chunkIndex )[ localIndex ];
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Writes the value of a populated cell. Never changes topology.
	/// </summary>
	/// <returns><see langword="false"/> if the cell is not populated.</returns>
	public bool TrySet(
		int column,
		int row,
		T value
	) {
		_values.Synchronize();
		if( Layout.TryLocate( column, row, out int chunkIndex, out int localIndex ) ) {
			_values.Get( chunkIndex )[ localIndex ] = value;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Enumerates the allocated chunks in ascending chunk index order. The
	/// layout must not have chunks allocated or removed during enumeration.
	/// </summary>
	public GridChunkEnumerator<T> EnumerateChunks() {
		_values.Synchronize();
		return new GridChunkEnumerator<T>( _values );
	}

	/// <summary>
	/// Enumerates the allocated chunks in ascending chunk index order, each
	/// with its eight neighbours resolved. The layout must not have chunks
	/// allocated or removed during enumeration.
	/// </summary>
	public GridChunkNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		_values.Synchronize();
		return new GridChunkNeighbourhoodEnumerator<T>( _values );
	}

	/// <summary>
	/// Exchanges all values with <paramref name="other"/>. O(1).
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="other"/> is this layer or uses a different layout instance.
	/// </exception>
	public void SwapStorage(
		ChunkedGridLayer<T> other
	) {
		ValidateOther( other );
		_values.SwapWith( other._values );
	}

	/// <summary>
	/// Copies the values of every allocated chunk into <paramref name="other"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="other"/> is this layer or uses a different layout instance.
	/// </exception>
	public void CopyTo(
		ChunkedGridLayer<T> other
	) {
		ValidateOther( other );
		_values.CopyTo( other._values );
	}

	/// <summary>
	/// Sets every value to <see langword="default"/>. Presence is untouched.
	/// </summary>
	public void ClearValues() {
		_values.Clear();
	}

	private void ValidateOther(
		ChunkedGridLayer<T> other
	) {
		ArgumentNullException.ThrowIfNull( other );
		if( ReferenceEquals( other, this ) ) {
			throw new ArgumentException( "A layer cannot be combined with itself.", nameof( other ) );
		}
		if( !ReferenceEquals( other.Layout, Layout ) ) {
			throw new ArgumentException( "The layers must share the same layout.", nameof( other ) );
		}
	}
}
