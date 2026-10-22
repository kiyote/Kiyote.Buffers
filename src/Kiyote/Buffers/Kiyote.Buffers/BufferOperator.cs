namespace Kiyote.Buffers;

internal sealed class BufferOperator : IBufferOperator {

	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		Func<T, T> op,
		IBuffer<T> output
	) {
		ValidateShape( a, output );
		ArgumentNullException.ThrowIfNull( op );
		int rows = a.Rows;
		int columns = a.Columns;

		for( int r = 0; r < rows; r++ ) {
			Span<T> source = a.GetRowSpan( r );
			Span<T> target = output.GetRowSpan( r );
			for( int c = 0; c < columns; c++ ) {
				target[ c ] = op( source[ c ] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		IBuffer<T> b,
		Func<T, T, T> op,
		IBuffer<T> output
	) {
		ValidateShape( a, b );
		ValidateShape( a, output );
		ArgumentNullException.ThrowIfNull( op );
		int rows = a.Rows;
		int columns = a.Columns;
		for( int r = 0; r < rows; r++ ) {
			Span<T> left = a.GetRowSpan( r );
			Span<T> right = b.GetRowSpan( r );
			Span<T> target = output.GetRowSpan( r );
			for( int c = 0; c < columns; c++ ) {
				target[ c ] = op( left[ c ], right[ c ] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> source,
		Func<int, int, IBuffer<T>, T, T> op,
		IBuffer<T> output
	) {
		ValidateShape( source, output );
		ArgumentNullException.ThrowIfNull( op );
		int rows = source.Rows;
		int columns = source.Columns;

		for( int r = 0; r < rows; r++ ) {
			Span<T> target = output.GetRowSpan( r );
			for( int c = 0; c < columns; c++ ) {
				// The source buffer is handed to the delegate, which may read any
				// cell, so it is indexed directly rather than through a cached span.
				target[ c ] = op( c, r, source, source[ c, r ] );
			}
		}
	}

	void IBufferOperator.Perform<TSource, TOutput>(
		IBuffer<TSource> source,
		Func<int, int, TSource, TOutput> op,
		IBuffer<TOutput> output
	) {
		ValidateShape( source, output );
		ArgumentNullException.ThrowIfNull( op );
		int rows = source.Rows;
		int columns = source.Columns;

		for( int r = 0; r < rows; r++ ) {
			Span<TSource> values = source.GetRowSpan( r );
			Span<TOutput> target = output.GetRowSpan( r );
			for( int c = 0; c < columns; c++ ) {
				target[ c ] = op( c, r, values[ c ] );
			}
		}
	}

	private static void ValidateShape<TSource, TOutput>( IBuffer<TSource> source, IBuffer<TOutput> destination ) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( destination );
		if( source.Rows != destination.Rows || source.Columns != destination.Columns ) {
			throw new InvalidOperationException( "Buffers must have the same dimensions." );
		}
	}
}
