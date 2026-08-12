namespace Kiyote.Buffers;

public sealed class ArrayBuffer<T> : IBuffer<T> {

	private readonly T[][] _buffer;
	private readonly int _columns;
	private readonly int _rows;

	public ArrayBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
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
			return _buffer[ row ][ column ];
		}
		set {
			_buffer[ row ][ column ] = value;
		}
	}
}

