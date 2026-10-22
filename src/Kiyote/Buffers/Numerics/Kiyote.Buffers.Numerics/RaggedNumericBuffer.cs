using System.Numerics;

namespace Kiyote.Buffers.Numerics;

public sealed class RaggedNumericBuffer<T> : INumericBuffer<T> where T : struct, INumber<T> {

	private readonly int _columns;
	private readonly int _rows;
	internal readonly T[][] Content;
	private readonly int _allocWidth;
	internal readonly int OpCount;

	public RaggedNumericBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( columns );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( rows );
		int lanes = Vector<T>.Count;
		long paddedWidth = ( (long)columns + lanes - 1 ) / lanes * lanes;
		if( paddedWidth > Array.MaxLength ) {
			throw new ArgumentOutOfRangeException( nameof( columns ), "The padded row exceeds the maximum array length." );
		}
		ArgumentOutOfRangeException.ThrowIfGreaterThan( rows, Array.MaxLength );
		_columns = columns;
		_rows = rows;
		Content = new T[ rows ][];
		_allocWidth = (int)paddedWidth;
		OpCount = _allocWidth / lanes;
		for( int i = 0; i < rows; i++ ) {
			Content[ i ] = new T[ _allocWidth ];
			if (defaultValue != default) {
				Array.Fill( Content[ i ], defaultValue );
			}
		}
	}

	T IBuffer<T>.this[ int column, int row ] {
		get {
			ValidateCoordinates( column, row );
			return Content[ row ][ column ];
		}
		set {
			ValidateCoordinates( column, row );
			Content[ row ][ column ] = value;
		}
	}

	int IBuffer<T>.Columns => _columns;

	int IBuffer<T>.Rows => _rows;

	Span<T> IBuffer<T>.GetRowSpan( int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
		// Rows are allocated wider than the logical column count so they can be
		// processed as whole vectors; the padding must never be exposed.
		return Content[ row ].AsSpan( 0, _columns );
	}

	private void ValidateCoordinates( int column, int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( column );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( column, _columns );
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
	}

}
