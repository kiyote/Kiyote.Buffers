namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// The current and alternate value buffers of one chunk of a multi-buffered
/// grid (<see cref="DoubleBufferedChunkedGrid{T}"/>, <see cref="TripleBufferedChunkedGrid{T}"/>).
/// <see cref="Current"/> and <see cref="Alternate"/> share the same presence.
/// </summary>
/// <remarks>Buffer roles are captured when the view is created. After a grid swaps,
/// an existing view still addresses the same physical buffers; acquire a new view
/// to observe the new current and alternate roles.</remarks>
public readonly ref struct BufferedChunk<T> where T : unmanaged {

	internal BufferedChunk(
		GridChunk<T> current,
		GridChunk<T> alternate
	) {
		Current = current;
		Alternate = alternate;
	}

	public GridChunk<T> Current { get; }

	public GridChunk<T> Alternate { get; }
}
