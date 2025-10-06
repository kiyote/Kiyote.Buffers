namespace Kiyote.Buffers;

internal sealed class ArrayBufferFactory : IBufferFactory {

	IBuffer<T> IBufferFactory.Create<T>(
		ISize size,
		T initialValue
	) {
		return new ArrayBuffer<T>( size, initialValue );
	}

	IBuffer<T> IBufferFactory.Create<T>(
		int columns,
		int rows,
		T initialValue
	) {
		return new ArrayBuffer<T>( new Point( columns, rows ), initialValue );
	}
}
