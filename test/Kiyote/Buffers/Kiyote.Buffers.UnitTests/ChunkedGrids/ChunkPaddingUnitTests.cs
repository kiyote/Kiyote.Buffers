namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class ChunkPaddingUnitTests {

	private const int Size = 4;

	private static IEnumerable<TestCaseData> MissingCases() {
		int[] halos = [ 1, 2, Size ];
		foreach( int halo in halos ) {
			yield return new TestCaseData( halo, -1, false ).SetName( $"Gather_Halo{halo}_AllPresent_MatchesReference" );
			yield return new TestCaseData( halo, -1, true ).SetName( $"Gather_Halo{halo}_AllMissing_MatchesReference" );
			for( int missing = 0; missing < 9; missing++ ) {
				if( missing != 4 ) {
					yield return new TestCaseData( halo, missing, false ).SetName( $"Gather_Halo{halo}_Missing{missing}_MatchesReference" );
				}
			}
		}
	}

	[TestCaseSource( nameof( MissingCases ) )]
	public void Gather_MatchesReference( int halo, int missing, bool allMissing ) {
		int[]?[] chunks = CreateChunks( missing, allMissing );
		int[] actual = new int[ ChunkPadding.GetPaddedLength( Size, halo ) ];

		ChunkPadding.Gather( Size, halo, Spans( chunks ), actual );

		Assert.That( actual, Is.EqualTo( Reference( chunks, halo ) ) );
	}

	[Test]
	public void Gather_HaloZero_EqualsCentre() {
		int[]?[] chunks = CreateChunks( -1, false );
		int[] actual = new int[ Size * Size ];

		ChunkPadding.Gather( Size, 0, Spans( chunks ), actual );

		Assert.That( actual, Is.EqualTo( chunks[ 4 ] ) );
	}

	[TestCase( 1, 2 )]
	[TestCase( 2, -1 )]
	[TestCase( Size, 5 )]
	public void GatherRow_MatchesGatherRows( int halo, int missing ) {
		int[]?[] chunks = CreateChunks( missing, false );
		int stride = Size + ( 2 * halo );
		int[] full = new int[ stride * stride ];
		ChunkPadding.Gather( Size, halo, Spans( chunks ), full );
		int[] row = new int[ stride ];

		using( Assert.EnterMultipleScope() ) {
			for( int r = -halo; r < Size + halo; r++ ) {
				ChunkPadding.GatherRow( Size, halo, r, Spans( chunks ), row );
				Assert.That( row, Is.EqualTo( full.AsSpan( ( r + halo ) * stride, stride ).ToArray() ), $"row {r}" );
			}
		}
	}

	[Test]
	public void Gather_InvalidArguments_Throw() {
		int[]?[] chunks = CreateChunks( -1, false );
		int[] destination = new int[ 1024 ];
		int[]?[] noCentre = CreateChunks( -1, false );
		noCentre[ 4 ] = null;
		int[]?[] wrongLength = CreateChunks( -1, false );
		wrongLength[ 2 ] = new int[ 3 ];

		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => ChunkPadding.Gather( Size, -1, Spans( chunks ), destination ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.Gather( Size, Size + 1, Spans( chunks ), destination ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.Gather( 0, 0, Spans( chunks ), destination ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.Gather( Size, 1, Spans( chunks ), new int[ 35 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.Gather( Size, 1, Spans( noCentre ), destination ), Throws.ArgumentException );
			Assert.That( () => ChunkPadding.Gather( Size, 1, Spans( wrongLength ), destination ), Throws.ArgumentException );
			Assert.That( () => ChunkPadding.GatherRow( Size, 1, -2, Spans( chunks ), destination ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.GatherRow( Size, 1, Size + 1, Spans( chunks ), destination ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => ChunkPadding.GatherRow( Size, 1, 0, Spans( chunks ), new int[ 5 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[Test]
	public void Gather_ReferenceType_Works() {
		string[]?[] chunks = new string[]?[ 9 ];
		for( int k = 0; k < 9; k++ ) {
			if( k == 3 ) {
				continue;
			}
			chunks[ k ] = new string[ Size * Size ];
			for( int i = 0; i < Size * Size; i++ ) {
				chunks[ k ]![ i ] = $"{k}:{i}";
			}
		}
		string[] actual = new string[ ChunkPadding.GetPaddedLength( Size, 1 ) ];

		ChunkPadding.Gather( Size, 1, Spans( chunks ), actual );

		int stride = Size + 2;
		using( Assert.EnterMultipleScope() ) {
			Assert.That( actual[ 0 ], Is.EqualTo( $"0:{( Size * Size ) - 1}" ) );
			Assert.That( actual[ stride + 1 ], Is.EqualTo( "4:0" ) );
			Assert.That( actual[ stride ], Is.Null );
			Assert.That( actual[ ( 2 * stride ) - 1 ], Is.EqualTo( "5:0" ) );
		}
	}

	[TestCase( 8, 1 )]
	[TestCase( 8, 8 )]
	[TestCase( 32, 2 )]
	public void Neighbourhood_CopyPadded_MatchesGather( int chunkSize, int halo ) {
		ChunkedGridLayout layout = new( chunkSize );
		ChunkedGridLayer<float> layer = new( layout );
		int[] indices = new int[ 9 ];
		for( int k = 0; k < 9; k++ ) {
			// Leave the south-east chunk unallocated.
			indices[ k ] = k == 8 ? -1 : layout.GetOrCreateChunk( ( k % 3 ) - 1, ( k / 3 ) - 1 );
		}
		foreach( GridChunk<float> chunk in layer.EnumerateChunks() ) {
			for( int i = 0; i < chunk.Values.Length; i++ ) {
				chunk.Values[ i ] = ( chunk.ChunkIndex * 10000 ) + i;
			}
		}
		float[]?[] arrays = new float[]?[ 9 ];
		for( int k = 0; k < 9; k++ ) {
			arrays[ k ] = indices[ k ] < 0 ? null : layer.GetValues( indices[ k ] ).ToArray();
		}
		int length = ChunkPadding.GetPaddedLength( chunkSize, halo );
		float[] expected = new float[ length ];
		ChunkPadding.Gather( chunkSize, halo, Spans( arrays ), expected );
		float[] actual = new float[ length ];
		int stride = chunkSize + ( 2 * halo );
		float[] row = new float[ stride ];

		GridChunkNeighbourhood<float> neighbourhood = layer.GetNeighbourhood( indices[ 4 ] );
		neighbourhood.CopyPadded( halo, actual );
		bool rowsMatch = true;
		for( int r = -halo; r < chunkSize + halo; r++ ) {
			neighbourhood.CopyPaddedRow( r, halo, row );
			rowsMatch &= row.AsSpan().SequenceEqual( expected.AsSpan( ( r + halo ) * stride, stride ) );
		}

		using( Assert.EnterMultipleScope() ) {
			Assert.That( actual, Is.EqualTo( expected ) );
			Assert.That( rowsMatch, Is.True );
		}
	}

	[Test]
	public void Neighbourhood_CopyPadded_InvalidArguments_Throw() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		int index = layout.GetOrCreateChunk( 0, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => layer.GetNeighbourhood( index ).CopyPadded( -1, new float[ 1024 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetNeighbourhood( index ).CopyPadded( 9, new float[ 1024 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetNeighbourhood( index ).CopyPadded( 1, new float[ 99 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetNeighbourhood( index ).CopyPaddedRow( 9, 1, new float[ 10 ] ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[TestCase( 8 )]
	[TestCase( 32 )]
	[TestCase( 128 )]
	public void Neighbourhood_ReadPresenceBits_MatchesLayout( int chunkSize ) {
		ChunkedGridLayout layout = new( chunkSize );
		ChunkedGridLayer<float> layer = new( layout );
		for( int r = -chunkSize; r < 2 * chunkSize; r++ ) {
			for( int c = -chunkSize; c < 2 * chunkSize; c++ ) {
				if( ( ( ( c * 5 ) + ( r * 3 ) ) % 4 ) == 0 ) {
					_ = layout.SetPopulated( c, r );
				}
			}
		}
		_ = layout.RemoveChunk( 1, 1 );
		_ = layout.TryGetChunk( 0, 0, out int centre );
		GridChunkNeighbourhood<float> neighbourhood = layer.GetNeighbourhood( centre );

		bool matches = true;
		for( int r = -chunkSize; r < 2 * chunkSize; r += 3 ) {
			for( int c = -chunkSize; c < 2 * chunkSize; c += 7 ) {
				int count = Math.Min( 64, ( 2 * chunkSize ) - c );
				matches &= neighbourhood.ReadPresenceBits( c, r, count ) == layout.ReadPresenceBits( centre, c, r, count );
			}
		}

		Assert.That( matches, Is.True );
	}

	private static int[]?[] CreateChunks(
		int missing,
		bool allMissing
	) {
		int[]?[] chunks = new int[]?[ 9 ];
		for( int k = 0; k < 9; k++ ) {
			if( k != 4 && ( allMissing || k == missing ) ) {
				continue;
			}
			chunks[ k ] = new int[ Size * Size ];
			for( int i = 0; i < Size * Size; i++ ) {
				chunks[ k ]![ i ] = ( ( k + 1 ) * 1000 ) + i;
			}
		}
		return chunks;
	}

	private static int[] Reference(
		int[]?[] chunks,
		int halo
	) {
		int stride = Size + ( 2 * halo );
		int[] result = new int[ stride * stride ];
		for( int pr = 0; pr < stride; pr++ ) {
			for( int pc = 0; pc < stride; pc++ ) {
				int column = pc - halo;
				int row = pr - halo;
				int dx = column < 0 ? -1 : column >= Size ? 1 : 0;
				int dy = row < 0 ? -1 : row >= Size ? 1 : 0;
				int[]? chunk = chunks[ ( ( dy + 1 ) * 3 ) + dx + 1 ];
				result[ ( pr * stride ) + pc ] = chunk is null
					? 0
					: chunk[ ( ( row - ( dy * Size ) ) * Size ) + column - ( dx * Size ) ];
			}
		}
		return result;
	}

	private static ChunkNeighbourSpans<T> Spans<T>(
		T[]?[] chunks
	) {
		return new ChunkNeighbourSpans<T>() {
			NorthWest = chunks[ 0 ],
			North = chunks[ 1 ],
			NorthEast = chunks[ 2 ],
			West = chunks[ 3 ],
			Centre = chunks[ 4 ],
			East = chunks[ 5 ],
			SouthWest = chunks[ 6 ],
			South = chunks[ 7 ],
			SouthEast = chunks[ 8 ]
		};
	}
}
