namespace Kiyote.Buffers;

// This seems to be marginally more performant than ArrayBuffer
internal sealed class FlatArrayBuffer<T> : IBuffer<T> {

	private readonly T[] _buffer;
	private readonly ISize _size;

	public FlatArrayBuffer(
		ISize size
	) {
		ArgumentNullException.ThrowIfNull( size, nameof( size ) );

		_size = size;
		_buffer = new T[size.Width * size.Height];
	}

	public FlatArrayBuffer(
		ISize size,
		T initialValue
	) {
		ArgumentNullException.ThrowIfNull( size, nameof( size ) );

		_size = size;
		_buffer = new T[size.Width * size.Height];
		Array.Fill( _buffer, initialValue );
	}

	public ISize Size => _size;

	public T this[int column, int row] {
		get {
			int index = (row * _size.Width) + column;
			return _buffer[index];
		}
		set {
			int index = ( row * _size.Width ) + column;
			_buffer[index] = value;
		}
	}
}

