namespace Kiyote.Buffers;

internal sealed class ArrayBuffer<T>: IBuffer<T> {

	private readonly T[][] _buffer;
	private readonly ISize _size;

	public ArrayBuffer(
		ISize size
	) {
		ArgumentNullException.ThrowIfNull( size, nameof( size ) );

		_size = size;
		_buffer = new T[size.Height][];
		for( int r = 0; r < size.Height; r++ ) {
			_buffer[r] = new T[size.Width];
		}
	}

	public ISize Size => _size;

	public T this[int column, int row] {
		get {
			return _buffer[row][column];
		}
		set {
			_buffer[row][column] = value;
		}
	}
}

