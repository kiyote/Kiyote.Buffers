namespace Kiyote.Buffers;

public interface IBuffer<T> {

	int Columns { get; }

	int Rows { get; }

	T this[int column, int row] { get; set; }

	/// <summary>
	/// Returns the contents of the specified row as a span of exactly
	/// <see cref="Columns"/> elements, allowing direct read and write access
	/// to the underlying storage.
	/// </summary>
	Span<T> GetRowSpan( int row );
}
