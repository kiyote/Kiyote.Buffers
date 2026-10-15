namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Selects one of the two value buffers of a <see cref="DoubleBufferedChunkedGrid{T}"/>.
/// </summary>
public enum GridBuffer {
	Current = 0,
	Alternate = 1
}
