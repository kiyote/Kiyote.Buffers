namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates the allocated chunks of a grid in ascending chunk index order.
/// Throws <see cref="InvalidOperationException"/> if chunks are allocated or
/// removed during enumeration.
/// </summary>
public ref struct GridChunkEnumerator<T> where T : unmanaged {

	private readonly ChunkValues<T> _values;
	private ChunkIndexEnumerator _indices;

	internal GridChunkEnumerator(
		ChunkValues<T> values
	) {
		_values = values;
		_indices = values.Layout.EnumerateChunks();
	}

	public readonly GridChunk<T> Current => new( _values.Layout, _indices.Current, _values.Get( _indices.Current ) );

	public bool MoveNext() {
		return _indices.MoveNext();
	}

	public readonly GridChunkEnumerator<T> GetEnumerator() {
		return this;
	}
}