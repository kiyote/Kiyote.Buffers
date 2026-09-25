namespace Kiyote.Buffers;

internal sealed class FlatArrayBufferFactory : IBufferFactory {

	IBuffer<T> IBufferFactory.Create<T>(
		int columns,
		int rows,
		T initialValue
	) {
		return new FlatArrayBuffer<T>( columns, rows, initialValue );
	}
}
