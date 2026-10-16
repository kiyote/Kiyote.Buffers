namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A sparse chunked grid with three value buffers sharing a single topology,
/// designed for a simulation that runs on its own thread while another thread
/// reads the most recently published state.
/// </summary>
/// <remarks>
/// <para>
/// The owning thread uses the grid exactly like a
/// <see cref="DoubleBufferedChunkedGrid{T}"/>: it reads
/// <see cref="GridBuffer.Alternate"/> (the previous state), writes
/// <see cref="GridBuffer.Current"/>, then calls <see cref="Swap"/>. Swap
/// publishes the buffer just written as the latest state.
/// </para>
/// <para>
/// A single consumer thread calls <see cref="Acquire"/> to obtain the latest
/// published buffer and disposes the returned lease to give it back. Because
/// there are three buffers the owning thread never waits: it always writes the
/// one buffer that is neither the latest nor leased. Acquire and Swap are
/// lock-free.
/// </para>
/// <para>
/// No other protection is provided. Leased buffers must be treated as
/// read-only, and the owning thread must not change topology
/// (<c>Attach</c>, <c>Detach</c>, <see cref="Compact"/>, <see cref="RemoveChunk"/>)
/// or call the copy methods while a lease is outstanding.
/// </para>
/// </remarks>
public sealed class TripleBufferedChunkedGrid<T> where T : unmanaged {

	private const int None = -1;

	private readonly ChunkMap<T> _map;
	private int _current;
	private int _alternate;
	private int _latest;
	private int _leased;

	public TripleBufferedChunkedGrid(
		int chunkSize = ChunkedGrid<T>.DefaultChunkSize
	) {
		_map = new ChunkMap<T>( chunkSize, 3 );
		_current = 0;
		_alternate = 1;
		_latest = 1;
		_leased = None;
	}

	internal ChunkMap<T> Map => _map;

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
	/// Publishes the current buffer as the latest state, makes it the
	/// alternate buffer, and selects a buffer that is not leased as the new
	/// current buffer. Never blocks.
	/// </summary>
	public void Swap() {
		int published = _current;
		int previous = _alternate;
		_ = Interlocked.Exchange( ref _latest, published );
		int leased = Volatile.Read( ref _leased );
		_alternate = published;
		_current = leased == previous
			? 3 - published - previous
			: previous;
	}

	/// <summary>
	/// Leases the most recently published buffer. Call from the consumer
	/// thread only; dispose the lease before acquiring another.
	/// </summary>
	public TripleBufferedLease<T> Acquire() {
		int latest;
		do {
			latest = Volatile.Read( ref _latest );
			_ = Interlocked.Exchange( ref _leased, latest );
		} while( Volatile.Read( ref _latest ) != latest );
		return new TripleBufferedLease<T>( this, latest );
	}

	internal void Release() {
		Volatile.Write( ref _leased, None );
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

	/// <inheritdoc cref="DoubleBufferedChunkedGrid{T}.TrySet(int, int, GridBuffer, T)"/>
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
			_map.CreateView( index, _alternate )
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
		return new BufferedChunkEnumerator<T>( _map.GetBuffer( _current ), _map.GetBuffer( _alternate ) );
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
			_map.CreateNeighbourhood( index, _alternate )
		);
		return true;
	}

	public BufferedNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		return new BufferedNeighbourhoodEnumerator<T>( _map.GetBuffer( _current ), _map.GetBuffer( _alternate ) );
	}

	/// <summary>
	/// Copies every value of the current buffer into the alternate buffer.
	/// </summary>
	public void CopyCurrentToAlternate() {
		_map.CopyBuffer( _current, _alternate );
	}

	/// <summary>
	/// Copies every value of the alternate buffer into the current buffer.
	/// </summary>
	public void CopyAlternateToCurrent() {
		_map.CopyBuffer( _alternate, _current );
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

	/// <inheritdoc cref="DoubleBufferedChunkedGrid{T}.Attach(ChunkedGrid{T}, int, int)"/>
	public void Attach(
		ChunkedGrid<T> source,
		int column,
		int row
	) {
		ArgumentNullException.ThrowIfNull( source );
		_map.Attach( source.Map, 0, column, row, _current );
	}

	/// <inheritdoc cref="DoubleBufferedChunkedGrid{T}.Attach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
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

	/// <inheritdoc cref="DoubleBufferedChunkedGrid{T}.Attach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
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
		return buffer == GridBuffer.Current ? _current : _alternate;
	}
}
