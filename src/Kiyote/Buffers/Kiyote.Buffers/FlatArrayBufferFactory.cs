namespace Kiyote.Buffers;

internal sealed class FlatArrayBufferFactory : IBufferFactory {

	IBuffer<T> IBufferFactory.Create<T>(
		ISize size,
		T initialValue
	) {
		FlatArrayBuffer<T> buffer = new FlatArrayBuffer<T>( size, initialValue );
		for( int r = 0; r < size.Height; r++ ) {
			for( int c = 0; c < size.Width; c++ ) {
				buffer[c, r] = initialValue;
			}
		}

		return buffer;
	}

	IBuffer<T> IBufferFactory.Create<T>(
		ISize size
	) {
		return new FlatArrayBuffer<T>( size );
	}

	IBuffer<T> IBufferFactory.Create<T>(
		int columns,
		int rows
	) {
		return new FlatArrayBuffer<T>( new Point( columns, rows ) );
	}
}
