namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Accepts every populated source cell; used when only the source's presence
/// matters, such as when detaching a grid of a different element type.
/// </summary>
internal readonly struct AllCellExtractor<TSource, T> : ICellExtractor<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	public bool TryExtract(
		int sourceColumn,
		int sourceRow,
		in TSource source,
		out T value
	) {
		value = default;
		return true;
	}
}
