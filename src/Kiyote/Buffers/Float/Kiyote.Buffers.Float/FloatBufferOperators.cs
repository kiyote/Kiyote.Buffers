namespace Kiyote.Buffers.Float;

internal sealed class FloatBufferOperators : IFloatBufferOperators {

	private readonly IBufferOperator _bufferOperator;

	public FloatBufferOperators(
		IBufferOperator bufferOperator
	) {
		_bufferOperator = bufferOperator;
	}

	void IFloatBufferOperators.Add(
		IBuffer<float> buffer,
		float value
	) {
		( this as IFloatBufferOperators ).Add( buffer, value, buffer );
	}

	void IFloatBufferOperators.Add(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			( float value ) => {
				return value + amount;
			},
			output
		);
	}

	void IFloatBufferOperators.Add(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	) {
		( this as IFloatBufferOperators ).Add( buffer, amounts, buffer );
	}

	void IFloatBufferOperators.Add(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			amounts,
			( float value, float amount ) => {
				return value + amount;
			},
			output
		);
	}

	void IFloatBufferOperators.HorizontalBlur(
		IBuffer<float> input,
		int amount,
		IBuffer<float> output
	) {
		if( amount == 0 ) {
			throw new InvalidOperationException( "Why blur with zero effect?" );
		}

		for( int r = 0; r < input.Size.Height; r++ ) {
			float sum = input[0, r] * amount;
			for( int c = 0; c < input.Size.Width; c++ ) {
				int x = Math.Max( 0, c - amount );
				sum += input[c, r];
				sum -= input[x, r];
				output[c, r] = sum / amount;
			}
		}
	}

	void IFloatBufferOperators.VerticalBlur(
		IBuffer<float> input,
		int amount,
		IBuffer<float> output
	) {
		if( amount == 0 ) {
			throw new InvalidOperationException( "Why blur with zero effect?" );
		}

		for( int c = 0; c < input.Size.Height; c++ ) {
			float sum = input[c, 0] * amount;
			for( int r = 0; r < input.Size.Width; r++ ) {
				int y = Math.Max( 0, r - amount );
				sum += input[c, r];
				sum -= input[c, y];
				output[c, r] = sum / amount;
			}
		}
	}

	void IFloatBufferOperators.Multiply(
		IBuffer<float> buffer,
		float amount
	) {
		( this as IFloatBufferOperators ).Multiply( buffer, amount, buffer );
	}

	void IFloatBufferOperators.Multiply(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			( float value ) => {
				return value * amount;
			},
			output
		);
	}

	void IFloatBufferOperators.Multiply(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	) {
		( this as IFloatBufferOperators ).Multiply( buffer, amounts, buffer );
	}

	void IFloatBufferOperators.Multiply(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			amounts,
			( float value, float amount ) => {
				return value * amount;
			},
			output
		);
	}

	void IFloatBufferOperators.Normalize(
		IBuffer<float> buffer,
		float minimum,
		float maximum
	) {
		( this as IFloatBufferOperators ).Normalize( buffer, minimum, maximum, buffer );
	}

	void IFloatBufferOperators.Normalize(
		IBuffer<float> input,
		float minimum,
		float maximum,
		IBuffer<float> output
	) {
		float desiredRange = maximum - minimum;
		float minValue = float.MaxValue;
		float maxValue = float.MinValue;
		int rows = input.Size.Height;
		int columns = input.Size.Width;
		for( int r = 0; r < rows; r++ ) {
			for( int c = 0; c < columns; c++ ) {
				float value = input[c, r];
				if( value < minValue ) {
					minValue = value;
				}
				if( value > maxValue ) {
					maxValue = value;
				}
			}
		}

		float actualRange = Math.Abs( maxValue - minValue );
		float scale = 1.0f / actualRange;
		_bufferOperator.Perform(
			input,
			( float value ) => {
				float result = value - minValue;
				result *= scale; // Value will now be between 0..1

				return result * desiredRange;
			},
			output
		);

	}

	void IFloatBufferOperators.Subtract(
		IBuffer<float> buffer,
		float amount
	) {
		( this as IFloatBufferOperators ).Subtract( buffer, amount, buffer );
	}

	void IFloatBufferOperators.Subtract(
		IBuffer<float> input,
		float amount,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			( float value ) => {
				return value - amount;
			},
			output
		);
	}

	void IFloatBufferOperators.Subtract(
		IBuffer<float> buffer,
		IBuffer<float> amounts
	) {
		( this as IFloatBufferOperators ).Subtract( buffer, amounts, buffer );
	}

	void IFloatBufferOperators.Subtract(
		IBuffer<float> input,
		IBuffer<float> amounts,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			amounts,
			( float value, float amount ) => {
				return value - amount;
			},
			output
		);
	}

	void IFloatBufferOperators.Invert(
		IBuffer<float> input,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			( float value ) => {
				return 1.0f - value;
			},
			output
		);
	}

	void IFloatBufferOperators.Quantize(
		IBuffer<float> input,
		IReadOnlyList<float> ranges,
		IBuffer<float> output
	) {
		_bufferOperator.Perform(
			input,
			( float value ) => {
				int index = 0;
				while( value > ranges[index] ) {
					index++;
				}
				return index;
			},
			output
		);
	}
}
