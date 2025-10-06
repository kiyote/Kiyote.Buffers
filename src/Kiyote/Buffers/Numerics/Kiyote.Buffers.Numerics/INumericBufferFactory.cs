using System.Numerics;

namespace Kiyote.Buffers.Numerics;

public interface INumericBufferFactory {

	INumericBuffer<T> Create<T>(
		int columns,
		int rows,
		T defaultValue
	) where T : struct, INumber<T>;

}
