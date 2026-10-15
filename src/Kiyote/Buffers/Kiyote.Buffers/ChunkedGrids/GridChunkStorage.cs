namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Backing storage for a single chunk of a chunked grid.
/// </summary>
/// <remarks>
/// <para>
/// A chunk has one <see cref="Presence"/> mask and one or more value buffers.
/// Every buffer shares the presence mask, so all buffers of a chunk always
/// have identical topology.
/// </para>
/// <para>
/// Chunks are long-lived and their arrays are exposed to callers as spans
/// through <see cref="GridChunk{T}"/>, so they are allocated rather than
/// rented. A span that outlives its chunk then refers to orphaned memory
/// instead of an array the pool has handed to someone else.
/// </para>
/// </remarks>
internal sealed class GridChunkStorage<T> where T : unmanaged {

	public GridChunkStorage(
		int chunkColumn,
		int chunkRow,
		int area,
		int presenceWords,
		int bufferCount
	) {
		ChunkColumn = chunkColumn;
		ChunkRow = chunkRow;
		Buffers = new T[ bufferCount ][];
		for( int i = 0; i < bufferCount; i++ ) {
			Buffers[ i ] = new T[ area ];
		}
		Presence = new ulong[ presenceWords ];
	}

	public int ChunkColumn { get; }

	public int ChunkRow { get; }

	public T[][] Buffers { get; }

	public ulong[] Presence { get; }
}
