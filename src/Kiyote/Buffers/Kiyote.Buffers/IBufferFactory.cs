namespace Kiyote.Buffers;

public interface IBufferFactory {

	IBuffer<T> Create<T>(
		ISize size,
		T initialValue
	);

	IBuffer<T> Create<T>(
		ISize size
	);

	IBuffer<T> Create<T>(
		int columns,
		int rows
	);
}
