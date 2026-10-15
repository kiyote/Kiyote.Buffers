namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates both buffers of the allocated chunks of a
/// multi-buffered grid. Throws
/// <see cref="InvalidOperationException"/> if the topology changes during enumeration.
/// </summary>
public ref struct BufferedChunkEnumerator<T> where T : unmanaged {

	private ChunkCursor<T> _cursor;
	private readonly int _current;
	private readonly int _alternate;

	internal BufferedChunkEnumerator(
		ChunkMap<T> map,
		int current,
		int alternate
	) {
		_cursor = new ChunkCursor<T>( map );
		_current = current;
		_alternate = alternate;
	}

	public readonly BufferedChunk<T> Current => new(
		_cursor.Map.CreateView( _cursor.Current, _current ),
		_cursor.Map.CreateView( _cursor.Current, _alternate )
	);

	public bool MoveNext() {
		return _cursor.MoveNext();
	}

	public readonly BufferedChunkEnumerator<T> GetEnumerator() {
		return this;
	}
}
