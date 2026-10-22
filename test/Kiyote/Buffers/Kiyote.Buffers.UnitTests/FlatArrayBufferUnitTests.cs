namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public class FlatArrayBufferUnitTests {

	[Test]
	public void Indexer_FirstAndLastCells_RoundTripWithoutAliasing() {
		FlatArrayBuffer<int> buffer = new( 3, 2, 7 );
		buffer[ 0, 0 ] = 1;
		buffer[ 2, 1 ] = 9;

		Assert.That( buffer[ 0, 0 ], Is.EqualTo( 1 ) );
		Assert.That( buffer[ 2, 1 ], Is.EqualTo( 9 ) );
		Assert.That( buffer[ 0, 1 ], Is.EqualTo( 7 ) );
	}

	[TestCase( -1, 0 )]
	[TestCase( 3, 0 )]
	[TestCase( 0, -1 )]
	[TestCase( 0, 2 )]
	public void Indexer_OutsideLogicalDimensions_ThrowsWithoutChangingOtherCells( int column, int row ) {
		FlatArrayBuffer<int> buffer = new( 3, 2, 5 );

		Assert.Throws<ArgumentOutOfRangeException>( () => _ = buffer[ column, row ] );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer[ column, row ] = 9 );
		Assert.That( buffer[ 0, 1 ], Is.EqualTo( 5 ) );
	}

	[TestCase( 0, 2 )]
	[TestCase( 2, 0 )]
	[TestCase( -1, 2 )]
	[TestCase( 2, -1 )]
	public void Constructor_NonPositiveDimension_Throws( int columns, int rows ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new FlatArrayBuffer<int>( columns, rows, 0 ) );
	}

	[Test]
	public void Constructor_UnrepresentableLength_Throws() {
		Assert.Throws<ArgumentOutOfRangeException>( () => new FlatArrayBuffer<int>( int.MaxValue, 2, 0 ) );
	}

	[Test]
	public void GetRowSpan_ValidAndInvalidRows_ReturnsLogicalRowOrThrows() {
		FlatArrayBuffer<int> buffer = new( 3, 2, 4 );
		buffer.GetRowSpan( 1 )[ 2 ] = 8;

		Assert.That( buffer[ 2, 1 ], Is.EqualTo( 8 ) );
		Assert.That( buffer.GetRowSpan( 1 ).Length, Is.EqualTo( 3 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer.GetRowSpan( -1 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer.GetRowSpan( 2 ) );
	}
}
