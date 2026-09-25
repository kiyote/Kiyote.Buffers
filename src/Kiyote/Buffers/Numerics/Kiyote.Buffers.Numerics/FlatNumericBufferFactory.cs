namespace Kiyote.Buffers.Numerics;

internal class FlatNumericBufferFactory : INumericBufferFactory {

	INumericBuffer<T> INumericBufferFactory.Create<T>(
		int columns,
		int rows,
		T defaultValue
	) {
		return new FlatNumericBuffer<T>( columns, rows, defaultValue );
	}

}
