namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public sealed class BufferBoundsUnitTests {

	[TestCase( true )]
	[TestCase( false )]
	public void Indexer_ColumnOutsideLogicalWidth_Throws( bool flat ) {
		IBuffer<int> buffer = flat ? new FlatArrayBuffer<int>( 3, 2, 0 ) : new RaggedArrayBuffer<int>( 3, 2, 0 );
		buffer[ 0, 1 ] = 7;

		Assert.Throws<ArgumentOutOfRangeException>( () => _ = buffer[ 3, 0 ] );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer[ -1, 1 ] = 1 );
		Assert.That( buffer[ 0, 1 ], Is.EqualTo( 7 ) );
	}

	[TestCase( true )]
	[TestCase( false )]
	public void Indexer_RowOutsideLogicalHeight_Throws( bool flat ) {
		IBuffer<int> buffer = flat ? new FlatArrayBuffer<int>( 3, 2, 0 ) : new RaggedArrayBuffer<int>( 3, 2, 0 );

		Assert.Throws<ArgumentOutOfRangeException>( () => _ = buffer[ 0, 2 ] );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer[ 0, -1 ] = 1 );
		Assert.Throws<ArgumentOutOfRangeException>( () => buffer.GetRowSpan( -1 ) );
	}

	[TestCase( true )]
	[TestCase( false )]
	public void Constructor_ZeroDimension_Throws( bool flat ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, 0, 2 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, 2, 0 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Create( flat, -1, 2 ) );
	}

	[Test]
	public void FlatConstructor_ProductExceedsArrayLimit_Throws() {
		Assert.Throws<ArgumentOutOfRangeException>( () => new FlatArrayBuffer<byte>( int.MaxValue, 2, 0 ) );
	}

	private static IBuffer<int> Create( bool flat, int columns, int rows ) {
		return flat ? new FlatArrayBuffer<int>( columns, rows, 0 ) : new RaggedArrayBuffer<int>( columns, rows, 0 );
	}
}
