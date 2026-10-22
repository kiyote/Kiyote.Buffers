namespace Kiyote.Buffers;

public interface IBuffer<T> {

	int Columns { get; }

	int Rows { get; }

	/// <summary>Gets or sets a cell at valid logical coordinates.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The column or row is outside the buffer.</exception>
	T this[int column, int row] { get; set; }

	/// <summary>
	/// Returns the contents of the specified row as a span of exactly
	/// <see cref="Columns"/> elements, allowing direct read and write access
	/// to the underlying storage.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The row is outside the buffer.</exception>
	Span<T> GetRowSpan( int row );
}
