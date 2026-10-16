namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// An unbounded sparse 2D grid stored as square, power-of-two sized chunks
/// with a per-cell presence bit, laid out for vectorized processing.
/// </summary>
/// <remarks>
/// Chunks are allocated on demand. Each chunk's values are row-major and each
/// row is a whole number of <see cref="System.Runtime.Intrinsics.Vector256{T}"/>
/// lanes. The presence bits form a stencil describing which cells hold data.
/// Enumerators throw if chunks are added or removed while enumerating.
/// </remarks>
public sealed class ChunkedGrid<T> where T : unmanaged {

	public const int DefaultChunkSize = 64;
	public const int MaximumChunkSize = ChunkMap<T>.MaximumChunkSize;

	private readonly ChunkMap<T> _map;

	public ChunkedGrid(
		int chunkSize = DefaultChunkSize
	) {
		_map = new ChunkMap<T>( chunkSize, 1 );
	}

	internal ChunkMap<T> Map => _map;

	public int ChunkSize => _map.ChunkSize;

	public int ChunkShift => _map.ChunkShift;

	public int PresenceWordsPerRow => _map.WordsPerRow;

	public int ChunkCount => _map.Count;

	public bool IsEmpty => _map.Count == 0;

	/// <summary>
	/// The left-most column of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	public int Column => _map.Column;

	/// <summary>
	/// The top-most row of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	public int Row => _map.Row;

	public int Width => _map.Width;

	public int Height => _map.Height;

	/// <summary>
	/// Gets the value of a cell (default if unpopulated) or sets it, marking
	/// the cell populated and allocating its chunk if required.
	/// </summary>
	public T this[ int column, int row ] {
		get {
			_ = _map.TryGet( column, row, 0, out T value );
			return value;
		}
		set {
			_map.SetCell( column, row, 0, value );
		}
	}

	public bool TryGet(
		int column,
		int row,
		out T value
	) {
		return _map.TryGet( column, row, 0, out value );
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		return _map.IsPopulated( column, row );
	}

	/// <summary>
	/// Marks a cell unpopulated. The chunk remains allocated; see <see cref="Compact"/>.
	/// </summary>
	public void Clear(
		int column,
		int row
	) {
		_map.ClearCell( column, row );
	}

	public bool HasChunk(
		int chunkColumn,
		int chunkRow
	) {
		return _map.FindStorage( chunkColumn, chunkRow ) is not null;
	}

