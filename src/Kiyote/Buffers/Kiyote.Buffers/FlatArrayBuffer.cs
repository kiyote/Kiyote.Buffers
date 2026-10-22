namespace Kiyote.Buffers;

public sealed class FlatArrayBuffer<T> : IBuffer<T> {

	private readonly T[] _buffer;
	private readonly int _columns;
	private readonly int _rows;

	public FlatArrayBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( columns );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( rows );
		if( (long)columns * rows > Array.MaxLength ) {
			throw new ArgumentOutOfRangeException( nameof( rows ), "The buffer exceeds the maximum array length." );
		}
		_columns = columns;
		_rows = rows;
		_buffer = new T[ columns * rows ];
		Array.Fill( _buffer, defaultValue );
	}

	public int Columns => _columns;

	public int Rows => _rows;

	public T this[ int column, int row ] {
		get {
			ValidateCoordinates( column, row );
			return _buffer[ ( row * _columns ) + column ];
		}
		set {
			ValidateCoordinates( column, row );
			_buffer[ ( row * _columns ) + column ] = value;
		}
	}

	public Span<T> GetRowSpan( int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
		return _buffer.AsSpan( row * _columns, _columns );
	}

	private void ValidateCoordinates( int column, int row ) {
		ArgumentOutOfRangeException.ThrowIfNegative( column );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( column, _columns );
		ArgumentOutOfRangeException.ThrowIfNegative( row );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( row, _rows );
	}
}

