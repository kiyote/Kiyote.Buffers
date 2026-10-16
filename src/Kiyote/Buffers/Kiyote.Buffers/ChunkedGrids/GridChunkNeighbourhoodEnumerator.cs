namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates the allocated chunks of a grid in ascending chunk index order,
/// with their neighbours resolved. Throws <see cref="InvalidOperationException"/>
/// if chunks are allocated or removed during enumeration.
/// </summary>
public ref struct GridChunkNeighbourhoodEnumerator<T> where T : unmanaged {

	private readonly ChunkValues<T> _values;
	private ChunkIndexEnumerator _indices;

	internal GridChunkNeighbourhoodEnumerator(
		ChunkValues<T> values
	) {
		_values = values;
		_indices = values.Layout.EnumerateChunks();
	}

	public readonly GridChunkNeighbourhood<T> Current => new( _values.Layout, _values, _indices.Current );

	public bool MoveNext() {
		return _indices.MoveNext();
	}

	public readonly GridChunkNeighbourhoodEnumerator<T> GetEnumerator() {
		return this;
	}
}