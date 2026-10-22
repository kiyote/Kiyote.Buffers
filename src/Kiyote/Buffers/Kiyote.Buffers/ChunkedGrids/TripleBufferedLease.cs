namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A lease on the latest published buffer of a
/// <see cref="TripleBufferedChunkedGrid{T}"/>, obtained with
/// <see cref="TripleBufferedChunkedGrid{T}.Acquire"/>. Dispose it to return
/// the buffer to the owning thread.
/// </summary>
/// <remarks>
/// The lease is a copyable value. Disposing any copy releases that acquisition;
/// repeated or stale disposals do not release later acquisitions. Treat the
/// buffer as read-only and do not use any copy after one is disposed. Read
/// accessors do not check whether a lease is still active. Disposing a default
/// lease does nothing; its read accessors are not valid.
/// </remarks>
public readonly struct TripleBufferedLease<T> : IDisposable, IEquatable<TripleBufferedLease<T>> where T : unmanaged {

	private readonly TripleBufferedChunkedGrid<T> _grid;
	private readonly int _buffer;
	private readonly long _token;

	internal TripleBufferedLease(
		TripleBufferedChunkedGrid<T> grid,
		int buffer,
		long token
	) {
		_grid = grid;
		_buffer = buffer;
		_token = token;
	}

	public int ChunkSize => _grid.Map.ChunkSize;

	public int ChunkCount => _grid.Map.Count;

	public bool TryGet(
		int column,
		int row,
		out T value
	) {
		return _grid.Map.TryGet( column, row, _buffer, out value );
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		return _grid.Map.IsPopulated( column, row );
	}

	public bool TryGetChunk(
		int chunkColumn,
		int chunkRow,
		out GridChunk<T> chunk
	) {
		ChunkMap<T> map = _grid.Map;
		int index = map.FindChunk( chunkColumn, chunkRow );
		if( index == ChunkedGridLayout.NoChunk ) {
			chunk = default;
			return false;
		}
		chunk = map.CreateView( index, _buffer );
		return true;
	}

	public GridChunkEnumerator<T> EnumerateChunks() {
		return new GridChunkEnumerator<T>( _grid.Map.GetBuffer( _buffer ) );
	}

	public bool TryGetNeighbourhood(
		int chunkColumn,
		int chunkRow,
		out GridChunkNeighbourhood<T> neighbourhood
	) {
		ChunkMap<T> map = _grid.Map;
		int index = map.FindChunk( chunkColumn, chunkRow );
		if( index == ChunkedGridLayout.NoChunk ) {
			neighbourhood = default;
			return false;
		}
		neighbourhood = map.CreateNeighbourhood( index, _buffer );
		return true;
	}

	public GridChunkNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		return new GridChunkNeighbourhoodEnumerator<T>( _grid.Map.GetBuffer( _buffer ) );
	}

	public void Dispose() {
		_grid?.Release( _token );
	}

	public bool Equals(
		TripleBufferedLease<T> other
	) {
		return ReferenceEquals( _grid, other._grid ) && _token == other._token;
	}

	public override bool Equals(
		object? obj
	) {
		return obj is TripleBufferedLease<T> other && Equals( other );
	}

	public override int GetHashCode() {
		return HashCode.Combine( _grid, _token );
	}

	public static bool operator ==( TripleBufferedLease<T> left, TripleBufferedLease<T> right ) {
		return left.Equals( right );
	}

	public static bool operator !=( TripleBufferedLease<T> left, TripleBufferedLease<T> right ) {
		return !left.Equals( right );
	}
}
