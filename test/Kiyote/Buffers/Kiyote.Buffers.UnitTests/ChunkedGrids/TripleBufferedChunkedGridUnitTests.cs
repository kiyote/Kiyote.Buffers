namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class TripleBufferedChunkedGridUnitTests {

	[Test]
	public void Swap_PublishesWrittenBuffer() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		grid.TrySet( 0, 0, GridBuffer.Current, 5f );

		grid.Swap();

		using TripleBufferedLease<float> lease = grid.Acquire();
		lease.TryGet( 0, 0, out float value );
		grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate );
		using( Assert.EnterMultipleScope() ) {
			Assert.That( value, Is.EqualTo( 5f ) );
			Assert.That( alternate, Is.EqualTo( 5f ) );
		}
	}

	[Test]
	public void Swap_WhileLeased_NeverWritesLeasedBuffer() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		grid.TrySet( 0, 0, GridBuffer.Current, 1f );
		grid.Swap();

		using TripleBufferedLease<float> lease = grid.Acquire();
		for( int step = 2; step < 20; step++ ) {
			grid.TrySet( 0, 0, GridBuffer.Current, step );
			grid.Swap();
			lease.TryGet( 0, 0, out float held );
			Assert.That( held, Is.EqualTo( 1f ), $"step {step}" );
		}
	}

	[Test]
	public void Acquire_AfterRelease_ReturnsLatest() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		grid.TrySet( 0, 0, GridBuffer.Current, 1f );
		grid.Swap();
		using( grid.Acquire() ) {
			grid.TrySet( 0, 0, GridBuffer.Current, 2f );
			grid.Swap();
			grid.TrySet( 0, 0, GridBuffer.Current, 3f );
			grid.Swap();
		}

		using TripleBufferedLease<float> lease = grid.Acquire();
		lease.TryGet( 0, 0, out float value );
		Assert.That( value, Is.EqualTo( 3f ) );
	}

	[Test]
	public void Acquire_NothingNew_ReturnsSameBuffer() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		grid.TrySet( 0, 0, GridBuffer.Current, 4f );
		grid.Swap();
		using( grid.Acquire() ) {
		}

		using TripleBufferedLease<float> lease = grid.Acquire();
		lease.TryGet( 0, 0, out float value );
		Assert.That( value, Is.EqualTo( 4f ) );
	}

	[Test]
	public void Concurrent_ReaderAlwaysSeesConsistentBuffer() {
		const int Cells = 256;
		TripleBufferedChunkedGrid<int> grid = new();
		ChunkedGrid<int> source = new();
		for( int c = 0; c < Cells; c++ ) {
			source[ c, 0 ] = 0;
		}
		grid.Attach( source, 0, 0 );

		using CancellationTokenSource stop = new();
		int torn = 0;
		int reads = 0;
		Thread reader = new( () => {
			int last = 0;
			while( !stop.IsCancellationRequested ) {
				using TripleBufferedLease<int> lease = grid.Acquire();
				lease.TryGet( 0, 0, out int first );
				for( int c = 1; c < Cells; c++ ) {
					lease.TryGet( c, 0, out int v );
					if( v != first ) {
						torn++;
					}
				}
				if( first < last ) {
					torn++;
				}
				last = first;
				reads++;
			}
		} );
		reader.Start();

		for( int step = 1; step <= 20000; step++ ) {
			for( int c = 0; c < Cells; c++ ) {
				grid.TrySet( c, 0, GridBuffer.Current, step );
			}
			grid.Swap();
		}
		stop.Cancel();
		reader.Join();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( torn, Is.Zero );
			Assert.That( reads, Is.GreaterThan( 0 ) );
		}
	}

	private static TripleBufferedChunkedGrid<float> CreateGrid() {
		TripleBufferedChunkedGrid<float> grid = new();
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 0f;
		grid.Attach( source, 0, 0 );
		return grid;
	}
}
