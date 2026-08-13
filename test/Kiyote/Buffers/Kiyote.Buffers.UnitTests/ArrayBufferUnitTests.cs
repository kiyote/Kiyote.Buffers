namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public class ArrayBufferUnitTests {

	[Test]
	public void GetRowSpan_ValidRow_LengthMatchesColumns() {
		ArrayBuffer<int> buffer = new( 5, 3, 0 );

		Span<int> span = buffer.GetRowSpan( 1 );

		Assert.That( span.Length, Is.EqualTo( buffer.Columns ) );
	}

	[Test]
	public void GetRowSpan_ValuesWritten_BufferUpdated() {
		ArrayBuffer<int> buffer = new( 5, 3, 0 );

		Span<int> span = buffer.GetRowSpan( 1 );
		span.Fill( 9 );

		Assert.That( buffer[ 0, 1 ], Is.EqualTo( 9 ) );
		Assert.That( buffer[ 4, 1 ], Is.EqualTo( 9 ) );
	}

	[Test]
	public void GetRowSpan_ValuesWritten_OtherRowsUnchanged() {
		ArrayBuffer<int> buffer = new( 5, 3, 0 );

		buffer.GetRowSpan( 1 ).Fill( 9 );

		Assert.That( buffer[ 0, 0 ], Is.Zero );
		Assert.That( buffer[ 0, 2 ], Is.Zero );
	}

	[Test]
	public void GetRowSpan_ExistingValues_ValuesVisible() {
		ArrayBuffer<int> buffer = new( 5, 3, 0 );
		buffer[ 2, 1 ] = 4;

		Span<int> span = buffer.GetRowSpan( 1 );

		Assert.That( span[ 2 ], Is.EqualTo( 4 ) );
	}

	[Test]
	public void GetRowSpan_RowOutOfRange_ThrowsException() {
		ArrayBuffer<int> buffer = new( 5, 3, 0 );

		Assert.Throws<IndexOutOfRangeException>( () => buffer.GetRowSpan( 3 ) );
	}
}
