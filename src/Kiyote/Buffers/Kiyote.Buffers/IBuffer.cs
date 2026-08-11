namespace Kiyote.Buffers;

public interface IBuffer<T> {

	int Columns { get; }

	int Rows { get; }

	T this[int column, int row] { get; set; }
}
