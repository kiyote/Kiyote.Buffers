namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates the allocated chunks of a grid with their neighbours resolved.
/// Throws <see cref="InvalidOperationException"/> if the grid topology
/// changes during enumeration.
/// </summary>
public ref struct GridChunkNeighbourhoodEnumerator<T> where T : unmanaged {

	private ChunkCursor<T> _cursor;
	private readonly int _buffer;
	private NeighbourChunks<T> _current;

	internal GridChunkNeighbourhoodEnumerator(
		ChunkMap<T> map,
		int buffer
	) {
		_cursor = new ChunkCursor<T>( map );
		_buffer = buffer;
		_current = default;
	}

	public readonly GridChunkNeighbourhood<T> Current => new( _cursor.Map, in _current, _buffer );

	public bool MoveNext() {
		if( !_cursor.MoveNext() ) {
			return false;
		}
		_current = _cursor.Map.GatherNeighbours( _cursor.Current );
		return true;
	}

	public readonly GridChunkNeighbourhoodEnumerator<T> GetEnumerator() {
		return this;
	}
}
