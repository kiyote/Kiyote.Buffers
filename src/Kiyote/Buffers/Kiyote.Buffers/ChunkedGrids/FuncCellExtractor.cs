namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Adapts a delegate to <see cref="ICellExtractor{TSource, T}"/> so the
/// delegate-based API can share the generic implementation.
/// </summary>
internal readonly struct FuncCellExtractor<TSource, T> : ICellExtractor<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	private readonly Func<TSource, (bool populated, T value)> _extract;

	public FuncCellExtractor(
		Func<TSource, (bool populated, T value)> extract
	) {
		_extract = extract;
	}

	public bool TryExtract(
		int sourceColumn,
		int sourceRow,
		in TSource source,
		out T value
	) {
		(bool populated, T result) = _extract( source );
		value = result;
		return populated;
	}
}
