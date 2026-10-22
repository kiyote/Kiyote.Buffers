using System.Runtime.Intrinsics;

namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class ChunkedGridUnitTests {

	private enum Terrain : byte {
		None,
		Land,
		Water
	}

	[Test]
	public void Ctor_DefaultChunkSize_Is64() {
		ChunkedGrid<float> grid = new();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.ChunkSize, Is.EqualTo( 64 ) );
			Assert.That( grid.PresenceWordsPerRow, Is.EqualTo( 1 ) );
		}
	}

	[TestCase( 0 )]
	[TestCase( 48 )]
	[TestCase( 4 )]
	public void Ctor_InvalidFloatChunkSize_Throws( int chunkSize ) {
		Assert.That( () => _ = new ChunkedGrid<float>( chunkSize ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void Ctor_ByteChunkSmallerThanVector256_Throws() {
		Assert.That( () => _ = new ChunkedGrid<Terrain>( 16 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
	}

	[TestCase( 8 )]
	[TestCase( 128 )]
	public void Ctor_ValidFloatChunkSize_Succeeds( int chunkSize ) {
		ChunkedGrid<float> grid = new( chunkSize );

		Assert.That( grid.ChunkSize, Is.EqualTo( chunkSize ) );
	}

	[Test]
	public void Indexer_Set_CellPopulated() {
		ChunkedGrid<float> grid = new();

		grid[ 3, 5 ] = 1.5f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.IsPopulated( 3, 5 ), Is.True );
			Assert.That( grid[ 3, 5 ], Is.EqualTo( 1.5f ) );
			Assert.That( grid.IsPopulated( 4, 5 ), Is.False );
		}
	}

	[Test]
	public void Indexer_NegativeCoordinates_RoundTrip() {
		ChunkedGrid<Terrain> grid = new();

		grid[ -1, -65 ] = Terrain.Water;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid[ -1, -65 ], Is.EqualTo( Terrain.Water ) );
			Assert.That( grid.HasChunk( -1, -2 ), Is.True );
		}
	}

	[TestCase( true )]
	[TestCase( false )]
	public void Attach_TranslatedCellsOutsideRange_ThrowsBeforeWriting( bool aligned ) {
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		source[ 64, 0 ] = 2f;
		ChunkedGrid<float> target = new();
		target[ 0, 0 ] = 42f;
		int offset = aligned ? int.MaxValue - 63 : int.MaxValue - 62;

		Assert.Throws<ArgumentOutOfRangeException>( () => target.Attach( source, offset, 0 ) );
		Assert.That( target.ChunkCount, Is.EqualTo( 1 ) );
		Assert.That( target[ 0, 0 ], Is.EqualTo( 42f ) );
		Assert.That( target.IsPopulated( offset, 0 ), Is.False );
	}

	[TestCase( int.MinValue + 1, -1, int.MinValue )]
	[TestCase( int.MaxValue, 0, int.MaxValue )]
	public void Attach_TranslatedPopulatedCellAtCoordinateLimit_RoundTrips( int offset, int sourceColumn, int targetColumn ) {
		ChunkedGrid<float> source = new();
		source[ sourceColumn, 0 ] = 5f;
		ChunkedGrid<float> target = new();

		target.Attach( source, offset, 0 );

		Assert.That( target[ targetColumn, 0 ], Is.EqualTo( 5f ) );
		Assert.That( target.HasOverlap( source, offset, 0 ), Is.True );
		target.Detach( source, offset, 0 );
		Assert.That( target.IsPopulated( targetColumn, 0 ), Is.False );
	}

	[Test]
	public void TryGet_UnpopulatedCellInAllocatedChunk_ReturnsFalse() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;

		bool found = grid.TryGet( 1, 0, out float value );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( found, Is.False );
			Assert.That( value, Is.Zero );
		}
	}

	[Test]
	public void Clear_PopulatedCell_Unpopulated() {
		ChunkedGrid<float> grid = new();
		grid[ 10, 10 ] = 2f;

		grid.Clear( 10, 10 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.IsPopulated( 10, 10 ), Is.False );
			Assert.That( grid.HasChunk( 0, 0 ), Is.True );
		}
	}

	[Test]
	public void Hull_Empty_IsZero() {
		ChunkedGrid<float> grid = new();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.Width, Is.Zero );
			Assert.That( grid.Height, Is.Zero );
		}
	}

	[Test]
	public void Hull_GrowsChunkAligned() {
		ChunkedGrid<float> grid = new();

		grid[ 5, 5 ] = 1f;
		grid[ -3, 130 ] = 1f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.Column, Is.EqualTo( -64 ) );
			Assert.That( grid.Row, Is.Zero );
			Assert.That( grid.Width, Is.EqualTo( 128 ) );
			Assert.That( grid.Height, Is.EqualTo( 192 ) );
		}
	}

	[Test]
	public void Hull_AfterRemoveChunk_Shrinks() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ 200, 0 ] = 1f;

		bool removed = grid.RemoveChunk( 3, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( removed, Is.True );
			Assert.That( grid.Width, Is.EqualTo( 64 ) );
			Assert.That( grid.IsPopulated( 200, 0 ), Is.False );
		}
	}

	[Test]
	public void GetChunk_RowSpans_AreVectorMultiples() {
		ChunkedGrid<float> grid = new();
		grid[ 1, 2 ] = 7f;

		Assert.That( grid.TryGetChunk( 0, 0, out GridChunk<float> chunk ), Is.True );
		Span<float> rowValues = chunk.GetRowValues( 2 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( rowValues.Length % Vector256<float>.Count, Is.Zero );
			Assert.That( rowValues[ 1 ], Is.EqualTo( 7f ) );
			Assert.That( chunk.GetPresenceRow( 2 )[ 0 ], Is.EqualTo( 1UL << 1 ) );
			Assert.That( chunk.IsFull, Is.False );
			Assert.That( chunk.IsEmpty, Is.False );
		}
	}

	[Test]
	public void GetChunk_RowValuesWritten_GridUpdated() {
		ChunkedGrid<float> grid = new();
		GridChunk<float> chunk = grid.GetOrCreateChunk( 1, 0 );

		chunk.GetRowValues( 0 ).Fill( 3f );
		chunk.GetPresenceRow( 0 )[ 0 ] = ulong.MaxValue;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid[ 64, 0 ], Is.EqualTo( 3f ) );
			Assert.That( grid[ 127, 0 ], Is.EqualTo( 3f ) );
		}
	}

	[Test]
	public void EnumerateChunks_VisitsAllocatedChunks() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ 64, 0 ] = 1f;
		grid[ 65, 1 ] = 1f;

		int count = 0;
		foreach( GridChunk<float> chunk in grid.EnumerateChunks() ) {
			count++;
		}

		Assert.That( count, Is.EqualTo( 2 ) );
	}

	[Test]
	public void IsFull_AllCellsPopulated_SmallChunk_True() {
		ChunkedGrid<float> grid = new( 8 );
		for( int r = 0; r < 8; r++ ) {
			for( int c = 0; c < 8; c++ ) {
				grid[ c, r ] = 1f;
			}
		}

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.TryGetChunk( 0, 0, out GridChunk<float> chunk ), Is.True );
			Assert.That( chunk.IsFull, Is.True );
		}
	}

	[Test]
	public void PresenceMask_Expand_ProducesLaneMask() {
		ReadOnlySpan<ulong> presence = [ 0b1010UL ];
		Span<uint> mask = stackalloc uint[ 4 ];

		PresenceMask.Expand( presence, mask );

		Assert.That( mask.ToArray(), Is.EqualTo( new uint[] { 0, uint.MaxValue, 0, uint.MaxValue } ) );
	}

	[Test]
	public void PresenceMask_ExpandVector256_SelectsPopulated() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 5f;
		grid[ 2, 0 ] = 6f;
		grid.TryGetChunk( 0, 0, out GridChunk<float> chunk );

		Vector256<float> values = Vector256.Create<float>( chunk.GetRowValues( 0 ) );
		Vector256<float> mask = PresenceMask.ExpandVector256<uint>( chunk.GetPresenceRow( 0 ), 0 ).AsSingle();
		Vector256<float> selected = Vector256.ConditionalSelect( mask, values, Vector256.Create( -1f ) );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( selected.GetElement( 0 ), Is.EqualTo( 5f ) );
			Assert.That( selected.GetElement( 1 ), Is.EqualTo( -1f ) );
			Assert.That( selected.GetElement( 2 ), Is.EqualTo( 6f ) );
		}
	}

	[Test]
	public void Attach_SameType_Aligned_CopiesCells() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		target[ 0, 0 ] = 1f;
		source[ 0, 0 ] = 2f;
		source[ 3, 4 ] = 3f;

		target.Attach( source, 64, -64 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target[ 64, -64 ], Is.EqualTo( 2f ) );
			Assert.That( target[ 67, -60 ], Is.EqualTo( 3f ) );
			Assert.That( target.IsPopulated( 65, -64 ), Is.False );
			Assert.That( target[ 0, 0 ], Is.EqualTo( 1f ) );
		}
	}

	[Test]
	public void Attach_SameType_AlignedFullRow_CopiesRow() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		for( int c = 0; c < 64; c++ ) {
			source[ c, 0 ] = c;
		}

		target.Attach( source, 0, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target[ 63, 0 ], Is.EqualTo( 63f ) );
			Assert.That( target.IsPopulated( 0, 1 ), Is.False );
		}
	}

	[Test]
	public void Attach_Unaligned_TranslatesAcrossChunks() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		source[ 63, 0 ] = 4f;

		target.Attach( source, 1, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target[ 64, 0 ], Is.EqualTo( 4f ) );
			Assert.That( target.HasChunk( 1, 0 ), Is.True );
		}
	}

	[Test]
	public void Attach_WithExtractor_ConvertsAndFilters() {
		ChunkedGrid<Terrain> target = new();
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 0.2f;
		source[ 1, 0 ] = 0.8f;
		source[ 2, 0 ] = -1f;

		target.Attach(
			source,
			10,
			10,
			( float h ) => ( h >= 0f, h > 0.5f ? Terrain.Land : Terrain.Water )
		);

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target[ 10, 10 ], Is.EqualTo( Terrain.Water ) );
			Assert.That( target[ 11, 10 ], Is.EqualTo( Terrain.Land ) );
			Assert.That( target.IsPopulated( 12, 10 ), Is.False );
		}
	}

	[Test]
	public void Attach_StructExtractor_ReceivesSourceCoordinates() {
		ChunkedGrid<int> target = new();
		ChunkedGrid<float> source = new();
		source[ 5, 7 ] = 1f;

		target.Attach( source, 100, 100, default( ColumnExtractor ) );

		Assert.That( target[ 105, 107 ], Is.EqualTo( 5 ) );
	}

	[Test]
	public void HasOverlap_Overlapping_ReturnsTrue() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		target[ 5, 5 ] = 1f;
		source[ 5, 5 ] = 3f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target.HasOverlap( source, 0, 0 ), Is.True );
			Assert.That( target.HasOverlap( source, 64, 0 ), Is.False );
		}
	}

	[Test]
	public void HasOverlap_Unaligned_ReturnsTrue() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new( 8 );
		target[ 6, 6 ] = 1f;
		source[ 5, 5 ] = 3f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target.HasOverlap( source, 1, 1 ), Is.True );
			Assert.That( target.HasOverlap( source, 2, 1 ), Is.False );
		}
	}

	[Test]
	public void Attach_Overlap_OverwritesCells() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		target[ 5, 5 ] = 1f;
		target[ 6, 5 ] = 9f;
		source[ 0, 0 ] = 2f;
		source[ 5, 5 ] = 3f;

		target.Attach( source, 0, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target[ 0, 0 ], Is.EqualTo( 2f ) );
			Assert.That( target[ 5, 5 ], Is.EqualTo( 3f ) );
			Assert.That( target[ 6, 5 ], Is.EqualTo( 9f ) );
		}
	}

	[Test]
	public void Attach_UnalignedOverlap_OverwritesCells() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new( 8 );
		target[ 6, 6 ] = 1f;
		source[ 5, 5 ] = 3f;

		target.Attach( source, 1, 1 );

		Assert.That( target[ 6, 6 ], Is.EqualTo( 3f ) );
	}

	[Test]
	public void Attach_Self_Throws() {
		ChunkedGrid<float> grid = new();

		Assert.That( () => grid.Attach( grid, 64, 0 ), Throws.ArgumentException );
	}

	[Test]
	public void HasOverlap_Self_Throws() {
		ChunkedGrid<float> grid = new();

		Assert.That( () => grid.HasOverlap( grid, 64, 0 ), Throws.ArgumentException );
	}

	[Test]
	public void Detach_Aligned_UnpopulatesSourceCellsOnly() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		target[ 0, 0 ] = 1f;
		source[ 3, 4 ] = 3f;
		target.Attach( source, 64, 0 );

		target.Detach( source, 64, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target.IsPopulated( 67, 4 ), Is.False );
			Assert.That( target.IsPopulated( 0, 0 ), Is.True );
			Assert.That( target.HasChunk( 1, 0 ), Is.True );
		}
	}

	[Test]
	public void Detach_Unaligned_DifferentType_UnpopulatesCells() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<Terrain> source = new();
		source[ 63, 0 ] = Terrain.Land;
		target[ 64, 0 ] = 1f;
		target[ 65, 0 ] = 2f;

		target.Detach( source, 1, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target.IsPopulated( 64, 0 ), Is.False );
			Assert.That( target.IsPopulated( 65, 0 ), Is.True );
		}
	}

	[Test]
	public void Detach_WithExtractor_DeclinedCellsUntouched() {
		ChunkedGrid<Terrain> target = new();
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 0.2f;
		source[ 1, 0 ] = -1f;
		target[ 0, 0 ] = Terrain.Land;
		target[ 1, 0 ] = Terrain.Land;

		target.Detach( source, 0, 0, ( float h ) => ( h >= 0f, Terrain.Water ) );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( target.IsPopulated( 0, 0 ), Is.False );
			Assert.That( target.IsPopulated( 1, 0 ), Is.True );
		}
	}

	[Test]
	public void Detach_Self_Throws() {
		ChunkedGrid<float> grid = new();

		Assert.That( () => grid.Detach( grid, 0, 0 ), Throws.ArgumentException );
	}

	[Test]
	public void Compact_AfterDetach_ReleasesEmptyChunksAndShrinksHull() {
		ChunkedGrid<float> target = new();
		ChunkedGrid<float> source = new();
		target[ 0, 0 ] = 1f;
		source[ 0, 0 ] = 2f;
		target.Attach( source, 128, 0 );
		target.Detach( source, 128, 0 );

		int released = target.Compact();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( released, Is.EqualTo( 1 ) );
			Assert.That( target.HasChunk( 2, 0 ), Is.False );
			Assert.That( target.Width, Is.EqualTo( 64 ) );
			Assert.That( target[ 0, 0 ], Is.EqualTo( 1f ) );
		}
	}

	[Test]
	public void Compact_NoEmptyChunks_ReturnsZero() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( grid.Compact(), Is.Zero );
			Assert.That( grid.ChunkCount, Is.EqualTo( 1 ) );
		}
	}

	[Test]
	public void Neighbourhood_MissingCenter_ReturnsFalse() {
		ChunkedGrid<float> grid = new();

		Assert.That( grid.TryGetNeighbourhood( 0, 0, out _ ), Is.False );
	}

	[Test]
	public void Neighbourhood_ResolvesNeighbours() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ -1, 0 ] = 2f;
		grid[ 64, 64 ] = 3f;

		Assert.That( grid.TryGetNeighbourhood( 0, 0, out GridChunkNeighbourhood<float> n ), Is.True );

		Assert.That( n.Center.ChunkColumn, Is.Zero );
		Assert.That( n.HasNeighbour( -1, 0 ), Is.True );
		Assert.That( n.HasNeighbour( 1, 1 ), Is.True );
		Assert.That( n.HasNeighbour( 0, -1 ), Is.False );
		Assert.That( n.TryGetNeighbour( 1, 1, out GridChunk<float> se ), Is.True );
		Assert.That( se.Column, Is.EqualTo( 64 ) );
	}

	[Test]
	public void Neighbourhood_TryGetValue_ReadsAcrossBorders() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ -1, -1 ] = 2f;
		grid[ 64, 10 ] = 3f;

		grid.TryGetNeighbourhood( 0, 0, out GridChunkNeighbourhood<float> n );

		Assert.That( n.TryGetValue( -1, -1, out float nw ), Is.True );
		Assert.That( nw, Is.EqualTo( 2f ) );
		Assert.That( n.TryGetValue( 64, 10, out float e ), Is.True );
		Assert.That( e, Is.EqualTo( 3f ) );
		Assert.That( n.IsPopulated( 0, -1 ), Is.False );
		Assert.Throws<ArgumentOutOfRangeException>( () => {
			grid.TryGetNeighbourhood( 0, 0, out GridChunkNeighbourhood<float> inner );
			inner.IsPopulated( 128, 0 );
		} );
	}

	[Test]
	public void Neighbourhood_GetRowValues_ReturnsHaloRows() {
		ChunkedGrid<float> grid = new();
		grid[ 5, 0 ] = 1f;
		grid[ 5, -1 ] = 2f;

		grid.TryGetNeighbourhood( 0, 0, out GridChunkNeighbourhood<float> n );
		Span<float> north = n.GetRowValues( -1 );
		Span<ulong> northPresence = n.GetPresenceRow( -1 );
		Span<float> south = n.GetRowValues( 64 );

		Assert.That( north.Length, Is.EqualTo( 64 ) );
		Assert.That( north[ 5 ], Is.EqualTo( 2f ) );
		Assert.That( northPresence[ 0 ], Is.EqualTo( 1UL << 5 ) );
		Assert.That( south.IsEmpty, Is.True );
	}

	[Test]
	public void Neighbourhood_CopyColumn_GathersHaloColumn() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ -1, 3 ] = 4f;

		grid.TryGetNeighbourhood( 0, 0, out GridChunkNeighbourhood<float> n );
		Span<float> west = stackalloc float[ 64 ];
		Span<ulong> westPresence = stackalloc ulong[ 1 ];
		Span<float> east = stackalloc float[ 64 ];
		Span<ulong> eastPresence = stackalloc ulong[ 1 ];
		east.Fill( 9f );
		eastPresence[ 0 ] = ulong.MaxValue;

		bool hasWest = n.CopyColumn( -1, west, westPresence );
		bool hasEast = n.CopyColumn( 64, east, eastPresence );

		Assert.That( hasWest, Is.True );
		Assert.That( west[ 3 ], Is.EqualTo( 4f ) );
		Assert.That( westPresence[ 0 ], Is.EqualTo( 1UL << 3 ) );
		Assert.That( hasEast, Is.False );
		Assert.That( east[ 0 ], Is.Zero );
		Assert.That( eastPresence[ 0 ], Is.Zero );
	}

	[Test]
	public void EnumerateNeighbourhoods_VisitsEveryChunk() {
		ChunkedGrid<float> grid = new();
		grid[ 0, 0 ] = 1f;
		grid[ 64, 0 ] = 1f;

		int count = 0;
		int withEastOrWest = 0;
		foreach( GridChunkNeighbourhood<float> n in grid.EnumerateNeighbourhoods() ) {
			count++;
			if( n.HasNeighbour( 1, 0 ) || n.HasNeighbour( -1, 0 ) ) {
				withEastOrWest++;
			}
		}

		Assert.That( count, Is.EqualTo( 2 ) );
		Assert.That( withEastOrWest, Is.EqualTo( 2 ) );
	}

	[Test]
	public void GetChunk_ChunkIndexAndPopulatedCount_Reported() {
		ChunkedGrid<float> grid = new( 8 );
		grid[ 0, 0 ] = 1f;
		grid[ 9, 0 ] = 1f;
		grid[ 10, 1 ] = 1f;

		_ = grid.TryGetChunk( 1, 0, out GridChunk<float> chunk );
		_ = grid.TryGetNeighbourhood( 1, 0, out GridChunkNeighbourhood<float> neighbourhood );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( chunk.ChunkIndex, Is.EqualTo( 1 ) );
			Assert.That( chunk.PopulatedCount, Is.EqualTo( 2 ) );
			Assert.That( neighbourhood.ChunkIndex, Is.EqualTo( 1 ) );
			Assert.That( neighbourhood.Center.ChunkIndex, Is.EqualTo( 1 ) );
		}
	}

	[Test]
	public void TryGetPopulatedBounds_ExactCellBounds() {
		ChunkedGrid<float> grid = new( 8 );

		bool emptyFound = grid.TryGetPopulatedBounds( out _, out _, out _, out _ );
		grid[ -3, 2 ] = 1f;
		grid[ 12, 20 ] = 1f;
		grid[ 40, 5 ] = 1f;
		grid.Clear( 40, 5 );
		bool found = grid.TryGetPopulatedBounds( out int column, out int row, out int width, out int height );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( emptyFound, Is.False );
			Assert.That( found, Is.True );
			Assert.That( (column, row, width, height), Is.EqualTo( (-3, 2, 16, 19) ) );
			Assert.That( grid.Width, Is.EqualTo( 56 ) );
		}
	}

	private readonly struct ColumnExtractor : ICellExtractor<float, int> {

		public bool TryExtract(
			int sourceColumn,
			int sourceRow,
			in float source,
			out int value
		) {
			value = sourceColumn;
			return true;
		}
	}
}
