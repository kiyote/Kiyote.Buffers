namespace Kiyote.Buffers.Tests;

[TestFixture]
public class BufferOperatorIntegrationTests {

	private IBufferFactory _bufferFactory;
	private IBufferOperator _bufferOperator;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		_bufferFactory = new ArrayBufferFactory();
	}

	[SetUp]
	public void Setup() {
		_bufferOperator = new BufferOperator();
	}

	[Test]
	public void Perform_SingleBuffer_ValuesIteratedCorrectly() {
		IBuffer<int> input = _bufferFactory.Create<int>( 2, 2, 0 );
		input[ 0, 0 ] = 1;
		input[ 1, 0 ] = 2;
		input[ 0, 1 ] = 3;
		input[ 1, 1 ] = 4;

		IBuffer<int> output = _bufferFactory.Create<int>( 2, 2, 0 );
		_bufferOperator.Perform(
			input,
			( int value ) => {
				return value;
			},
			output
		);

		for( int r = 0; r < input.Size.Height; r++ ) {
			for( int c = 0; c < input.Size.Width; c++ ) {
				Assert.That( input[ c, r ], Is.EqualTo( output[ c, r ] ) );
			}
		}
	}

	[Test]
	public void Perform_MultiBuffer_ValuesIteratedCorrectly() {
		IBuffer<int> input1 = _bufferFactory.Create<int>( 2, 2, 0 );
		input1[ 0, 0 ] = 1;
		input1[ 1, 0 ] = 2;
		input1[ 0, 1 ] = 3;
		input1[ 1, 1 ] = 4;

		IBuffer<int> input2 = _bufferFactory.Create<int>( 2, 2, 0 );
		input2[ 0, 0 ] = 1;
		input2[ 1, 0 ] = 2;
		input2[ 0, 1 ] = 3;
		input2[ 1, 1 ] = 4;

		IBuffer<int> output = _bufferFactory.Create<int>( 2, 2, 0 );
		_bufferOperator.Perform(
			input1,
			input2,
			( int left, int right ) => {
				return left + right;
			},
			output
		);

		for( int r = 0; r < input1.Size.Height; r++ ) {
			for( int c = 0; c < input1.Size.Width; c++ ) {
				Assert.That( input1[ c, r ] * 2, Is.EqualTo( output[ c, r ] ) );
			}
		}
	}

	[Test]
	public void Perform_BufferSizesDiffer_ThrowsException() {
		IBuffer<int> input1 = _bufferFactory.Create<int>( 2, 2, 0 );
		IBuffer<int> input2 = _bufferFactory.Create<int>( 3, 3, 0 );
		IBuffer<int> output = _bufferFactory.Create<int>( 4, 4, 0 );

		Assert.Throws<InvalidOperationException>( () => {
			_bufferOperator.Perform(
				input1,
				input2,
				( int left, int right ) => {
					return left + right;
				},
				output
			);
		} );
	}
}
