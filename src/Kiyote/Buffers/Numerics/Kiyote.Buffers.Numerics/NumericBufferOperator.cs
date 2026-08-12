using System.Numerics;
using System.Runtime.InteropServices;

namespace Kiyote.Buffers.Numerics;

internal class NumericBufferOperator : INumericBufferOperator {

	void INumericBufferOperator.Clear<T>(
		INumericBuffer<T> source,
		T value
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer ) {
			Vector<T> values = Vector.Create( value );
			int opCount = numericBuffer.OpCount;
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );
				for( int i = 0; i < opCount; i++ ) {
					vcontent[ i ] = values;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					source[ col, row ] = value;
				}
			}
		}
	}

	void INumericBufferOperator.Add<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer ) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vcontent[ i ] += amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					source[ col, row ] += amount;
				}
			}
		}
	}

	void INumericBufferOperator.Subtract<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer ) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vcontent[ i ] -= amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					source[ col, row ] -= amount;
				}
			}
		}
	}

	void INumericBufferOperator.Multiply<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer ) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vcontent[ i ] *= amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					source[ col, row ] *= amount;
				}
			}
		}
	}

	void INumericBufferOperator.Divide<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer ) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vcontent[ i ] /= amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					source[ col, row ] /= amount;
				}
			}
		}
	}

	T INumericBufferOperator.Max<T>(
		INumericBuffer<T> source
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer
			&& numericBuffer.OpCount > 1 // Only use vectorization if we have more than one vector's worth of data
		) {
			int opCount = numericBuffer.OpCount;
			Vector<T> maxes = Vector.Create( source[ 0, 0 ] );
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					maxes = Vector.Max( vcontent[ i ], maxes );
				}
			}

			T max = maxes[ 0 ];
			for( int i = 1; i < Vector<T>.Count; i++ ) {
				if( maxes[ i ] > max ) {
					max = maxes[ i ];
				}
			}
			return max;

		} else {
			int columns = source.Columns;
			T max = source[ 0, 0 ];
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					T value = source[ col, row ];
					if( value > max ) {
						max = value;
					}
				}
			}

			return max;
		}
	}

	T INumericBufferOperator.Min<T>(
		INumericBuffer<T> source
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer
			&& numericBuffer.OpCount > 1 // Only use vectorization if we have more than one vector's worth of data
		) {
			int opCount = numericBuffer.OpCount;
			Vector<T> mins = Vector.Create( source[ 0, 0 ] );
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					mins = Vector.Min( vcontent[ i ], mins );
				}
			}

			T min = mins[ 0 ];
			for( int i = 1; i < Vector<T>.Count; i++ ) {
				if( mins[ i ] < min ) {
					min = mins[ i ];
				}
			}
			return min;

		} else {
			int columns = source.Columns;
			T min = source[ 0, 0 ];
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					T value = source[ col, row ];
					if( value < min ) {
						min = value;
					}
				}
			}

			return min;
		}
	}

	(T min, T max) INumericBufferOperator.MinMax<T>(
		INumericBuffer<T> source
	) {
		int rows = source.Rows;
		if( source is NumericBuffer<T> numericBuffer
			&& numericBuffer.OpCount > 1 // Only use vectorization if we have more than one vector's worth of data
		) {
			int opCount = numericBuffer.OpCount;
			Vector<T> mins = Vector.Create( source[ 0, 0 ] );
			Vector<T> maxs = mins;
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					Vector<T> values = vcontent[ i ];
					mins = Vector.Min( values, mins );
					maxs = Vector.Max( values, maxs );
				}
			}

			T min = mins[ 0 ];
			T max = maxs[ 0 ];
			for( int i = 1; i < Vector<T>.Count; i++ ) {
				if( mins[ i ] < min ) {
					min = mins[ i ];
				}
				if( maxs[ i ] > max ) {
					max = maxs[ i ];
				}
			}
			return (min, max);

		} else {
			int columns = source.Columns;
			T min = source[ 0, 0 ];
			T max = min;
			for( int row = 0; row < rows; row++ ) {
				for( int col = 0; col < columns; col++ ) {
					T value = source[ col, row ];
					if( value < min ) {
						min = value;
					}
					if( value > max ) {
						max = value;
					}
				}
			}

			return (min, max);
		}
	}

	void INumericBufferOperator.Normalize<T>(
		INumericBuffer<T> source
	) {
		INumericBufferOperator op = this;
		(T min, T max) = op.MinMax( source );
		T range = max - min;
		if (range == T.Zero) {
			return;
		}
		op.Subtract( source, min );
		op.Divide( source, range );
	}


	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		Func<T, T> op,
		IBuffer<T> output
	) {
		int rows = a.Rows;
		int columns = a.Columns;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[ c, r ] = op( a[ c, r ] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> a,
		IBuffer<T> b,
		Func<T, T, T> op,
		IBuffer<T> output
	) {
		int rows = a.Rows;
		int columns = a.Columns;
		if( rows != b.Rows
			|| columns != b.Columns
		) {
			throw new InvalidOperationException( "Operands must be same dimensions." );
		}
		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[ c, r ] = op( a[ c, r ], b[ c, r ] );
			}
		}
	}

	void IBufferOperator.Perform<T>(
		IBuffer<T> source,
		Func<int, int, IBuffer<T>, T, T> op,
		IBuffer<T> output
	) {
		int rows = source.Rows;
		int columns = source.Columns;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[ c, r ] = op( c, r, source, source[ c, r ] );
			}
		}
	}

	void IBufferOperator.Perform<TSource, TOutput>(
		IBuffer<TSource> source,
		Func<int, int, TSource, TOutput> op,
		IBuffer<TOutput> output
	) {
		int rows = source.Rows;
		int columns = source.Columns;

		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				output[ c, r ] = op( c, r, source[ c, r ] );
			}
		}
	}
}
