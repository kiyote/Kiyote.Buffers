namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates the allocated chunks of a grid. Throws
/// <see cref="InvalidOperationException"/> if the grid topology changes
/// during enumeration.
/// </summary>
public ref struct GridChunkEnumerator<T> where T : unmanaged {

	private ChunkCursor<T> _cursor;
	private readonly int _buffer;

	internal GridChunkEnumerator(
		ChunkMap<T> map,
		int buffer
	) {
		_cursor = new ChunkCursor<T>( map );
		_buffer = buffer;
	}

	public readonly GridChunk<T> Current => _cursor.Map.CreateView( _cursor.Current, _buffer );

	public bool MoveNext() {
		return _cursor.MoveNext();
	}

	public readonly GridChunkEnumerator<T> GetEnumerator() {
		return this;
	}
}
