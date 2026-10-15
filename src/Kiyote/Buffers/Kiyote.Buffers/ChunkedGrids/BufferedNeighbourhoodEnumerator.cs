namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates both buffers of every chunk neighbourhood of a
/// multi-buffered grid. Throws
/// <see cref="InvalidOperationException"/> if the topology changes during enumeration.
/// </summary>
public ref struct BufferedNeighbourhoodEnumerator<T> where T : unmanaged {

	private ChunkCursor<T> _cursor;
	private readonly int _current;
	private readonly int _alternate;
	private NeighbourChunks<T> _chunks;

	internal BufferedNeighbourhoodEnumerator(
		ChunkMap<T> map,
		int current,
		int alternate
	) {
		_cursor = new ChunkCursor<T>( map );
		_current = current;
		_alternate = alternate;
		_chunks = default;
	}

	public readonly BufferedNeighbourhood<T> Current => new(
		new GridChunkNeighbourhood<T>( _cursor.Map, in _chunks, _current ),
		new GridChunkNeighbourhood<T>( _cursor.Map, in _chunks, _alternate )
	);

	public bool MoveNext() {
		if( !_cursor.MoveNext() ) {
			return false;
		}
		_chunks = _cursor.Map.GatherNeighbours( _cursor.Current );
		return true;
	}

	public readonly BufferedNeighbourhoodEnumerator<T> GetEnumerator() {
		return this;
	}
}
