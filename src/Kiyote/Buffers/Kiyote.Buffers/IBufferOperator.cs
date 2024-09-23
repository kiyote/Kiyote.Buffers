namespace Kiyote.Buffers;

public interface IBufferOperator {
	/// <summary>
	/// Performs the supplied <paramref name="op"/> on each value in the
	/// <paramref name="source"/> buffer, storing the result in the
	/// <paramref name="output"/> buffer.
	/// </summary>
	/// <typeparam name="T">The type of each value in the buffer.</typeparam>
	/// <param name="source">The buffer to be operated upon.</param>
	/// <param name="op">The function to be called to transform each value.</param>
	/// <param name="output">The buffer to receive the resulting value.</param>
	/// <remarks>
	/// <paramref name="op" /> is a function that takes the value from the buffer and returns the desired value.
	/// </remarks>
	void Perform<T>(
		IBuffer<T> source,
		Func<T, T> op,
		IBuffer<T> output
	);

	/// <summary>
	/// Performs the supplied <paramref name="op"/> on the correspond pair of 
	/// values from <paramref name="source1"/> and <paramref name="source2"/>,
	/// storing the result in the <paramref name="output"/> buffer.
	/// </summary>
	/// <typeparam name="T">The type of each value in the buffer.</typeparam>
	/// <param name="source1">Supplies the first value to the <paramref name="op"/> function.</param>
	/// <param name="source2">Supplies the second value to the <paramref name="op"/> function.</param>
	/// <param name="output">The buffer to receive the resulting value.</param>
	/// <remarks>
	/// <paramref name="op" /> is function that takes a value from each source buffer and returns the desired value.
	/// </remarks>
	void Perform<T>(
		IBuffer<T> source1,
		IBuffer<T> source2,
		Func<T, T, T> op,
		IBuffer<T> output
	);

	/// <summary>
	/// Performs the supplied <paramref name="op"/> on the values contained
	/// in <paramref name="source"/>, storing the result in <paramref name="output"/>.
	/// </summary>
	/// <typeparam name="T">The type of each value in the buffer.</typeparam>
	/// <param name="source">The buffer to be operated upon.</param>
	/// <param name="op">The function to be called to transform each value.</param>
	/// <param name="output">The buffer to receive the resulting value.</param>
	/// <remarks>
	/// <paramref name="op"/> is a function that receives the column and row of the value being
	/// operated upon, the buffer from which that value came and the value itself
	/// that is being operated upon, and returns the new value for that column
	/// and row pair.
	/// </remarks>
	void Perform<T>(
		IBuffer<T> source,
		Func<int, int, IBuffer<T>, T, T> op,
		IBuffer<T> output
	);

	/// <summary>
	/// Performs the supplied <paramref name="op"/> on the values contained
	/// in <paramref name="source"/>, storing the result in <paramref name="output"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of each value in the source buffer.</typeparam>
	/// <typeparam name="TOutput">The type of each value in the output buffer.</typeparam>
	/// <param name="source">The buffer to be operated upon.</param>
	/// <param name="op">The function to be called for each value in the buffer.</param>
	/// <param name="output">The buffer to receive the resulting value.</param>
	/// <remarks>
	/// <paramref name="op"/> is a function that receives the column and row of
	/// the value, as well as the value at that location for each point in the buffer
	/// and returns the new value to be stored at that location in the output.
	/// </remarks>
	void Perform<TSource, TOutput>(
		IBuffer<TSource> source,
		Func<int, int, TSource, TOutput> op,
		IBuffer<TOutput> output
	);
}