	public bool TryGetChunk(
		int chunkColumn,
		int chunkRow,
		out GridChunk<T> chunk
	) {
		GridChunkStorage<T>? storage = _map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			chunk = default;
			return false;
		}
		chunk = _map.CreateView( storage, 0 );
		return true;
	}

	public GridChunk<T> GetOrCreateChunk(
		int chunkColumn,
		int chunkRow
	) {
		return _map.CreateView( _map.GetOrCreateStorage( chunkColumn, chunkRow ), 0 );
	}

	public bool RemoveChunk(
		int chunkColumn,
		int chunkRow
	) {
		return _map.Remove( chunkColumn, chunkRow );
	}

	/// <summary>
	/// Enumerates the allocated chunks in no particular order. The grid must
	/// not have chunks added or removed during enumeration.
	/// </summary>
	public GridChunkEnumerator<T> EnumerateChunks() {
		return new GridChunkEnumerator<T>( _map, 0 );
	}

	/// <summary>
	/// Gets an allocated chunk together with its eight neighbours, for
	/// stencil operations that read across chunk borders.
	/// </summary>
	public bool TryGetNeighbourhood(
		int chunkColumn,
		int chunkRow,
		out GridChunkNeighbourhood<T> neighbourhood
	) {
		GridChunkStorage<T>? storage = _map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			neighbourhood = default;
			return false;
		}
		NeighbourChunks<T> chunks = _map.GatherNeighbours( storage );
		neighbourhood = new GridChunkNeighbourhood<T>( _map, in chunks, 0 );
		return true;
	}

	/// <summary>
	/// Enumerates the allocated chunks, in no particular order, each with its
	/// eight neighbours resolved. The grid must not have chunks added or
	/// removed during enumeration.
	/// </summary>
	public GridChunkNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		return new GridChunkNeighbourhoodEnumerator<T>( _map, 0 );
	}

	/// <summary>
	/// Determines whether any populated cell of <paramref name="source"/>,
	/// translated by (<paramref name="column"/>, <paramref name="row"/>),
	/// would land on a populated cell of this grid.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="source"/> is this grid.</exception>
	public bool HasOverlap<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		ValidateSource( source );
		return _map.HasOverlap( source.Map, column, row );
	}

	/// <summary>
	/// Copies the populated cells of <paramref name="source"/> into this grid,
	/// translated by (<paramref name="column"/>, <paramref name="row"/>),
	/// overwriting overlapping cells and allocating chunks as required.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="source"/> is this grid.</exception>
	public void Attach(
		ChunkedGrid<T> source,
		int column,
		int row
	) {
		ValidateSource( source );
		_map.Attach( source.Map, 0, column, row, 0 );
	}

	/// <summary>
	/// Copies the populated cells of <paramref name="source"/> into this grid,
	/// translated by (<paramref name="column"/>, <paramref name="row"/>),
	/// using <paramref name="extractor"/> to produce each value.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="source"/> is this grid.</exception>
	public void Attach<TSource, TExtractor>(
		ChunkedGrid<TSource> source,
		int column,
		int row,
		TExtractor extractor
	) where TSource : unmanaged
	  where TExtractor : struct, ICellExtractor<TSource, T> {
		ValidateSource( source );
		_map.Attach( source.Map, 0, column, row, 0, extractor );
	}

	/// <inheritdoc cref="Attach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
	public void Attach<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row,
		Func<TSource, (bool populated, T value)> extractor
	) where TSource : unmanaged {
		ArgumentNullException.ThrowIfNull( extractor );
		Attach( source, column, row, new FuncCellExtractor<TSource, T>( extractor ) );
	}

	/// <summary>
	/// Unpopulates every cell of this grid that a populated cell of
	/// <paramref name="source"/>, translated by (<paramref name="column"/>,
	/// <paramref name="row"/>), lands on.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="source"/> is this grid.</exception>
	public void Detach<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		ValidateSource( source );
		_map.Detach( source.Map, column, row );
	}

	/// <summary>
	/// Unpopulates every cell of this grid that a source cell accepted by
	/// <paramref name="extractor"/> lands on.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="source"/> is this grid.</exception>
	public void Detach<TSource, TExtractor>(
		ChunkedGrid<TSource> source,
		int column,
		int row,
		TExtractor extractor
	) where TSource : unmanaged
	  where TExtractor : struct, ICellExtractor<TSource, T> {
		ValidateSource( source );
		_map.Detach( source.Map, 0, column, row, extractor );
	}

	/// <inheritdoc cref="Detach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
	public void Detach<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row,
		Func<TSource, (bool populated, T value)> extractor
	) where TSource : unmanaged {
		ArgumentNullException.ThrowIfNull( extractor );
		Detach( source, column, row, new FuncCellExtractor<TSource, T>( extractor ) );
	}

	/// <summary>
	/// Removes every chunk that has no populated cells.
	/// </summary>
	/// <returns>The number of chunks removed.</returns>
	public int Compact() {
		return _map.Compact();
	}

	private void ValidateSource<TSource>(
		ChunkedGrid<TSource> source
	) where TSource : unmanaged {
		ArgumentNullException.ThrowIfNull( source );
		if( ReferenceEquals( source, this ) ) {
			throw new ArgumentException( "A grid cannot be combined with itself.", nameof( source ) );
		}
	}
}
