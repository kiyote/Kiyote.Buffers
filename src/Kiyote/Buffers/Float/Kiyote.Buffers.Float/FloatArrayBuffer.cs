namespace Kiyote.Buffers.Float;

internal class FloatArrayBuffer {

	private readonly int _rows;
	private readonly int _columns;
	private readonly float[][] _buffer;

	public FloatArrayBuffer(
		int columns,
		int rows
	) {
		_columns = columns;
		_rows = rows;
		_buffer = new float[ rows ][];
		for (int row = 0; row < rows; row++) {
			_buffer[ row ] = new float[ columns ];
		}
	}

	public float this[int column, int row] {
		get {
			return _buffer[ row ][ column ];
		}
		set {
			_buffer[ row ][ column ] = value;
		}
	}

	public void Add(
		float value
	) {
		for (int row = 0; row < _rows; row++) {
			for (int column = 0; column < _columns; column++) {
				_buffer[ row ][ column ] += value;
			}
		}
	}
}
