namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A sparse chunked grid with two value buffers that share a single topology.
/// </summary>
/// <remarks>
/// <para>
/// Each chunk owns one presence mask and two value arrays, so the buffers can
/// never disagree about which cells exist. Topology can only change through
/// <see cref="Attach(ChunkedGrid{T}, int, int)"/>, <c>Detach</c>,
/// <see cref="Compact"/> and <see cref="RemoveChunk"/>; enumerators throw if
/// this happens mid-iteration. Value writes touch one buffer only.
/// </para>
/// <para>
/// <see cref="Attach(ChunkedGrid{T}, int, int)"/> writes only the
/// <see cref="GridBuffer.Current"/> buffer, so the alternate buffer can be
/// read concurrently. Newly allocated chunks start zeroed in both buffers.
/// No synchronisation is performed; callers coordinate access themselves.
/// </para>
/// </remarks>
public sealed class DoubleBufferedChunkedGrid<T> : IDisposable where T : unmanaged {

	private readonly ChunkMap<T> _map;
	private int _current;
	private bool _disposed;

	public DoubleBufferedChunkedGrid(
		int chunkSize = ChunkedGrid<T>.DefaultChunkSize
	) {
		_map = new ChunkMap<T>( chunkSize, 2 );
	}

	public int ChunkSize => _map.ChunkSize;

	public int ChunkShift => _map.ChunkShift;

	public int PresenceWordsPerRow => _map.WordsPerRow;

	public int ChunkCount => _map.Count;

	public bool IsEmpty => _map.Count == 0;

	public int Column => _map.Column;

	public int Row => _map.Row;

	public int Width => _map.Width;

	public int Height => _map.Height;

	/// <summary>
	/// Exchanges the roles of the current and alternate buffers. O(1).
	/// </summary>
	public void Swap() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		_current ^= 1;
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.IsPopulated( column, row );
	}

	public bool TryGet(
		int column,
		int row,
		GridBuffer buffer,
		out T value
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.TryGet( column, row, Resolve( buffer ), out value );
	}

	/// <summary>
	/// Writes a value into an already populated cell. Never changes topology.
	/// </summary>
	/// <returns><see langword="false"/> if the cell is not populated.</returns>
	public bool TrySet(
		int column,
		int row,
		GridBuffer buffer,
		T value
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.TrySetPopulated( column, row, Resolve( buffer ), value );
	}

	public bool HasChunk(
		int chunkColumn,
		int chunkRow
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.FindStorage( chunkColumn, chunkRow ) is not null;
	}

	public bool TryGetChunk(
		int chunkColumn,
		int chunkRow,
		out BufferedChunk<T> chunk
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		GridChunkStorage<T>? storage = _map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			chunk = default;
			return false;
		}
		chunk = new BufferedChunk<T>(
			_map.CreateView( storage, _current ),
			_map.CreateView( storage, _current ^ 1 )
		);
		return true;
	}

	public bool RemoveChunk(
		int chunkColumn,
		int chunkRow
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.Remove( chunkColumn, chunkRow );
	}

	public BufferedChunkEnumerator<T> EnumerateChunks() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return new BufferedChunkEnumerator<T>( _map, _current, _current ^ 1 );
	}

	public bool TryGetNeighbourhood(
		int chunkColumn,
		int chunkRow,
		out BufferedNeighbourhood<T> neighbourhood
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );
		GridChunkStorage<T>? storage = _map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			neighbourhood = default;
			return false;
		}
		NeighbourChunks<T> chunks = _map.GatherNeighbours( storage );
		neighbourhood = new BufferedNeighbourhood<T>(
			new GridChunkNeighbourhood<T>( _map, in chunks, _current ),
			new GridChunkNeighbourhood<T>( _map, in chunks, _current ^ 1 )
		);
		return true;
	}

	public BufferedNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return new BufferedNeighbourhoodEnumerator<T>( _map, _current, _current ^ 1 );
	}

	/// <summary>
	/// Copies every value of the current buffer into the alternate buffer.
	/// </summary>
	public void CopyCurrentToAlternate() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		_map.CopyBuffer( _current, _current ^ 1 );
	}

	/// <summary>
	/// Copies every value of the alternate buffer into the current buffer.
	/// </summary>
	public void CopyAlternateToCurrent() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		_map.CopyBuffer( _current ^ 1, _current );
	}

	/// <inheritdoc cref="ChunkedGrid{T}.HasOverlap{TSource}(ChunkedGrid{TSource}, int, int)"/>
	public bool HasOverlap<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		ValidateSource( source );
		return _map.HasOverlap( source.Map, column, row );
	}

	/// <summary>
	/// Copies the populated cells of <paramref name="source"/> into the
	/// current buffer, translated by (<paramref name="column"/>,
	/// <paramref name="row"/>), populating them and allocating chunks as required.
	/// </summary>
	public void Attach(
		ChunkedGrid<T> source,
		int column,
		int row
	) {
		ValidateSource( source );
		_map.Attach( source.Map, 0, column, row, _current );
	}

	/// <summary>
	/// Copies the populated cells of <paramref name="source"/> accepted by
	/// <paramref name="extractor"/> into the current buffer.
	/// </summary>
	public void Attach<TSource, TExtractor>(
		ChunkedGrid<TSource> source,
		int column,
		int row,
		TExtractor extractor
	) where TSource : unmanaged
	  where TExtractor : struct, ICellExtractor<TSource, T> {
		ValidateSource( source );
		_map.Attach( source.Map, 0, column, row, _current, extractor );
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

	/// <inheritdoc cref="ChunkedGrid{T}.Detach{TSource}(ChunkedGrid{TSource}, int, int)"/>
	public void Detach<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		ValidateSource( source );
		_map.Detach( source.Map, column, row );
	}

	/// <inheritdoc cref="ChunkedGrid{T}.Detach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
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

	/// <inheritdoc cref="ChunkedGrid{T}.Compact"/>
	public int Compact() {
		ObjectDisposedException.ThrowIf( _disposed, this );
		return _map.Compact();
	}

	public void Dispose() {
		if( _disposed ) {
			return;
		}
		_map.Reset();
		_disposed = true;
	}

	private int Resolve(
		GridBuffer buffer
	) {
		return buffer == GridBuffer.Current ? _current : _current ^ 1;
	}

	private void ValidateSource<TSource>(
		ChunkedGrid<TSource> source
	) where TSource : unmanaged {
		ObjectDisposedException.ThrowIf( _disposed, this );
		ArgumentNullException.ThrowIfNull( source );
	}
}
