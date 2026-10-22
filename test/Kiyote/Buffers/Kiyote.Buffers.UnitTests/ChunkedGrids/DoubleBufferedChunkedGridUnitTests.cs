using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Buffers.UnitTests.ChunkedGrids;

[TestFixture]
public sealed class DoubleBufferedChunkedGridUnitTests {

	[Test]
	public void Attach_WritesCurrentOnly_SharesPresence() {
		ChunkedGrid<float> source = new();
		source[ 3, 4 ] = 7f;
		DoubleBufferedChunkedGrid<float> grid = new();

		grid.Attach( source, 0, 0 );

		Assert.Multiple( () => {
			Assert.That( grid.TryGet( 3, 4, GridBuffer.Current, out float current ), Is.True );
			Assert.That( current, Is.EqualTo( 7f ) );
			Assert.That( grid.TryGet( 3, 4, GridBuffer.Alternate, out float alternate ), Is.True );
			Assert.That( alternate, Is.Zero );
		} );
	}

	[Test]
	public void TryGet_InvalidBuffer_Throws() {
		DoubleBufferedChunkedGrid<float> grid = new();
		Assert.Throws<ArgumentOutOfRangeException>( () => grid.TryGet( 0, 0, (GridBuffer)2, out _ ) );
	}

	[Test]
	public void TrySet_InvalidBuffer_ThrowsWithoutWriting() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		Assert.Throws<ArgumentOutOfRangeException>( () => grid.TrySet( 0, 0, (GridBuffer)(-1), 9f ) );
		Assert.That( grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate ), Is.True );
		Assert.That( alternate, Is.Zero );
	}

	[Test]
	public void Swap_ExchangesBuffers() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		grid.Swap();

		grid.TryGet( 0, 0, GridBuffer.Current, out float current );
		grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate );
		Assert.Multiple( () => {
			Assert.That( current, Is.Zero );
			Assert.That( alternate, Is.EqualTo( 1f ) );
		} );
	}

	[Test]
	public void TrySet_UnpopulatedCell_DoesNotChangeTopology() {
		DoubleBufferedChunkedGrid<float> grid = new();

		Assert.Multiple( () => {
			Assert.That( grid.TrySet( 0, 0, GridBuffer.Current, 1f ), Is.False );
			Assert.That( grid.ChunkCount, Is.Zero );
		} );
	}

	[Test]
	public void TrySet_PopulatedCell_WritesSelectedBuffer() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		Assert.That( grid.TrySet( 0, 0, GridBuffer.Alternate, 5f ), Is.True );
		grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate );
		grid.TryGet( 0, 0, GridBuffer.Current, out float current );
		Assert.Multiple( () => {
			Assert.That( alternate, Is.EqualTo( 5f ) );
			Assert.That( current, Is.EqualTo( 1f ) );
		} );
	}

	[Test]
	public void CopyCurrentToAlternate_CopiesValues() {
		ChunkedGrid<float> source = new();
		source[ 2, 2 ] = 3f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		grid.CopyCurrentToAlternate();

		grid.TryGet( 2, 2, GridBuffer.Alternate, out float alternate );
		Assert.That( alternate, Is.EqualTo( 3f ) );
	}

	[Test]
	public void CopyAlternateToCurrent_CopiesValues() {
		ChunkedGrid<float> source = new();
		source[ 2, 2 ] = 3f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		grid.CopyAlternateToCurrent();

		grid.TryGet( 2, 2, GridBuffer.Current, out float current );
		Assert.That( current, Is.Zero );
	}

	[Test]
	public void Detach_UnpopulatesBothBuffers() {
		ChunkedGrid<float> source = new();
		source[ 1, 1 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		grid.Detach( source, 0, 0 );

		Assert.Multiple( () => {
			Assert.That( grid.TryGet( 1, 1, GridBuffer.Current, out _ ), Is.False );
			Assert.That( grid.TryGet( 1, 1, GridBuffer.Alternate, out _ ), Is.False );
		} );
	}

	[Test]
	public void TryGetPopulatedBounds_AfterAttach_ExactCellBounds() {
		ChunkedGrid<float> source = new();
		source[ 1, 2 ] = 1f;
		source[ 5, 9 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();

		grid.Attach( source, 10, -20 );
		bool found = grid.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( found, Is.True );
			Assert.That( (column, row, width, height), Is.EqualTo( (11, -18, 5, 8) ) );
		}
	}

	[Test]
	public void EnumerateChunks_TopologyChanged_Throws() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		source[ 64, 0 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		Assert.That( () => {
			foreach( BufferedChunk<float> chunk in grid.EnumerateChunks() ) {
				grid.Attach( source, 128, 0 );
			}
		}, Throws.InvalidOperationException );
	}

	[Test]
	public void EnumerateChunks_ValueWrites_DoNotThrow() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );

		foreach( BufferedChunk<float> chunk in grid.EnumerateChunks() ) {
			chunk.Alternate.Values[ 0 ] = chunk.Current.Values[ 0 ] * 2f;
		}

		grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate );
		Assert.That( alternate, Is.EqualTo( 2f ) );
	}

	[Test]
	public void EnumerateNeighbourhoods_ExposesBothBuffers() {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		source[ 64, 0 ] = 2f;
		DoubleBufferedChunkedGrid<float> grid = new();
		grid.Attach( source, 0, 0 );
		grid.CopyCurrentToAlternate();
		grid.Swap();

		Assert.That( grid.TryGetNeighbourhood( 0, 0, out BufferedNeighbourhood<float> n ), Is.True );
		bool hasAlternate = n.Alternate.TryGetValue( 64, 0, out float east );
		bool hasCurrent = n.Current.TryGetValue( 64, 0, out float currentEast );
		Assert.Multiple( () => {
			Assert.That( hasAlternate, Is.True );
			Assert.That( east, Is.EqualTo( 2f ) );
			Assert.That( hasCurrent, Is.True );
			Assert.That( currentEast, Is.EqualTo( 2f ) );
		} );
	}
}
