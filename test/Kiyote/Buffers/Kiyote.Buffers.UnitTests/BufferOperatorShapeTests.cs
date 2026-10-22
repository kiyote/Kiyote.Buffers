namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public sealed class BufferOperatorShapeTests {

	[TestCase( 1, 2 )]
	[TestCase( 3, 2 )]
	[TestCase( 2, 3 )]
	public void Perform_OutputShapeDiffers_ThrowsBeforeWriting( int columns, int rows ) {
		IBufferOperator op = new BufferOperator();
		IBuffer<int> source = new FlatArrayBuffer<int>( 2, 2, 2 );
		RaggedArrayBuffer<int> output = new( columns, rows, 91 );

		Assert.Throws<InvalidOperationException>( () => op.Perform( source, value => value + 1, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform( source, source, ( left, right ) => left + right, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform( source, ( c, r, buffer, value ) => value, output ) );
		Assert.Throws<InvalidOperationException>( () => op.Perform<int, int>( source, ( c, r, value ) => value, output ) );
		Assert.That( output[ 0, 0 ], Is.EqualTo( 91 ) );
	}

	[Test]
	public void Perform_NullDelegate_ThrowsBeforeWriting() {
		IBufferOperator op = new BufferOperator();
		IBuffer<int> source = new FlatArrayBuffer<int>( 2, 2, 2 );
		FlatArrayBuffer<int> output = new( 2, 2, 91 );

		Assert.Throws<ArgumentNullException>( () => op.Perform( source, (Func<int, int>)null!, output ) );
		Assert.That( output[ 0, 0 ], Is.EqualTo( 91 ) );
	}
}
