namespace Kiyote.Buffers;

public interface IBufferFactory {

	static IBufferFactory CreateArrayFactory() {
		return new ArrayBufferFactory();
	}

	IBuffer<T> Create<T>(
		int columns,
		int rows,
		T initialValue
	);

}
