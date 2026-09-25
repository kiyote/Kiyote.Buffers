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
		_columns = columns;
		_rows = rows;
		_buffer = new T[ columns * rows ];
		Array.Fill( _buffer, defaultValue );
	}

	public int Columns => _columns;

	public int Rows => _rows;

	public T this[ int column, int row ] {
		get {
			return _buffer[ ( row * _columns ) + column ];
		}
		set {
			_buffer[ ( row * _columns ) + column ] = value;
		}
	}

	public Span<T> GetRowSpan( int row ) {
		return _buffer.AsSpan( row * _columns, _columns );
	}
}

