namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

/// <summary>
/// Verifies the run-based unaligned Attach/Detach/HasOverlap paths against a
/// cell-by-cell reference across chunk sizes, offsets and sparse sources.
/// </summary>
[TestFixture]
public class ChunkedGridUnalignedUnitTests {

	private static readonly int[] Offsets = [ -129, -65, -63, -1, 1, 3, 63, 65, 98, 100, 127, 130 ];

	[TestCase( 8, 64 )]
	[TestCase( 64, 64 )]
	[TestCase( 128, 64 )]
	[TestCase( 64, 128 )]
	[TestCase( 256, 8 )]
	public void Attach_Unaligned_MatchesReference( int sourceChunkSize, int targetChunkSize ) {
		ChunkedGrid<float> source = CreateSparseSource( sourceChunkSize );
		foreach( int dc in Offsets ) {
			foreach( int dr in Offsets ) {
				ChunkedGrid<float> target = CreateBackground( targetChunkSize );
				Dictionary<(int, int), float> expected = Snapshot( target );
				foreach( ((int c, int r), float v) in Snapshot( source ) ) {
					expected[ (c + dc, r + dr) ] = v;
				}

				target.Attach( source, dc, dr );

				AssertMatches( target, expected, dc, dr );
			}
		}
	}

	[TestCase( 64, 64 )]
	[TestCase( 128, 8 )]
	public void AttachExtractor_Unaligned_MatchesReference( int sourceChunkSize, int targetChunkSize ) {
		ChunkedGrid<float> source = CreateSparseSource( sourceChunkSize );
		ChunkedGrid<float> target = CreateBackground( targetChunkSize );
		Dictionary<(int, int), float> expected = Snapshot( target );
		foreach( ((int c, int r), float v) in Snapshot( source ) ) {
			if( ( (int)v & 1 ) == 0 ) {
				expected[ (c + 37, r - 5) ] = v * 2f;
			}
		}

		target.Attach<float>( source, 37, -5, v => ( ( (int)v & 1 ) == 0, v * 2f ) );

		AssertMatches( target, expected, 37, -5 );
	}

	[TestCase( 64, 64 )]
	[TestCase( 8, 128 )]
	[TestCase( 128, 8 )]
	public void Detach_Unaligned_MatchesReference( int sourceChunkSize, int targetChunkSize ) {
		ChunkedGrid<float> source = CreateSparseSource( sourceChunkSize );
		foreach( int dc in Offsets ) {
			foreach( int dr in Offsets ) {
				ChunkedGrid<float> target = CreateBackground( targetChunkSize );
				Dictionary<(int, int), float> expected = Snapshot( target );
				foreach( (int c, int r) in Snapshot( source ).Keys ) {
					_ = expected.Remove( (c + dc, r + dr) );
				}

				target.Detach( source, dc, dr );

				AssertMatches( target, expected, dc, dr );
			}
		}
	}

	[TestCase( 64, 64 )]
	[TestCase( 128, 8 )]
	public void DetachExtractor_Unaligned_MatchesReference( int sourceChunkSize, int targetChunkSize ) {
		ChunkedGrid<float> source = CreateSparseSource( sourceChunkSize );
		ChunkedGrid<float> target = CreateBackground( targetChunkSize );
		Dictionary<(int, int), float> expected = Snapshot( target );
		foreach( ((int c, int r), float v) in Snapshot( source ) ) {
			if( ( (int)v & 1 ) == 0 ) {
				_ = expected.Remove( (c + 70, r + 3) );
			}
		}

		target.Detach<float>( source, 70, 3, v => ( ( (int)v & 1 ) == 0, 0f ) );

		AssertMatches( target, expected, 70, 3 );
	}

	[TestCase( 64, 64 )]
	[TestCase( 8, 128 )]
	public void HasOverlap_Unaligned_MatchesReference( int sourceChunkSize, int targetChunkSize ) {
		ChunkedGrid<float> source = CreateSparseSource( sourceChunkSize );
		ChunkedGrid<float> target = new( targetChunkSize );
		target[ 400, 401 ] = 1f;
		HashSet<(int, int)> cells = [ .. Snapshot( source ).Keys ];
		foreach( int dc in Offsets ) {
			foreach( int dr in Offsets ) {
				bool expected = cells.Contains( (400 - dc, 401 - dr) );
				Assert.That( target.HasOverlap( source, dc, dr ), Is.EqualTo( expected ), $"offset ({dc}, {dr})" );
			}
		}
	}

	private static ChunkedGrid<float> CreateSparseSource( int chunkSize ) {
		ChunkedGrid<float> grid = new( chunkSize );
		for( int r = -20; r < 280; r++ ) {
			for( int c = -30; c < 290; c++ ) {
				int hash = ( c * 7 ) + ( r * 13 );
				bool fullRow = r % 5 == 0 && c >= 0 && c < 200;
				if( fullRow || ( hash & 3 ) == 0 ) {
					grid[ c, r ] = ( c * 1000 ) + r;
				}
			}
		}
		return grid;
	}

	private static ChunkedGrid<float> CreateBackground( int chunkSize ) {
		ChunkedGrid<float> grid = new( chunkSize );
		for( int r = -150; r < 450; r += 3 ) {
			for( int c = -150; c < 450; c += 2 ) {
				grid[ c, r ] = -1f;
			}
		}
		return grid;
	}

	private static Dictionary<(int, int), float> Snapshot( ChunkedGrid<float> grid ) {
		Dictionary<(int, int), float> cells = [];
		foreach( GridChunk<float> chunk in grid.EnumerateChunks() ) {
			for( int r = 0; r < chunk.Size; r++ ) {
				for( int c = 0; c < chunk.Size; c++ ) {
					if( chunk.IsPopulated( c, r ) ) {
						cells[ (chunk.Column + c, chunk.Row + r) ] = chunk.Values[ ( r * chunk.Size ) + c ];
					}
				}
			}
		}
		return cells;
	}

	private static void AssertMatches(
		ChunkedGrid<float> target,
		Dictionary<(int, int), float> expected,
		int dc,
		int dr
	) {
		Dictionary<(int, int), float> actual = Snapshot( target );
		Assert.That( actual, Has.Count.EqualTo( expected.Count ), $"offset ({dc}, {dr}) cell count" );
		foreach( KeyValuePair<(int, int), float> cell in expected ) {
			if( !actual.TryGetValue( cell.Key, out float value ) || value != cell.Value ) {
				Assert.Fail( $"offset ({dc}, {dr}) cell {cell.Key}: expected {cell.Value}, found {( actual.ContainsKey( cell.Key ) ? value : "unpopulated" )}" );
			}
		}
	}
}
