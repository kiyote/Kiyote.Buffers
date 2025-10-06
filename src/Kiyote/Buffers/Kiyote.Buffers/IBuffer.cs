namespace Kiyote.Buffers;

public interface IBuffer<T> {

	ISize Size { get; }

	T this[int column, int row] { get; set; }
}
