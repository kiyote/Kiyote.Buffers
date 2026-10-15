namespace Kiyote.Buffers.ChunkedGrids;

internal readonly struct IdentityCellExtractor<T> : ICellExtractor<T, T> where T : unmanaged {

	public bool TryExtract(
		int sourceColumn,
		int sourceRow,
		in T source,
		out T value
	) {
		value = source;
		return true;
	}
}
