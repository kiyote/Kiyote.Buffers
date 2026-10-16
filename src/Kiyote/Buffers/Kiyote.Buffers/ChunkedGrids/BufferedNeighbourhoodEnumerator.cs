namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates both buffers of every chunk neighbourhood of a
/// multi-buffered grid in ascending chunk index order. Throws
/// <see cref="InvalidOperationException"/> if the topology changes during enumeration.
/// </summary>
public ref struct BufferedNeighbourhoodEnumerator<T> where T : unmanaged {

	private readonly ChunkValues<T> _current;
	private readonly ChunkValues<T> _alternate;
	private ChunkIndexEnumerator _indices;

	internal BufferedNeighbourhoodEnumerator(
		ChunkValues<T> current,
		ChunkValues<T> alternate
	) {
		_current = current;
		_alternate = alternate;
		_indices = current.Layout.EnumerateChunks();
	}

	public readonly BufferedNeighbourhood<T> Current => new(
		new GridChunkNeighbourhood<T>( _current.Layout, _current, _indices.Current ),
		new GridChunkNeighbourhood<T>( _alternate.Layout, _alternate, _indices.Current )
	);

	public bool MoveNext() {
		return _indices.MoveNext();
	}

	public readonly BufferedNeighbourhoodEnumerator<T> GetEnumerator() {
		return this;
	}
}