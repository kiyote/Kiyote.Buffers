namespace Kiyote.Buffers;

internal sealed class BufferOperator : IBufferOperator {

	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		Func<T, T> op,
		IBuffer<T> output
	) {
		int rows = a.Size.Height;
		int columns = a.Size.Width;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[c, r] = op( a[c, r] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		IBuffer<T> b,
		Func<T, T, T> op,
		IBuffer<T> output
	) {
		int rows = a.Size.Height;
		int columns = a.Size.Width;
		if( rows != b.Size.Height
			|| columns != b.Size.Width
		) {
			throw new InvalidOperationException( "Operands must be same dimensions." );
		}
		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[c, r] = op( a[c, r], b[c, r] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> source,
		Func<int, int, IBuffer<T>, T, T> op,
		IBuffer<T> output
	) {
		int rows = source.Size.Height;
		int columns = source.Size.Width;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[c, r] = op( c, r, source, source[c, r] );
			}
		}
	}

	void IBufferOperator.Perform<TSource, TOutput>(
		IBuffer<TSource> source,
		Func<int, int, TSource, TOutput> op,
		IBuffer<TOutput> output
	) {
		int rows = source.Size.Height;
		int columns = source.Size.Width;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[c, r] = op( c, r, source[c, r] );
			}
		}
	}
}
