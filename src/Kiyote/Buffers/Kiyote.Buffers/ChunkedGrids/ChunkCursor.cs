namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Walks the chunks of a <see cref="ChunkMap{T}"/>, failing if the topology
/// changes during the walk.
/// </summary>
internal struct ChunkCursor<T> where T : unmanaged {

	private readonly ChunkMap<T> _map;
	private readonly int _version;
	private Dictionary<long, GridChunkStorage<T>>.ValueCollection.Enumerator _inner;

	public ChunkCursor(
		ChunkMap<T> map
	) {
		_map = map;
		_version = map.Version;
		_inner = map.Chunks.GetEnumerator();
	}

	public readonly ChunkMap<T> Map => _map;

	public readonly GridChunkStorage<T> Current => _inner.Current;

	public bool MoveNext() {
		if( _map.Version != _version ) {
			throw new InvalidOperationException( "The grid topology changed during enumeration." );
		}
		return _inner.MoveNext();
	}
}
