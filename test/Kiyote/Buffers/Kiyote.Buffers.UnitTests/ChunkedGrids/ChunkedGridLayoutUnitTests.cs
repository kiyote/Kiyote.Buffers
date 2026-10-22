namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class ChunkedGridLayoutUnitTests {

	private static readonly int[] ExpectedEvenIndices = [ 0, 2, 4 ];

	[TestCase( 0 )]
	[TestCase( -8 )]
	[TestCase( 48 )]
	[TestCase( ChunkedGridLayout.MaximumChunkSize * 2 )]
	public void Ctor_InvalidChunkSize_Throws( int chunkSize ) {
		Assert.That( () => _ = new ChunkedGridLayout( chunkSize ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
	}

	[TestCase( 1 )]
	[TestCase( 8 )]
	[TestCase( ChunkedGridLayout.MaximumChunkSize )]
	public void Ctor_ValidChunkSize_Succeeds( int chunkSize ) {
		ChunkedGridLayout layout = new( chunkSize );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.ChunkSize, Is.EqualTo( chunkSize ) );
			Assert.That( layout.ChunkMask, Is.EqualTo( chunkSize - 1 ) );
			Assert.That( 1 << layout.ChunkShift, Is.EqualTo( chunkSize ) );
			Assert.That( layout.IsEmpty, Is.True );
		}
	}

	[TestCase( 8, 1 )]
	[TestCase( 64, 1 )]
	[TestCase( 128, 2 )]
	public void Ctor_PresenceWords_Correct( int chunkSize, int wordsPerRow ) {
		ChunkedGridLayout layout = new( chunkSize );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.PresenceWordsPerRow, Is.EqualTo( wordsPerRow ) );
			Assert.That( layout.PresenceWordsPerChunk, Is.EqualTo( wordsPerRow * chunkSize ) );
		}
	}

	[Test]
	public void GetOrCreateChunk_UnrepresentableCoordinate_DoesNotAllocate() {
		ChunkedGridLayout layout = new( 64 );

		Assert.Throws<ArgumentOutOfRangeException>( () => layout.GetOrCreateChunk( int.MaxValue, 0 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => layout.GetOrCreateChunk( 0, int.MinValue ) );
		Assert.That( layout.ChunkCount, Is.Zero );
	}

	[Test]
	public void Bounds_DistantChunks_ThrowInsteadOfWrapping() {
		ChunkedGridLayout layout = new( 64 );
		_ = layout.SetPopulated( int.MinValue, int.MinValue );
		_ = layout.SetPopulated( int.MaxValue, int.MaxValue );

		Assert.Throws<OverflowException>( () => _ = layout.Width );
		Assert.Throws<OverflowException>( () => _ = layout.Height );
		Assert.Throws<OverflowException>( () => layout.TryGetPopulatedBounds( out _, out _, out _, out _ ) );
		Assert.That( layout.Column, Is.EqualTo( int.MinValue ) );
		Assert.That( layout.Row, Is.EqualTo( int.MinValue ) );
	}

	[Test]
	public void TryGetPopulatedBounds_SingleCellAtMaximumCoordinate_ReturnsOneByOne() {
		ChunkedGridLayout layout = new( 64 );
		_ = layout.SetPopulated( int.MaxValue, int.MaxValue );

		bool found = layout.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		Assert.That( found, Is.True );
		Assert.That( (column, row, width, height), Is.EqualTo( (int.MaxValue, int.MaxValue, 1, 1) ) );
	}

	[Test]
	public void PopulatedRuns_BeyondMaximumColumn_ThrowBeforeMutation() {
		ChunkedGridLayout layout = new( 64 );

		Assert.Throws<ArgumentOutOfRangeException>( () => layout.SetPopulatedRun( int.MaxValue - 1, 0, 3 ) );
		Assert.That( layout.ChunkCount, Is.Zero );
		_ = layout.SetPopulated( int.MaxValue, 0 );
		Assert.Throws<ArgumentOutOfRangeException>( () => layout.ClearPopulatedRun( int.MaxValue, 0, 2 ) );
		Assert.That( layout.IsPopulated( int.MaxValue, 0 ), Is.True );
		Assert.That( layout.SetPopulatedRun( int.MaxValue, 0, 1 ), Is.Zero );
	}

	[Test]
	public void GetNeighbours_ExtremeChunkCoordinates_DoNotWrap() {
		ChunkedGridLayout layout = new( 1 );
		int left = layout.GetOrCreateChunk( int.MinValue, 0 );
		int right = layout.GetOrCreateChunk( int.MaxValue, 0 );

		Assert.That( layout.GetNeighbours( left )[ 3 ], Is.EqualTo( -1 ) );
		Assert.That( layout.GetNeighbours( right )[ 5 ], Is.EqualTo( -1 ) );
	}

	[Test]
	public void GetOrCreateChunk_NewChunks_DenseIndicesInAllocationOrder() {
		ChunkedGridLayout layout = new( 8 );

		int a = layout.GetOrCreateChunk( 5, 5 );
		int b = layout.GetOrCreateChunk( -3, 7 );
		int c = layout.GetOrCreateChunk( 0, -9 );
		int again = layout.GetOrCreateChunk( -3, 7 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( (a, b, c), Is.EqualTo( (0, 1, 2) ) );
			Assert.That( again, Is.EqualTo( b ) );
			Assert.That( layout.ChunkCount, Is.EqualTo( 3 ) );
			Assert.That( layout.ChunkCapacity, Is.EqualTo( 3 ) );
			Assert.That( layout.TryGetChunk( 0, -9, out int found ), Is.True );
			Assert.That( found, Is.EqualTo( c ) );
			Assert.That( layout.TryGetChunk( 1, 1, out int missing ), Is.False );
			Assert.That( missing, Is.EqualTo( -1 ) );
			Assert.That( layout.GetChunkColumn( b ), Is.EqualTo( -3 ) );
			Assert.That( layout.GetChunkRow( b ), Is.EqualTo( 7 ) );
			Assert.That( layout.GetChunkColumn( c ), Is.Zero );
			Assert.That( layout.GetChunkRow( c ), Is.EqualTo( -9 ) );
		}
	}

	[Test]
	public void RemoveChunk_ThenAllocateElsewhere_ReusesIndexWithEmptyPresence() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.SetPopulated( 9, 1 );
		int removedIndex = layout.GetOrCreateChunk( 1, 0 );
		int versionBeforeRemove = layout.Version;

		bool removed = layout.RemoveChunk( 1, 0 );
		int versionAfterRemove = layout.Version;
		int reused = layout.GetOrCreateChunk( 4, 4 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( removed, Is.True );
			Assert.That( reused, Is.EqualTo( removedIndex ) );
			Assert.That( layout.ChunkCapacity, Is.EqualTo( 2 ) );
			Assert.That( layout.GetPresence( reused ).ContainsAnyExcept( 0UL ), Is.False );
			Assert.That( layout.GetChunkColumn( reused ), Is.EqualTo( 4 ) );
			Assert.That( versionAfterRemove, Is.GreaterThan( versionBeforeRemove ) );
			Assert.That( layout.Version, Is.GreaterThan( versionAfterRemove ) );
			Assert.That( layout.RemoveChunk( 1, 0 ), Is.False );
		}
	}

	[Test]
	public void IsAllocated_InvalidIndices_ReturnsFalse() {
		ChunkedGridLayout layout = new( 8 );
		int index = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.GetOrCreateChunk( 1, 0 );
		_ = layout.RemoveChunk( 0, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.IsAllocated( -1 ), Is.False );
			Assert.That( layout.IsAllocated( 2 ), Is.False );
			Assert.That( layout.IsAllocated( index ), Is.False );
			Assert.That( layout.IsAllocated( 1 ), Is.True );
			Assert.That( () => layout.GetPresence( index ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layout.GetChunkColumn( 5 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layout.GetNeighbours( -1 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[Test]
	public void Compact_RemovesOnlyEmptyChunks_AndIndicesReused() {
		ChunkedGridLayout layout = new( 8 );
		int empty = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.SetPopulated( 8, 0 );
		int alsoEmpty = layout.GetOrCreateChunk( 2, 0 );

		int removed = layout.Compact();
		int first = layout.GetOrCreateChunk( 5, 5 );
		int second = layout.GetOrCreateChunk( 6, 5 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( removed, Is.EqualTo( 2 ) );
			Assert.That( layout.TryGetChunk( 1, 0, out _ ), Is.True );
			Assert.That( new[] { first, second }, Is.EquivalentTo( new[] { empty, alsoEmpty } ) );
			Assert.That( layout.ChunkCapacity, Is.EqualTo( 3 ) );
		}
	}

	[Test]
	public void GetNeighbours_AllocatedInAnyOrder_LinkedBothWays() {
		ChunkedGridLayout layout = new( 8 );
		int[] order = [ 7, 0, 4, 2, 8, 1, 6, 3, 5 ];
		int[] indices = new int[ 9 ];
		foreach( int k in order ) {
			indices[ k ] = layout.GetOrCreateChunk( ( k % 3 ) - 1, ( k / 3 ) - 1 );
		}

		ReadOnlySpan<int> centre = layout.GetNeighbours( indices[ 4 ] );
		using( Assert.EnterMultipleScope() ) {
			for( int k = 0; k < 9; k++ ) {
				Assert.That( centre[ k ], Is.EqualTo( indices[ k ] ) );
				if( k != 4 ) {
					Assert.That( layout.GetNeighbours( indices[ k ] )[ 8 - k ], Is.EqualTo( indices[ 4 ] ) );
				}
			}
		}
	}

	[Test]
	public void GetNeighbours_AfterRemoval_LinksCleared() {
		ChunkedGridLayout layout = new( 8 );
		int centre = layout.GetOrCreateChunk( 0, 0 );
		int east = layout.GetOrCreateChunk( 1, 0 );
		int south = layout.GetOrCreateChunk( 0, 1 );

		_ = layout.RemoveChunk( 1, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.GetNeighbours( centre )[ 5 ], Is.EqualTo( -1 ) );
			Assert.That( layout.GetNeighbours( centre )[ 7 ], Is.EqualTo( south ) );
			Assert.That( layout.GetNeighbours( centre )[ 4 ], Is.EqualTo( centre ) );
			Assert.That( layout.GetNeighbours( south )[ 2 ], Is.EqualTo( -1 ) );
		}

		int reused = layout.GetOrCreateChunk( 5, 5 );
		Assert.That( reused, Is.EqualTo( east ) );
		ReadOnlySpan<int> isolated = layout.GetNeighbours( reused );
		using( Assert.EnterMultipleScope() ) {
			for( int k = 0; k < 9; k++ ) {
				Assert.That( isolated[ k ], Is.EqualTo( k == 4 ? reused : -1 ) );
			}
		}
	}

	[Test]
	public void SetPopulated_ClearPopulated_ReportChanges() {
		ChunkedGridLayout layout = new( 8 );

		bool first = layout.SetPopulated( -3, 2 );
		bool second = layout.SetPopulated( -3, 2 );
		bool cleared = layout.ClearPopulated( -3, 2 );
		bool clearedAgain = layout.ClearPopulated( -3, 2 );
		bool clearedUnallocated = layout.ClearPopulated( 100, 100 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( first, Is.True );
			Assert.That( second, Is.False );
			Assert.That( cleared, Is.True );
			Assert.That( clearedAgain, Is.False );
			Assert.That( clearedUnallocated, Is.False );
			Assert.That( layout.ChunkCount, Is.EqualTo( 1 ) );
			Assert.That( layout.TryGetChunk( -1, 0, out _ ), Is.True );
		}
	}

	[TestCase( 8, -13, 30 )]
	[TestCase( 8, 0, 30 )]
	[TestCase( 8, 7, 2 )]
	[TestCase( 8, 1, 17 )]
	[TestCase( 128, 60, 10 )]
	[TestCase( 128, -70, 200 )]
	[TestCase( 32, 3, 64 )]
	public void SetPopulatedRun_MatchesPerCell( int chunkSize, int column, int length ) {
		ChunkedGridLayout layout = new( chunkSize );
		_ = layout.SetPopulated( column + 1, 3 );

		int changed = layout.SetPopulatedRun( column, 3, length );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( changed, Is.EqualTo( length - 1 ) );
			for( int c = column - 2; c < column + length + 2; c++ ) {
				bool expected = c >= column && c < column + length;
				Assert.That( layout.IsPopulated( c, 3 ), Is.EqualTo( expected ), $"column {c}" );
				Assert.That( layout.IsPopulated( c, 2 ), Is.False );
			}
		}
	}

	[TestCase( 8, -13, 30 )]
	[TestCase( 128, 60, 10 )]
	[TestCase( 32, 3, 64 )]
	public void ClearPopulatedRun_ClearsOnlyRun_CountsChanges( int chunkSize, int column, int length ) {
		ChunkedGridLayout layout = new( chunkSize );
		_ = layout.SetPopulatedRun( column - 5, 3, length + 10 );
		_ = layout.ClearPopulated( column + 1, 3 );
		int chunks = layout.ChunkCount;

		int changed = layout.ClearPopulatedRun( column, 3, length );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( changed, Is.EqualTo( length - 1 ) );
			Assert.That( layout.ChunkCount, Is.EqualTo( chunks ) );
			for( int c = column - 5; c < column + length + 5; c++ ) {
				bool expected = c < column || c >= column + length;
				Assert.That( layout.IsPopulated( c, 3 ), Is.EqualTo( expected ), $"column {c}" );
			}
		}
	}

	[Test]
	public void ClearPopulatedRun_UnallocatedChunks_NeverAllocates() {
		ChunkedGridLayout layout = new( 8 );

		int changed = layout.ClearPopulatedRun( 0, 0, 100 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( changed, Is.Zero );
			Assert.That( layout.IsEmpty, Is.True );
		}
	}

	[Test]
	public void PopulatedRun_ZeroLength_NoOp_NegativeThrows() {
		ChunkedGridLayout layout = new( 8 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.SetPopulatedRun( 0, 0, 0 ), Is.Zero );
			Assert.That( layout.IsEmpty, Is.True );
			Assert.That( () => layout.SetPopulatedRun( 0, 0, -1 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layout.ClearPopulatedRun( 0, 0, -1 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[Test]
	public void TryLocate_Cases() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.SetPopulated( -6, 13 );

		bool unallocated = layout.TryLocate( 100, 100, out int unallocatedChunk, out _ );
		bool unpopulated = layout.TryLocate( -5, 13, out _, out _ );
		bool populated = layout.TryLocate( -6, 13, out int chunkIndex, out int localIndex );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( unallocated, Is.False );
			Assert.That( unallocatedChunk, Is.EqualTo( -1 ) );
			Assert.That( unpopulated, Is.False );
			Assert.That( populated, Is.True );
			Assert.That( layout.GetChunkColumn( chunkIndex ), Is.EqualTo( -1 ) );
			Assert.That( layout.GetChunkRow( chunkIndex ), Is.EqualTo( 1 ) );
			Assert.That( localIndex, Is.EqualTo( ( 5 * 8 ) + 2 ) );
		}
	}

	[TestCase( 8 )]
	[TestCase( 64 )]
	[TestCase( 128 )]
	public void GetFill_GetPopulatedCount_EmptyPartialFull( int chunkSize ) {
		ChunkedGridLayout layout = new( chunkSize );
		int index = layout.GetOrCreateChunk( 0, 0 );

		ChunkFill empty = layout.GetFill( index );
		int emptyCount = layout.GetPopulatedCount( index );
		_ = layout.SetPopulated( 1, 1 );
		ChunkFill partial = layout.GetFill( index );
		for( int r = 0; r < chunkSize; r++ ) {
			_ = layout.SetPopulatedRun( 0, r, chunkSize );
		}
		ChunkFill full = layout.GetFill( index );
		int fullCount = layout.GetPopulatedCount( index );
		layout.GetPresence( index )[ 0 ] &= ~1UL;
		ChunkFill afterSpanWrite = layout.GetFill( index );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( empty, Is.EqualTo( ChunkFill.Empty ) );
			Assert.That( emptyCount, Is.Zero );
			Assert.That( partial, Is.EqualTo( ChunkFill.Partial ) );
			Assert.That( full, Is.EqualTo( ChunkFill.Full ) );
			Assert.That( fullCount, Is.EqualTo( chunkSize * chunkSize ) );
			Assert.That( afterSpanWrite, Is.EqualTo( ChunkFill.Partial ) );
			Assert.That( layout.GetPopulatedCount( index ), Is.EqualTo( ( chunkSize * chunkSize ) - 1 ) );
		}
	}

	[Test]
	public void GetPresenceRow_SlicesRow() {
		ChunkedGridLayout layout = new( 128 );
		_ = layout.SetPopulated( 70, 3 );
		_ = layout.TryGetChunk( 0, 0, out int index );

		Span<ulong> row = layout.GetPresenceRow( index, 3 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( row.Length, Is.EqualTo( 2 ) );
			Assert.That( row[ 1 ], Is.EqualTo( 1UL << 6 ) );
			Assert.That( () => layout.GetPresenceRow( index, 128 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[TestCase( 8 )]
	[TestCase( 32 )]
	[TestCase( 64 )]
	[TestCase( 128 )]
	public void ReadPresenceBits_MatchesIsPopulated( int chunkSize ) {
		ChunkedGridLayout layout = new( chunkSize );
		// Centre chunk (0, 0) plus every neighbour except the north-east one.
		for( int dy = -1; dy <= 1; dy++ ) {
			for( int dx = -1; dx <= 1; dx++ ) {
				if( dx == 1 && dy == -1 ) {
					continue;
				}
				_ = layout.GetOrCreateChunk( dx, dy );
			}
		}
		for( int r = -chunkSize; r < 2 * chunkSize; r++ ) {
			for( int c = -chunkSize; c < 2 * chunkSize; c++ ) {
				if( ( ( ( c * 7 ) + ( r * 13 ) ) % 3 ) != 0 && layout.TryGetChunk( c >> layout.ChunkShift, r >> layout.ChunkShift, out _ ) ) {
					_ = layout.SetPopulated( c, r );
				}
			}
		}
		_ = layout.TryGetChunk( 0, 0, out int centre );

		int[] rows = [ -chunkSize, -1, 0, chunkSize - 1, chunkSize, ( 2 * chunkSize ) - 1 ];
		int[] columns = [ -chunkSize, -1, 0, chunkSize - 3, chunkSize ];
		int[] counts = [ 1, 5, Math.Min( 64, chunkSize ), 64 ];
		using( Assert.EnterMultipleScope() ) {
			foreach( int r in rows ) {
				foreach( int c in columns ) {
					foreach( int count in counts ) {
						if( c + count > 2 * chunkSize ) {
							continue;
						}
						ulong expected = 0;
						for( int i = 0; i < count; i++ ) {
							if( layout.IsPopulated( c + i, r ) ) {
								expected |= 1UL << i;
							}
						}
						Assert.That( layout.ReadPresenceBits( centre, c, r, count ), Is.EqualTo( expected ), $"c={c} r={r} n={count}" );
					}
				}
			}
		}
	}

	[Test]
	public void ReadPresenceBits_SpanningWestCentreEast_At32() {
		ChunkedGridLayout layout = new( 32 );
		_ = layout.SetPopulatedRun( -32, 0, 96 );
		_ = layout.ClearPopulated( -10, 0 );
		_ = layout.TryGetChunk( 0, 0, out int centre );

		ulong bits = layout.ReadPresenceBits( centre, -16, 0, 64 );

		Assert.That( bits, Is.EqualTo( ~( 1UL << 6 ) ) );
	}

	[Test]
	public void ReadPresenceBits_AbsentNeighbours_ReadZero() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.SetPopulatedRun( 0, 0, 8 );
		_ = layout.TryGetChunk( 0, 0, out int centre );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layout.ReadPresenceBits( centre, -4, 0, 16 ), Is.EqualTo( 0xFFUL << 4 ) );
			Assert.That( layout.ReadPresenceBits( centre, 0, -1, 8 ), Is.Zero );
		}
	}

	[TestCase( 0, 0, 0 )]
	[TestCase( 0, 0, 65 )]
	[TestCase( -9, 0, 1 )]
	[TestCase( 10, 0, 7 )]
	[TestCase( 0, -9, 1 )]
	[TestCase( 0, 16, 1 )]
	public void ReadPresenceBits_OutOfRange_Throws( int column, int row, int count ) {
		ChunkedGridLayout layout = new( 8 );
		int centre = layout.GetOrCreateChunk( 0, 0 );

		Assert.That( () => layout.ReadPresenceBits( centre, column, row, count ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void TryGetPopulatedBounds_Empty_ReturnsFalse() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.GetOrCreateChunk( 3, 3 );

		bool found = layout.TryGetPopulatedBounds( out _, out _, out int width, out int height );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( found, Is.False );
			Assert.That( width, Is.Zero );
			Assert.That( height, Is.Zero );
		}
	}

	[Test]
	public void TryGetPopulatedBounds_SingleCell() {
		ChunkedGridLayout layout = new( 64 );
		_ = layout.SetPopulated( 10, 20 );

		_ = layout.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		Assert.That( (column, row, width, height), Is.EqualTo( (10, 20, 1, 1) ) );
	}

	[TestCase( 8 )]
	[TestCase( 128 )]
	public void TryGetPopulatedBounds_SeveralChunks_IgnoresEmptyChunks( int chunkSize ) {
		ChunkedGridLayout layout = new( chunkSize );
		_ = layout.SetPopulated( -13, 5 );
		_ = layout.SetPopulated( 100, -40 );
		_ = layout.SetPopulated( 70, 90 );
		_ = layout.GetOrCreateChunk( -100, -100 );
		_ = layout.GetOrCreateChunk( 100, 100 );

		bool found = layout.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( found, Is.True );
			Assert.That( (column, row, width, height), Is.EqualTo( (-13, -40, 114, 131) ) );
		}
	}

	[Test]
	public void EnumerateChunks_AscendingAndSkipsFreed() {
		ChunkedGridLayout layout = new( 8 );
		for( int i = 0; i < 5; i++ ) {
			_ = layout.GetOrCreateChunk( i, 0 );
		}
		_ = layout.RemoveChunk( 1, 0 );
		_ = layout.RemoveChunk( 3, 0 );

		List<int> visited = [];
		foreach( int index in layout.EnumerateChunks() ) {
			visited.Add( index );
		}

		Assert.That( visited, Is.EqualTo( ExpectedEvenIndices ) );
	}

	[Test]
	public void EnumerateChunks_AllocationDuringEnumeration_Throws() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.GetOrCreateChunk( 1, 0 );

		Assert.That( () => {
			foreach( int index in layout.EnumerateChunks() ) {
				_ = layout.GetOrCreateChunk( 10 + index, 0 );
			}
		}, Throws.InvalidOperationException );
	}

	[Test]
	public void EnumerateChunks_RemovalDuringEnumeration_Throws() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.GetOrCreateChunk( 1, 0 );

		Assert.That( () => {
			foreach( int index in layout.EnumerateChunks() ) {
				_ = layout.RemoveChunk( 1, 0 );
			}
		}, Throws.InvalidOperationException );
	}

	[Test]
	public void EnumerateChunks_CellChanges_DoNotThrow() {
		ChunkedGridLayout layout = new( 8 );
		_ = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.GetOrCreateChunk( 1, 0 );
		int count = 0;

		foreach( int index in layout.EnumerateChunks() ) {
			_ = layout.SetPopulated( layout.GetChunkColumn( index ) * 8, 0 );
			count++;
		}

		Assert.That( count, Is.EqualTo( 2 ) );
	}

	[Test]
	public void Hull_AllocateRemoveCompact_MatchesChunkAlignedBounds() {
		ChunkedGridLayout layout = new( 8 );

		_ = layout.SetPopulated( -1, -1 );
		_ = layout.GetOrCreateChunk( 2, 1 );
		int column = layout.Column;
		int row = layout.Row;
		int width = layout.Width;
		int height = layout.Height;
		_ = layout.RemoveChunk( -1, -1 );
		int columnAfterRemove = layout.Column;
		_ = layout.Compact();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( (column, row, width, height), Is.EqualTo( (-8, -8, 32, 24) ) );
			Assert.That( columnAfterRemove, Is.EqualTo( 16 ) );
			Assert.That( layout.IsEmpty, Is.True );
			Assert.That( (layout.Column, layout.Row, layout.Width, layout.Height), Is.EqualTo( (0, 0, 0, 0) ) );
		}
	}
}
