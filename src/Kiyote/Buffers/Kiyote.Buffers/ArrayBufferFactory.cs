namespace Kiyote.Buffers;

internal sealed class ArrayBufferFactory : IBufferFactory {

	IBuffer<T> IBufferFactory.Create<T>(
		ISize size,
		T initialValue
	) {
		ArrayBuffer<T> buffer = new ArrayBuffer<T>( size );
		for( int r = 0; r < size.Height; r++ ) {
			for (int c = 0; c < size.Width; c++) {
				buffer[c, r] = initialValue;
			}
		}

		return buffer;
	}

	IBuffer<T> IBufferFactory.Create<T>(
		ISize size
	) {
		return new ArrayBuffer<T>( size );
	}

	IBuffer<T> IBufferFactory.Create<T>(
		int columns,
		int rows
	) {
		return new ArrayBuffer<T>( new Point( columns, rows ) );
	}
}
