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
public sealed class DoubleBufferedChunkedGrid<T> where T : unmanaged {

	private readonly ChunkMap<T> _map;
	private int _current;

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

	/// <inheritdoc cref="ChunkedGridLayout.TryGetPopulatedBounds(out int, out int, out int, out int)"/>
	public bool TryGetPopulatedBounds(
		out int column,
		out int row,
		out int width,
		out int height
	) {
		return _map.Layout.TryGetPopulatedBounds( out column, out row, out width, out height );
	}

	/// <summary>
	/// Exchanges the roles of the current and alternate buffers. O(1).
	/// </summary>
	public void Swap() {
		_current ^= 1;
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		return _map.IsPopulated( column, row );
	}

	public bool TryGet(
		int column,
		int row,
		GridBuffer buffer,
		out T value
	) {
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
		return _map.TrySetPopulated( column, row, Resolve( buffer ), value );
	}

	public bool HasChunk(
		int chunkColumn,
		int chunkRow
	) {
		return _map.FindChunk( chunkColumn, chunkRow ) != ChunkedGridLayout.NoChunk;
	}

	public bool TryGetChunk(
		int chunkColumn,
		int chunkRow,
		out BufferedChunk<T> chunk
	) {
		int index = _map.FindChunk( chunkColumn, chunkRow );
		if( index == ChunkedGridLayout.NoChunk ) {
			chunk = default;
			return false;
		}
		chunk = new BufferedChunk<T>(
			_map.CreateView( index, _current ),
			_map.CreateView( index, _current ^ 1 )
		);
		return true;
	}

	public bool RemoveChunk(
		int chunkColumn,
		int chunkRow
	) {
		return _map.Remove( chunkColumn, chunkRow );
	}

	public BufferedChunkEnumerator<T> EnumerateChunks() {
		return new BufferedChunkEnumerator<T>( _map.GetBuffer( _current ), _map.GetBuffer( _current ^ 1 ) );
	}

	public bool TryGetNeighbourhood(
		int chunkColumn,
		int chunkRow,
		out BufferedNeighbourhood<T> neighbourhood
	) {
		int index = _map.FindChunk( chunkColumn, chunkRow );
		if( index == ChunkedGridLayout.NoChunk ) {
			neighbourhood = default;
			return false;
		}
		neighbourhood = new BufferedNeighbourhood<T>(
			_map.CreateNeighbourhood( index, _current ),
			_map.CreateNeighbourhood( index, _current ^ 1 )
		);
		return true;
	}

	public BufferedNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		return new BufferedNeighbourhoodEnumerator<T>( _map.GetBuffer( _current ), _map.GetBuffer( _current ^ 1 ) );
	}

	/// <summary>
	/// Copies every value of the current buffer into the alternate buffer.
	/// </summary>
	public void CopyCurrentToAlternate() {
		_map.CopyBuffer( _current, _current ^ 1 );
	}

	/// <summary>
	/// Copies every value of the alternate buffer into the current buffer.
	/// </summary>
	public void CopyAlternateToCurrent() {
		_map.CopyBuffer( _current ^ 1, _current );
	}

	/// <inheritdoc cref="ChunkedGrid{T}.HasOverlap{TSource}(ChunkedGrid{TSource}, int, int)"/>
	public bool HasOverlap<TSource>(
		ChunkedGrid<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		ArgumentNullException.ThrowIfNull( source );
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
		ArgumentNullException.ThrowIfNull( source );
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
		ArgumentNullException.ThrowIfNull( source );
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
		ArgumentNullException.ThrowIfNull( source );
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
		ArgumentNullException.ThrowIfNull( source );
		_map.Detach( source.Map, 0, column, row, extractor );
	}

	/// <inheritdoc cref="ChunkedGrid{T}.Compact"/>
	public int Compact() {
		return _map.Compact();
	}

	private int Resolve(
		GridBuffer buffer
	) {
		return buffer == GridBuffer.Current ? _current : _current ^ 1;
	}
}
