namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class TripleBufferedChunkedGridUnitTests {

	[Test]
	public void TryGet_InvalidBuffer_Throws() {
		TripleBufferedChunkedGrid<float> grid = new();
		Assert.Throws<ArgumentOutOfRangeException>( () => grid.TryGet( 0, 0, (GridBuffer)2, out _ ) );
	}

	[Test]
	public void TrySet_InvalidBuffer_ThrowsWithoutWriting() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		Assert.Throws<ArgumentOutOfRangeException>( () => grid.TrySet( 0, 0, (GridBuffer)(-1), 9f ) );
		Assert.That( grid.TryGet( 0, 0, GridBuffer.Alternate, out float alternate ), Is.True );
		Assert.That( alternate, Is.Zero );
	}

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
	public void Acquire_WhileLeaseOutstanding_ThrowsAndPreservesPublishedBuffer() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		grid.TrySet( 0, 0, GridBuffer.Current, 1f );
		grid.Swap();
		using TripleBufferedLease<float> held = grid.Acquire();

		Assert.Throws<InvalidOperationException>( () => grid.Acquire() );
		for( int step = 2; step < 10; step++ ) {
			grid.TrySet( 0, 0, GridBuffer.Current, step );
			grid.Swap();
			Assert.That( held.TryGet( 0, 0, out float value ), Is.True );
			Assert.That( value, Is.EqualTo( 1f ) );
		}
	}

	[Test]
	public void Dispose_StaleCopiedLease_DoesNotReleaseNewLease() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		TripleBufferedLease<float> previous = grid.Acquire();
		TripleBufferedLease<float> staleCopy = previous;
		previous.Dispose();
		using TripleBufferedLease<float> current = grid.Acquire();

		Assert.That( current, Is.Not.EqualTo( previous ) );
		staleCopy.Dispose();
		Assert.Throws<InvalidOperationException>( () => grid.Acquire() );
		previous.Dispose();
		Assert.Throws<InvalidOperationException>( () => grid.Acquire() );
	}

	[Test]
	public void Dispose_DefaultLease_DoesNotAffectActiveLease() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		using TripleBufferedLease<float> current = grid.Acquire();
		default( TripleBufferedLease<float> ).Dispose();

		Assert.Throws<InvalidOperationException>( () => grid.Acquire() );
	}

	[Test]
	public void TopologyChanges_WhileLeased_ThrowBeforeMutation() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		ChunkedGrid<float> source = new();
		source[ 64, 0 ] = 1f;
		using TripleBufferedLease<float> lease = grid.Acquire();

		Assert.Throws<InvalidOperationException>( () => grid.Attach( source, 0, 0 ) );
		Assert.Throws<InvalidOperationException>( () => grid.Detach( source, 0, 0 ) );
		Assert.Throws<InvalidOperationException>( () => grid.RemoveChunk( 0, 0 ) );
		Assert.Throws<InvalidOperationException>( () => grid.Compact() );
		Assert.Throws<InvalidOperationException>( () => grid.CopyCurrentToAlternate() );
		Assert.Throws<InvalidOperationException>( () => grid.CopyAlternateToCurrent() );
		Assert.That( grid.ChunkCount, Is.EqualTo( 1 ) );
		Assert.That( lease.IsPopulated( 0, 0 ), Is.True );
	}

	[Test]
	public void Acquire_DuringAttach_ThrowsUntilMutationCompletes() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();
		ChunkedGrid<float> source = new();
		source[ 64, 0 ] = 1f;
		using ManualResetEventSlim entered = new();
		using ManualResetEventSlim release = new();
		Task owner = Task.Run( () => {
			grid.Attach<float>( source, 0, 0, value => {
				entered.Set();
				_ = release.Wait( TimeSpan.FromSeconds( 10 ) );
				return (true, value);
			} );
		} );
		try {
			Assert.That( entered.Wait( TimeSpan.FromSeconds( 10 ) ), Is.True );
			Assert.Throws<InvalidOperationException>( () => {
				using TripleBufferedLease<float> unexpected = grid.Acquire();
			} );
		} finally {
			release.Set();
			owner.GetAwaiter().GetResult();
		}
		using TripleBufferedLease<float> lease = grid.Acquire();
		Assert.That( lease.IsPopulated( 64, 0 ), Is.True );
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

	[Test]
	public void Acquire_AfterAttachAllocatingChunks_LeaseReadsValues() {
		TripleBufferedChunkedGrid<float> grid = new( 8 );
		ChunkedGrid<float> source = new( 8 );
		source[ 3, 3 ] = 1f;
		source[ 20, 3 ] = 2f;

		grid.Attach( source, 5, 0 );
		grid.Swap();
		using TripleBufferedLease<float> lease = grid.Acquire();
		int chunks = 0;
		foreach( GridChunk<float> chunk in lease.EnumerateChunks() ) {
			chunks++;
		}

		using( Assert.EnterMultipleScope() ) {
			Assert.That( lease.TryGet( 8, 3, out float first ), Is.True );
			Assert.That( first, Is.EqualTo( 1f ) );
			Assert.That( lease.TryGet( 25, 3, out float second ), Is.True );
			Assert.That( second, Is.EqualTo( 2f ) );
			Assert.That( chunks, Is.EqualTo( lease.ChunkCount ) );
			Assert.That( chunks, Is.EqualTo( 2 ) );
		}
	}

	[Test]
	public void TryGetPopulatedBounds_ExactCellBounds() {
		TripleBufferedChunkedGrid<float> grid = CreateGrid();

		bool found = grid.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( found, Is.True );
			Assert.That( (column, row, width, height), Is.EqualTo( (0, 0, 1, 1) ) );
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
