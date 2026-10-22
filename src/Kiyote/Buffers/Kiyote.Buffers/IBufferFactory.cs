namespace Kiyote.Buffers;

public interface IBufferFactory {

	/// <summary>Creates a buffer with positive logical dimensions.</summary>
	/// <exception cref="ArgumentOutOfRangeException">A dimension is not positive or the storage cannot fit in an array.</exception>
	IBuffer<T> Create<T>(
		int columns,
		int rows,
		T initialValue
	);

}
