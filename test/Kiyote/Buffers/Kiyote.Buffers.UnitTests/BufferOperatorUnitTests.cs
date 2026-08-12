namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public class BufferOperatorUnitTests {

	private IBufferOperator _bufferOperator;

	[SetUp]
	public void Setup() {
		_bufferOperator = new BufferOperator();
	}

	[Test]
	public void Create_ValidDimensions_BufferCreated() {
		IBufferFactory bufferFactory = new ArrayBufferFactory();

		IBuffer<int> buffer = bufferFactory.Create<int>( 3, 2, 7 );

		Assert.That( buffer.Columns, Is.EqualTo( 3 ) );
		Assert.That( buffer.Rows, Is.EqualTo( 2 ) );
		Assert.That( buffer[ 2, 1 ], Is.EqualTo( 7 ) );
	}

	[Test]
	public void Perform_SingleBuffer_ValuesIteratedCorrectly() {
		ArrayBuffer<int> input = new( 2, 2, 0 );
		input[ 0, 0 ] = 1;
		input[ 1, 0 ] = 2;
		input[ 0, 1 ] = 3;
		input[ 1, 1 ] = 4;

		ArrayBuffer<int> output = new( 2, 2, 0 );
		_bufferOperator.Perform(
			input,
			value => {
				return value;
			},
			output
		);

		for( int r = 0; r < input.Rows; r++ ) {
			for( int c = 0; c < input.Columns; c++ ) {
				Assert.That( input[ c, r ], Is.EqualTo( output[ c, r ] ) );
			}
		}
	}

	[Test]
	public void Perform_MultiBuffer_ValuesIteratedCorrectly() {
		ArrayBuffer<int> input1 = new( 2, 2, 0 );
		input1[ 0, 0 ] = 1;
		input1[ 1, 0 ] = 2;
		input1[ 0, 1 ] = 3;
		input1[ 1, 1 ] = 4;

		ArrayBuffer<int> input2 = new( 2, 2, 0 );
		input2[ 0, 0 ] = 1;
		input2[ 1, 0 ] = 2;
		input2[ 0, 1 ] = 3;
		input2[ 1, 1 ] = 4;

		ArrayBuffer<int> output = new( 2, 2, 0 );
		_bufferOperator.Perform(
			input1,
			input2,
			( left, right ) => {
				return left + right;
			},
			output
		);

		for( int r = 0; r < input1.Rows; r++ ) {
			for( int c = 0; c < input1.Columns; c++ ) {
				Assert.That( input1[ c, r ] * 2, Is.EqualTo( output[ c, r ] ) );
			}
		}
	}

	[Test]
	public void Perform_SourceBufferAndLocation_ValuesIteratedCorrectly() {
		ArrayBuffer<int> input = new( 2, 2, 0 );
		input[ 0, 0 ] = 1;
		input[ 1, 0 ] = 2;
		input[ 0, 1 ] = 3;
		input[ 1, 1 ] = 4;

		ArrayBuffer<int> output = new( 2, 2, 0 );
		_bufferOperator.Perform(
			input,
			( column, row, source, value ) => {
				return source[ column, row ] + column + row;
			},
			output
		);

		for( int r = 0; r < input.Rows; r++ ) {
			for( int c = 0; c < input.Columns; c++ ) {
				Assert.That( output[ c, r ], Is.EqualTo( input[ c, r ] + c + r ) );
			}
		}
	}

	[Test]
	public void Perform_DifferingOutputType_ValuesIteratedCorrectly() {
		ArrayBuffer<int> input = new( 2, 2, 0 );
		input[ 0, 0 ] = 1;
		input[ 1, 0 ] = 2;
		input[ 0, 1 ] = 3;
		input[ 1, 1 ] = 4;

		ArrayBuffer<string> output = new( 2, 2, "" );
		_bufferOperator.Perform(
			input,
			( column, row, value ) => {
				return $"{column},{row}={value}";
			},
			output
		);

		for( int r = 0; r < input.Rows; r++ ) {
			for( int c = 0; c < input.Columns; c++ ) {
				Assert.That( output[ c, r ], Is.EqualTo( $"{c},{r}={input[ c, r ]}" ) );
			}
		}
	}

	[Test]
	public void Perform_BufferSizesDiffer_ThrowsException() {
		ArrayBuffer<int> input1 = new( 2, 2, 0 );
		ArrayBuffer<int> input2 = new( 3, 3, 0 );
		ArrayBuffer<int> output = new( 4, 4, 0 );

		Assert.That(
			() => {
				_bufferOperator.Perform(
					input1,
					input2,
					( left, right ) => {
						return left + right;
					},
					output
				);
			},
			Throws.TypeOf<InvalidOperationException>()
		);
	}
}
