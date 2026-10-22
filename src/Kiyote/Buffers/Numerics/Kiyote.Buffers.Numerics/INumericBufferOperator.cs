using System.Numerics;

namespace Kiyote.Buffers.Numerics; 

/// <summary>Performs numeric operations on buffers.</summary>
/// <remarks>Destination-taking operations require the same logical dimensions as the source and
/// throw <see cref="InvalidOperationException"/> before writing if they differ.</remarks>
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

	/// <summary>Finds the logical minimum and maximum; returns NaN for both if any
	/// source value is NaN.</summary>
	(T min, T max) MinMax<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;

	/// <summary>Maps finite floating-point values to [0, 1]. Integral types map the
	/// minimum and intermediate values to 0 and the maximum to 1.</summary>
	/// <remarks>A constant source is cleared to zero. Nonfinite inputs throw
	/// <see cref="ArgumentException"/> before any writes. Nonrepresentable ranges
	/// throw <see cref="OverflowException"/>. Buffers must be nonempty.</remarks>
	void Normalize<T>(
		INumericBuffer<T> source
	) where T : struct, INumber<T>;

	/// <inheritdoc cref="Normalize{T}(INumericBuffer{T})"/>
	/// <remarks>The source remains unchanged unless it is also the destination.</remarks>
	void Normalize<T>(
		INumericBuffer<T> source,
		INumericBuffer<T> destination
	) where T : struct, INumber<T>;

	/// <summary>Maps source values into the destination type's full range using
	/// double intermediate arithmetic and saturating conversion to the destination type.</summary>
	/// <remarks>A constant source clears the destination to zero. Nonfinite inputs throw
	/// <see cref="ArgumentException"/> before writing. Ranges not representable as
	/// finite doubles throw <see cref="OverflowException"/>; large integral values
	/// may lose precision when converted to double. Buffers must be nonempty.</remarks>
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
