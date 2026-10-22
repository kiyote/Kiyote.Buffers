namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Both value buffers of a chunk neighbourhood of a
/// multi-buffered grid. Typically a stencil reads
/// <see cref="Alternate"/> (the previous state) and writes <see cref="Current"/>.
/// </summary>
/// <remarks>Buffer roles are captured when the view is created. After a grid swaps,
/// an existing view still addresses the same physical buffers; acquire a new view
/// to observe the new current and alternate roles.</remarks>
public readonly ref struct BufferedNeighbourhood<T> where T : unmanaged {

	internal BufferedNeighbourhood(
		GridChunkNeighbourhood<T> current,
		GridChunkNeighbourhood<T> alternate
	) {
		Current = current;
		Alternate = alternate;
	}

	public GridChunkNeighbourhood<T> Current { get; }

	public GridChunkNeighbourhood<T> Alternate { get; }
}
