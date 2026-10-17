namespace Kiyote.Buffers.Analyzers;

/// <summary>
/// Accumulates every visited cell into a caller supplied list.
/// </summary>
internal readonly struct ListCellVisitor<T> : ICellVisitor<T> {

	private readonly List<BufferCell<T>> _cells;

	public ListCellVisitor(
		List<BufferCell<T>> cells
	) {
		_cells = cells;
	}

	public readonly void Visit(
		int column,
		int row,
		T value
	) {
		_cells.Add( new BufferCell<T>( column, row, value ) );
	}
}
