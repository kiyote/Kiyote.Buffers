namespace Kiyote.Buffers.Numerics;

internal class RaggedNumericBufferFactory : INumericBufferFactory {

	INumericBuffer<T> INumericBufferFactory.Create<T>(
		int columns,
		int rows,
		T defaultValue
	) {
		return new RaggedNumericBuffer<T>( columns, rows, defaultValue );
	}

}
