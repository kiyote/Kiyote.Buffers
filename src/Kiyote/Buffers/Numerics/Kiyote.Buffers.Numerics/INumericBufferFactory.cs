using System.Numerics;

namespace Kiyote.Buffers.Numerics;

public interface INumericBufferFactory {

	/// <summary>Creates a numeric buffer with positive logical dimensions.</summary>
	/// <exception cref="ArgumentOutOfRangeException">A dimension is not positive or the padded storage cannot fit in an array.</exception>
	INumericBuffer<T> Create<T>(
		int columns,
		int rows,
		T defaultValue
	) where T : struct, INumber<T>;

}
