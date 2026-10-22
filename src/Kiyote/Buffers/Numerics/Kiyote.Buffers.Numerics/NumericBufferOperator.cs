using System.Numerics;
using System.Runtime.InteropServices;

namespace Kiyote.Buffers.Numerics;

internal class NumericBufferOperator : INumericBufferOperator {

	void INumericBufferOperator.Clear<T>(
		INumericBuffer<T> source,
		T value
	) {
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer ) {
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
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					content[ col ] = value;
				}
			}
		}
	}

	void INumericBufferOperator.Add<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer ) {
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
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					content[ col ] += amount;
				}
			}
		}
	}

	void INumericBufferOperator.Add<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) {
		ValidateShape( source, destination );
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& destination is RaggedNumericBuffer<T> numericDestination
		) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vsource = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );
				Span<Vector<T>> vdestination = MemoryMarshal.Cast<T, Vector<T>>( numericDestination.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vdestination[ i ] = vsource[ i ] + amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				Span<T> target = destination.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					target[ col ] = content[ col ] + amount;
				}
			}
		}
	}

	void INumericBufferOperator.Subtract<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer ) {
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
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					content[ col ] -= amount;
				}
			}
		}
	}

	void INumericBufferOperator.Subtract<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) {
		ValidateShape( source, destination );
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& destination is RaggedNumericBuffer<T> numericDestination
		) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vsource = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );
				Span<Vector<T>> vdestination = MemoryMarshal.Cast<T, Vector<T>>( numericDestination.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vdestination[ i ] = vsource[ i ] - amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				Span<T> target = destination.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					target[ col ] = content[ col ] - amount;
				}
			}
		}
	}

	void INumericBufferOperator.Multiply<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer ) {
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
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					content[ col ] *= amount;
				}
			}
		}
	}

	void INumericBufferOperator.Multiply<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) {
		ValidateShape( source, destination );
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& destination is RaggedNumericBuffer<T> numericDestination
		) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vsource = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );
				Span<Vector<T>> vdestination = MemoryMarshal.Cast<T, Vector<T>>( numericDestination.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vdestination[ i ] = vsource[ i ] * amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				Span<T> target = destination.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					target[ col ] = content[ col ] * amount;
				}
			}
		}
	}

	void INumericBufferOperator.Divide<T>(
		INumericBuffer<T> source,
		T amount
	) {
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer ) {
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
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					content[ col ] /= amount;
				}
			}
		}
	}

	void INumericBufferOperator.Divide<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) {
		ValidateShape( source, destination );
		int rows = source.Rows;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& destination is RaggedNumericBuffer<T> numericDestination
		) {
			Vector<T> amounts = Vector.Create( amount );
			int opCount = numericBuffer.OpCount;

			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vsource = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );
				Span<Vector<T>> vdestination = MemoryMarshal.Cast<T, Vector<T>>( numericDestination.Content[ row ].AsSpan() );

				for( int i = 0; i < opCount; i++ ) {
					vdestination[ i ] = vsource[ i ] / amounts;
				}
			}
		} else {
			int columns = source.Columns;
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				Span<T> target = destination.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					target[ col ] = content[ col ] / amount;
				}
			}
		}
	}

	T INumericBufferOperator.Max<T>(
		INumericBuffer<T> source
	) {
		int rows = source.Rows;
		int columns = source.Columns;
		int vectorCount = columns / Vector<T>.Count;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& vectorCount > 0 // Only use vectorization if we have at least one whole vector of real data
		) {
			int tailStart = vectorCount * Vector<T>.Count;
			Vector<T> maxes = Vector.Create( source[ 0, 0 ] );
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < vectorCount; i++ ) {
					maxes = Vector.Max( vcontent[ i ], maxes );
				}
			}

			T max = maxes[ 0 ];
			for( int i = 1; i < Vector<T>.Count; i++ ) {
				if( maxes[ i ] > max ) {
					max = maxes[ i ];
				}
			}

			// The columns beyond the last whole vector share their storage with
			// alignment padding, so they must be visited individually.
			for( int row = 0; row < rows; row++ ) {
				for( int col = tailStart; col < columns; col++ ) {
					T value = source[ col, row ];
					if( value > max ) {
						max = value;
					}
				}
			}
			return max;

		} else {
			T max = source[ 0, 0 ];
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					T value = content[ col ];
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
		int columns = source.Columns;
		int vectorCount = columns / Vector<T>.Count;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& vectorCount > 0 // Only use vectorization if we have at least one whole vector of real data
		) {
			int tailStart = vectorCount * Vector<T>.Count;
			Vector<T> mins = Vector.Create( source[ 0, 0 ] );
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < vectorCount; i++ ) {
					mins = Vector.Min( vcontent[ i ], mins );
				}
			}

			T min = mins[ 0 ];
			for( int i = 1; i < Vector<T>.Count; i++ ) {
				if( mins[ i ] < min ) {
					min = mins[ i ];
				}
			}

			// The columns beyond the last whole vector share their storage with
			// alignment padding, so they must be visited individually.
			for( int row = 0; row < rows; row++ ) {
				for( int col = tailStart; col < columns; col++ ) {
					T value = source[ col, row ];
					if( value < min ) {
						min = value;
					}
				}
			}
			return min;

		} else {
			T min = source[ 0, 0 ];
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					T value = content[ col ];
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
		int columns = source.Columns;
		int vectorCount = columns / Vector<T>.Count;
		if( source is RaggedNumericBuffer<T> numericBuffer
			&& vectorCount > 0 // Only use vectorization if we have at least one whole vector of real data
		) {
			int tailStart = vectorCount * Vector<T>.Count;
			Vector<T> mins = Vector.Create( source[ 0, 0 ] );
			Vector<T> maxs = mins;
			Vector<T> valid = Vector.Equals( mins, mins );
			for( int row = 0; row < rows; row++ ) {
				Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( numericBuffer.Content[ row ].AsSpan() );

				for( int i = 0; i < vectorCount; i++ ) {
					Vector<T> values = vcontent[ i ];
					valid &= Vector.Equals( values, values );
					mins = Vector.Min( values, mins );
					maxs = Vector.Max( values, maxs );
				}
			}
			if( !Vector.EqualsAll( valid, Vector.Equals( Vector<T>.Zero, Vector<T>.Zero ) ) ) {
				for( int row = 0; row < rows; row++ ) {
					for( int col = 0; col < tailStart; col++ ) {
						T value = source[ col, row ];
						if( T.IsNaN( value ) ) {
							return (value, value);
						}
					}
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

			// The columns beyond the last whole vector share their storage with
			// alignment padding, so they must be visited individually.
			for( int row = 0; row < rows; row++ ) {
				for( int col = tailStart; col < columns; col++ ) {
					T value = source[ col, row ];
					if( T.IsNaN( value ) ) {
						return (value, value);
					}
					if( value < min ) {
						min = value;
					}
					if( value > max ) {
						max = value;
					}
				}
			}
			return (min, max);

		} else {
			T min = source[ 0, 0 ];
			T max = min;
			if( T.IsNaN( min ) ) {
				return (min, min);
			}
			for( int row = 0; row < rows; row++ ) {
				Span<T> content = source.GetRowSpan( row );
				for( int col = 0; col < columns; col++ ) {
					T value = content[ col ];
					if( T.IsNaN( value ) ) {
						return (value, value);
					}
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
		ValidateFiniteRange( min, max );
		if( min == max ) {
			op.Clear( source, T.Zero );
			return;
		}
		if( T.One / T.CreateChecked( 2 ) == T.Zero ) {
			NormalizeIntegral( source, source, max );
			return;
		}
		T range = max - min;
		if( !T.IsFinite( range ) ) {
			NormalizeWide( source, source, min, max );
			return;
		}
		op.Subtract( source, min );
		op.Divide( source, range );
	}

	void INumericBufferOperator.Normalize<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination
	) {
		ValidateShape( source, destination );
		INumericBufferOperator op = this;
		(T min, T max) = op.MinMax( source );
		ValidateFiniteRange( min, max );
		if( min == max ) {
			op.Clear( destination, T.Zero );
			return;
		}
		if( T.One / T.CreateChecked( 2 ) == T.Zero ) {
			NormalizeIntegral( source, destination, max );
			return;
		}
		T range = max - min;
		if( !T.IsFinite( range ) ) {
			NormalizeWide( source, destination, min, max );
			return;
		}
		op.Subtract( source, destination, min );
		op.Divide( destination, range );
	}

	void INumericBufferOperator.ScaleToRange<TSource, TDestination>(
		INumericBuffer<TSource> source,
		INumericBuffer<TDestination> destination
	) {
		ValidateShape( source, destination );
		INumericBufferOperator op = this;
		(TSource min, TSource max) = op.MinMax( source );
		ValidateFiniteRange( min, max );
		if( min == max ) {
			op.Clear( destination, TDestination.Zero );
			return;
		}

		double doubleMin = double.CreateChecked( min );
		double doubleRange = double.CreateChecked( max ) - doubleMin;
		if( !double.IsFinite( doubleRange ) || doubleRange == 0 ) {
			throw new OverflowException( "The source range cannot be represented for scaling." );
		}
		double doubleDestinationMin = double.CreateChecked( TDestination.MinValue );
		double doubleDestinationMax = double.CreateChecked( TDestination.MaxValue );
		double doubleDestinationRange = doubleDestinationMax - doubleDestinationMin;
		if( !double.IsFinite( doubleDestinationRange ) ) {
			throw new OverflowException( "The destination range cannot be represented for scaling." );
		}
		int rows = source.Rows;
		int columns = source.Columns;
		for( int row = 0; row < rows; row++ ) {
			Span<TSource> content = source.GetRowSpan( row );
			Span<TDestination> target = destination.GetRowSpan( row );
			for( int col = 0; col < columns; col++ ) {
				double normalized = ( ( double.CreateChecked( content[ col ] ) - doubleMin ) / doubleRange * doubleDestinationRange ) + doubleDestinationMin;
				target[ col ] = TDestination.CreateSaturating( normalized );
			}
		}
	}

	private static void ValidateFiniteRange<T>( T min, T max ) where T : struct, INumber<T> {
		if( !T.IsFinite( min ) || !T.IsFinite( max ) ) {
			throw new ArgumentException( "The source must contain only finite numeric values." );
		}
	}

	private static void NormalizeIntegral<T>( INumericBuffer<T> source, INumericBuffer<T> destination, T max ) where T : struct, INumber<T> {
		for( int row = 0; row < source.Rows; row++ ) {
			Span<T> values = source.GetRowSpan( row );
			Span<T> target = destination.GetRowSpan( row );
			for( int col = 0; col < source.Columns; col++ ) {
				target[ col ] = values[ col ] == max ? T.One : T.Zero;
			}
		}
	}

	private static void NormalizeWide<T>( INumericBuffer<T> source, INumericBuffer<T> destination, T min, T max ) where T : struct, INumber<T> {
		double minimum = double.CreateChecked( min );
		double range = double.CreateChecked( max ) - minimum;
		if( !double.IsFinite( range ) || range == 0 ) {
			throw new OverflowException( "The source range cannot be represented for normalization." );
		}
		for( int row = 0; row < source.Rows; row++ ) {
			Span<T> values = source.GetRowSpan( row );
			Span<T> target = destination.GetRowSpan( row );
			for( int col = 0; col < source.Columns; col++ ) {
				target[ col ] = T.CreateSaturating( ( double.CreateChecked( values[ col ] ) - minimum ) / range );
			}
		}
	}


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
