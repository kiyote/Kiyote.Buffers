namespace Kiyote.Buffers.Numerics;

internal class NumericBufferFactory : INumericBufferFactory {

	INumericBuffer<T> INumericBufferFactory.Create<T>(
		int columns,
		int rows,
		T defaultValue
	) {
		return new NumericBuffer<T>( columns, rows, defaultValue );
	}

}
