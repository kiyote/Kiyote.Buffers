
namespace Kiyote.Buffers.Analyzers;

/// <summary>
/// Discards every visited cell; used when only the sealed/unsealed result matters.
/// </summary>
internal readonly struct NullCellVisitor<T> : ICellVisitor<T> {

	public readonly void Visit(
		int column,
		int row,
		T value
	) {	}
}
