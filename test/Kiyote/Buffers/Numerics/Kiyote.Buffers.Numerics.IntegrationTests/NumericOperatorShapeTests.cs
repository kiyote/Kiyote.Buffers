namespace Kiyote.Buffers.Numerics.IntegrationTests;

[TestFixture]
public sealed class NumericOperatorShapeTests {

	[TestCase( true, 1, 2 )]
	[TestCase( false, 3, 2 )]
	[TestCase( true, 2, 3 )]
	public void NumericOperations_DestinationShapeDiffers_ThrowBeforeWriting( bool flat, int columns, int rows ) {
		INumericBufferOperator op = new NumericBufferOperator();
		INumericBuffer<int> source = Create( flat, 2, 2, 2 );
		INumericBuffer<int> destination = Create( flat, columns, rows, 91 );

		Assert.Throws<InvalidOperationException>( () => op.Add( source, destination, 1 ) );
		Assert.Throws<InvalidOperationException>( () => op.Subtract( source, destination, 1 ) );
		Assert.Throws<InvalidOperationException>( () => op.Multiply( source, destination, 2 ) );
		Assert.Throws<InvalidOperationException>( () => op.Divide( source, destination, 2 ) );
		Assert.Throws<InvalidOperationException>( () => op.Normalize( source, destination ) );
		Assert.Throws<InvalidOperationException>( () => op.ScaleToRange( source, destination ) );
		Assert.That( destination[ 0, 0 ], Is.EqualTo( 91 ) );
	}

	[Test]
	public void Perform_DestinationShapeDiffers_ThrowsBeforeWriting() {
		IBufferOperator op = new NumericBufferOperator();
		IBuffer<int> source = new RaggedNumericBuffer<int>( 2, 2, 1 );
		IBuffer<int> output = new FlatNumericBuffer<int>( 1, 4, 91 );

		Assert.Throws<InvalidOperationException>( () => op.Perform( source, value => value, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform( source, source, ( left, right ) => left + right, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform( source, ( c, r, buffer, value ) => value, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform<int, int>( source, ( c, r, value ) => value, output ) );
		Assert.That( output[ 0, 0 ], Is.EqualTo( 91 ) );
	}

	private static INumericBuffer<int> Create( bool flat, int columns, int rows, int initialValue ) {
		return flat ? new FlatNumericBuffer<int>( columns, rows, initialValue ) : new RaggedNumericBuffer<int>( columns, rows, initialValue );
	}
}
