namespace Kiyote.Buffers;

public sealed class RaggedArrayBuffer<T> : IBuffer<T> {

	private readonly T[][] _buffer;
	private readonly int _columns;
	private readonly int _rows;

	public RaggedArrayBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( columns );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( rows );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( columns, Array.MaxLength );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( rows, Array.MaxLength );
		_columns = columns;
		_rows = rows;
		_buffer = new T[ rows ][];
		for( int r = 0; r < rows; r++ ) {
			_buffer[ r ] = new T[ columns ];
			Array.Fill( _buffer[ r ], defaultValue );
		}
	}

	public int Columns => _columns;

	public int Rows => _rows;

	public T this[ int column, int row ] {
		get {
			ValidateCoordinates( column, row );
			return _buffer[ row ][ column ];
		}
		set {
			ValidateCoordinates( column, row );
			_buffer[ row ][ column ] = value;
		}
	}

	public Span<T> GetRowSpan( int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
		return _buffer[ row ].AsSpan();
	}

	private void ValidateCoordinates( int column, int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( column );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( column, _columns );
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
	}
}

