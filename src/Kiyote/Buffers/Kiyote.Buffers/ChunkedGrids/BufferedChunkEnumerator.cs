namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates both buffers of the allocated chunks of a
/// multi-buffered grid in ascending chunk index order. Throws
/// <see cref="InvalidOperationException"/> if the topology changes during enumeration.
/// </summary>
public ref struct BufferedChunkEnumerator<T> where T : unmanaged {

	private readonly ChunkValues<T> _current;
	private readonly ChunkValues<T> _alternate;
	private ChunkIndexEnumerator _indices;

	internal BufferedChunkEnumerator(
		ChunkValues<T> current,
		ChunkValues<T> alternate
	) {
		_current = current;
		_alternate = alternate;
		_indices = current.Layout.EnumerateChunks();
	}

	public readonly BufferedChunk<T> Current => new(
		new GridChunk<T>( _current.Layout, _indices.Current, _current.Get( _indices.Current ) ),
		new GridChunk<T>( _alternate.Layout, _indices.Current, _alternate.Get( _indices.Current ) )
	);

	public bool MoveNext() {
		return _indices.MoveNext();
	}

	public readonly BufferedChunkEnumerator<T> GetEnumerator() {
		return this;
	}
}