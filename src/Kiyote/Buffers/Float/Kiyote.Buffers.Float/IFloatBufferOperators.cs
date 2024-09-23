namespace Kiyote.Buffers.Float;

public interface IFloatBufferOperators {

	/// <summary>
	/// Rescales the input <paramref name="buffer"/> to have values between
	/// the supplied <paramref name="minimum"/> and <paramref name="maximum"/>.
	/// </summary>
	/// <param name="buffer">The buffer containing the values to be scaled.</param>
	/// <param name="minimum">The lowest resulting value in <paramref name="buffer"/>.</param>
	/// <param name="maximum">The highest resulting value in <paramref name="buffer"/>.</param>
	void Normalize(
		IBuffer<float> buffer,
		float minimum,
		float maximum
	);

	/// <summary>
	/// Rescales the <paramref name="input"/> into <paramref name="output"/>
	/// where the resulting values are between the supplied
	/// <paramref name="minimum"/> and <paramref name="maximum"/>.
	/// </summary>
	/// <param name="input">The buffer containing the values to be scaled.</param>
	/// <param name="minimum">The lowest resulting value in <paramref name="output"/>.</param>
	/// <param name="maximum">The highest resulting value in <paramref name="output"/>.</param>
	/// <param name="output">The buffer to receive the scaled values.</param>
	void Normalize(
		IBuffer<float> input,
		float minimum,
		float maximum,
		IBuffer<float> output
	);


	/// <summary>
	/// Adds a constant value to each entry in the <paramref name="buffer" />.
	/// </summary>
	/// <param name="buffer">The buffer to be added to.</param>
	/// <param name="amount">The amount to add to each value.</param>
	void Add(
		IBuffer<float> buffer,
		float amount
	);

	/// <summary>
	/// Adds a constant value to each entry in <paramref name="input"/> storing
	/// the result in <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be added.</param>
	/// <param name="amount">The amount to add to each value.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Add(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	);


	/// <summary>
	/// Subtracts a constant value to each entry in the <paramref name="buffer" />.
	/// </summary>
	/// <param name="buffer">The buffer to be subtracted from.</param>
	/// <param name="amount">The amount to subtract from each value.</param>
	void Subtract(
		IBuffer<float> buffer,
		float amount
	);

	/// <summary>
	/// Subtracts a constant value to each entry in <paramref name="input"/> storing
	/// the result in <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be subtracted from.</param>
	/// <param name="amount">The amount to subtract from each value.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Subtract(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	);


	/// <summary>
	/// Adds an amount from <paramref name="amounts"/> to each entry in the <paramref name="buffer" />.
	/// </summary>
	/// <param name="buffer">The buffer to be added to.</param>
	/// <param name="amounts">The source of the amount to add to each value.</param>
	void Add(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	);

	/// <summary>
	/// Adds a constant value to each entry in <paramref name="input"/> storing
	/// the result in <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be added.</param>
	/// <param name="amounts">The amount to add to each value.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Add(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	);


	/// <summary>
	/// Subtracts an amount from <paramref name="amounts"/> from each entry in the <paramref name="buffer" />.
	/// </summary>
	/// <param name="buffer">The buffer to be subtracted from.</param>
	/// <param name="amounts">The source of the amount to subtract from each value.</param>
	void Subtract(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	);

	/// <summary>
	/// Subtracts an amount from <paramref name="amounts"/> from <paramref name="input"/> storing
	/// the result in <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be subtracted from.</param>
	/// <param name="amounts">The source of the amount to subtract from each value.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Subtract(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	);


	/// <summary>
	/// Multiplies a value from <paramref name="buffer"/> against a constant
	/// <paramref name="amount"/>.
	/// </summary>
	/// <param name="buffer">The values to be multiplied.</param>
	/// <param name="amount">The amount to multiply by.</param>
	void Multiply(
		IBuffer<float> buffer,
		float amount
	);

	/// <summary>
	/// Multiplies a value from <paramref name="input"/> against a constant
	/// <paramref name="amount"/> and stores the result in
	/// <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be multiplied.</param>
	/// <param name="amount">The amount to multiply by.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Multiply(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	);

	/// <summary>
	/// Multiplies a value from <paramref name="buffer"/> against a corresponding
	/// value from <paramref name="amounts"/>.
	/// </summary>
	/// <param name="buffer">The source of values to be multiplied.</param>
	/// <param name="amounts">The source of the amount to multiply by.</param>
	void Multiply(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	);

	/// <summary>
	/// Multiplies a value from <paramref name="input"/> against a corresponding
	/// value from <paramref name="amounts"/> and stores the result in
	/// <paramref name="output"/>.
	/// </summary>
	/// <param name="input">The source of values to be multiplied.</param>
	/// <param name="amounts">The source of the amount to multiply by.</param>
	/// <param name="output">The buffer to receive the new values.</param>
	void Multiply(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	);

	void HorizontalBlur(
		IBuffer<float> input,
		int amount,
		IBuffer<float> output
	);

	void VerticalBlur(
		IBuffer<float> input,
		int amount,
		IBuffer<float> output
	);

	void Invert(
		IBuffer<float> input,
		IBuffer<float> output
	);

	void Quantize(
		IBuffer<float> input,
		IReadOnlyList<float> ranges,
		IBuffer<float> output
	);
}
