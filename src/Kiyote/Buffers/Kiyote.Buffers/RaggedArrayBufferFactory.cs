namespace Kiyote.Buffers;

internal sealed class RaggedArrayBufferFactory : IBufferFactory {

	IBuffer<T> IBufferFactory.Create<T>(
		int columns,
		int rows,
		T initialValue
	) {
		return new RaggedArrayBuffer<T>( columns, rows, initialValue );
	}
}
