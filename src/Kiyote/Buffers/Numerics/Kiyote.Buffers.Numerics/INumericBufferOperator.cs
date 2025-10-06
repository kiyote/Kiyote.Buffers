using System.Numerics;

namespace Kiyote.Buffers.Numerics; 

public interface INumericBufferOperator : IBufferOperator {

	void Add<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Subtract<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Multiply<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Divide<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	T Max<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;

	T Min<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;

	(T min, T max) MinMax<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;

	void Normalize<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;
}
