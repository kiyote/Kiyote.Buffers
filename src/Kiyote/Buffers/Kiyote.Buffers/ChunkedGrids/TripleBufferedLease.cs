namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A lease on the latest published buffer of a
/// <see cref="TripleBufferedChunkedGrid{T}"/>, obtained with
/// <see cref="TripleBufferedChunkedGrid{T}.Acquire"/>. Dispose it to return
/// the buffer to the owning thread.
/// </summary>
/// <remarks>
/// The lease performs no checks: treat the buffer as read-only, dispose the
/// lease exactly once, and do not use it after disposal.
/// </remarks>
public readonly struct TripleBufferedLease<T> : IDisposable, IEquatable<TripleBufferedLease<T>> where T : unmanaged {

	private readonly TripleBufferedChunkedGrid<T> _grid;
	private readonly int _buffer;

	internal TripleBufferedLease(
		TripleBufferedChunkedGrid<T> grid,
		int buffer
	) {
		_grid = grid;
		_buffer = buffer;
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
		GridChunkStorage<T>? storage = map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			chunk = default;
			return false;
		}
		chunk = map.CreateView( storage, _buffer );
		return true;
	}

	public GridChunkEnumerator<T> EnumerateChunks() {
		return new GridChunkEnumerator<T>( _grid.Map, _buffer );
	}

	public bool TryGetNeighbourhood(
		int chunkColumn,
		int chunkRow,
		out GridChunkNeighbourhood<T> neighbourhood
	) {
		ChunkMap<T> map = _grid.Map;
		GridChunkStorage<T>? storage = map.FindStorage( chunkColumn, chunkRow );
		if( storage is null ) {
			neighbourhood = default;
			return false;
		}
		NeighbourChunks<T> chunks = map.GatherNeighbours( storage );
		neighbourhood = new GridChunkNeighbourhood<T>( map, in chunks, _buffer );
		return true;
	}

	public GridChunkNeighbourhoodEnumerator<T> EnumerateNeighbourhoods() {
		return new GridChunkNeighbourhoodEnumerator<T>( _grid.Map, _buffer );
	}

	public void Dispose() {
		_grid?.Release();
	}

	public bool Equals(
		TripleBufferedLease<T> other
	) {
		return ReferenceEquals( _grid, other._grid ) && _buffer == other._buffer;
	}

	public override bool Equals(
		object? obj
	) {
		return obj is TripleBufferedLease<T> other && Equals( other );
	}

	public override int GetHashCode() {
		return HashCode.Combine( _grid, _buffer );
	}

	public static bool operator ==( TripleBufferedLease<T> left, TripleBufferedLease<T> right ) {
		return left.Equals( right );
	}

	public static bool operator !=( TripleBufferedLease<T> left, TripleBufferedLease<T> right ) {
		return !left.Equals( right );
	}
}
