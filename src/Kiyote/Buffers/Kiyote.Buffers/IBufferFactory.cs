namespace Kiyote.Buffers;

public interface IBufferFactory {

	IBuffer<T> Create<T>(
		int columns,
		int rows,
		T initialValue
	);

}
