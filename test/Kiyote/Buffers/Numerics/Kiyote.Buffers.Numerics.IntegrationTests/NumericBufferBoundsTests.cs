namespace Kiyote.Buffers.Numerics.IntegrationTests;

[TestFixture]
public sealed class NumericBufferBoundsTests {

	[TestCase( true )]
	[TestCase( false )]
	public void Indexer_PaddedOrOutsideColumn_Throws( bool flat ) {
		INumericBuffer<int> buffer = Create( flat, 3, 2 );
		buffer[ 0, 1 ] = 7;

		Assert.Throws<ArgumentOutOfRangeException>( () => _ = buffer[ 3, 0 ] );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer[ -1, 1 ] = 1 );
		Assert.That( buffer[ 0, 1 ], Is.EqualTo( 7 ) );
	}

	[TestCase( true )]
	[TestCase( false )]
	public void Indexer_RowOutsideHeight_Throws( bool flat ) {
		INumericBuffer<int> buffer = Create( flat, 3, 2 );

		Assert.Throws<ArgumentOutOfRangeException>( () => _ = buffer[ 0, 2 ] );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer[ 0, -1 ] = 1 );
	}

	[TestCase( true )]
	[TestCase( false )]
	public void Constructor_InvalidDimensions_Throws( bool flat ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, 0, 2 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, 2, 0 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, -1, 2 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, int.MaxValue, 2 ) );
	}

	private static INumericBuffer<int> Create( bool flat, int columns, int rows ) {
		return flat ? new FlatNumericBuffer<int>( columns, rows, 0 ) : new RaggedNumericBuffer<int>( columns, rows, 0 );
	}
}
