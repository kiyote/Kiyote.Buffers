using System.Numerics;

namespace Kiyote.Buffers.Numerics; 

public interface INumericBufferOperator : IBufferOperator {

	void Add<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Add<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) where T : struct, INumber<T>;

	void Subtract<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Subtract<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) where T : struct, INumber<T>;

	void Multiply<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Multiply<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
		T amount
	) where T : struct, INumber<T>;

	void Divide<T>(
		INumericBuffer<T> source,
		T amount
	) where T : struct, INumber<T>;

	void Divide<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination,
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

	void Normalize<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination
	) where T : struct, INumber<T>;

	void ScaleToRange<TSource, TDestination>(
		INumericBuffer<TSource> source,
		INumericBuffer<TDestination> destination
	)
		where TSource : struct, INumber<TSource>
		where TDestination : struct, INumber<TDestination>, IMinMaxValue<TDestination>;

	void Clear<T>(
		INumericBuffer<T> source,
		T value
	) where T : struct, INumber<T>;
}
