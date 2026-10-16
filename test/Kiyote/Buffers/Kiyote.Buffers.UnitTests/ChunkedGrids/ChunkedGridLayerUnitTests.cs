namespace Kiyote.Buffers.ChunkedGrids.UnitTests;

[TestFixture]
public class ChunkedGridLayerUnitTests {

	private enum Terrain : byte {
		None,
		Land,
		Water
	}

	[Test]
	public void Ctor_NullLayout_Throws() {
		Assert.That( () => _ = new ChunkedGridLayer<float>( null! ), Throws.ArgumentNullException );
	}

	[Test]
	public void Ctor_ByteChunkSmallerThanVector256_Throws() {
		ChunkedGridLayout layout = new( 16 );

		Assert.That(
			() => _ = new ChunkedGridLayer<Terrain>( layout ),
			Throws.InstanceOf<ArgumentOutOfRangeException>().With.Property( "ParamName" ).EqualTo( "layout" )
		);
	}

	[Test]
	public void Ctor_ValidLaneSizes_Succeed() {
		ChunkedGridLayer<Terrain> bytes = new( new ChunkedGridLayout( 32 ) );
		ChunkedGridLayer<float> floats = new( new ChunkedGridLayout( 8 ) );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( bytes.ChunkSize, Is.EqualTo( 32 ) );
			Assert.That( floats.ChunkSize, Is.EqualTo( 8 ) );
		}
	}

	[Test]
	public void Layers_DifferentTypes_SeeChunksAllocatedLater_AsDefault() {
		ChunkedGridLayout layout = new( 32 );
		ChunkedGridLayer<float> pressure = new( layout );
		ChunkedGridLayer<Terrain> terrain = new( layout );

		_ = layout.SetPopulated( 5, 6 );
		_ = layout.TryGetChunk( 0, 0, out int index );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( pressure.GetValues( index ).ContainsAnyExcept( 0f ), Is.False );
			Assert.That( terrain.GetValues( index ).ContainsAnyExcept( Terrain.None ), Is.False );
			Assert.That( pressure.TryGet( 5, 6, out float value ), Is.True );
			Assert.That( value, Is.Zero );
			Assert.That( terrain.TrySet( 5, 6, Terrain.Water ), Is.True );
			Assert.That( terrain.TryGet( 5, 6, out Terrain cell ), Is.True );
			Assert.That( cell, Is.EqualTo( Terrain.Water ) );
		}
	}

	[Test]
	public void ReusedIndex_BetweenLayerCalls_ReadsDefault() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		int first = layout.GetOrCreateChunk( 0, 0 );
		layer.GetValues( first ).Fill( 3f );

		_ = layout.RemoveChunk( 0, 0 );
		int reused = layout.GetOrCreateChunk( 7, 7 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( reused, Is.EqualTo( first ) );
			Assert.That( layer.GetValues( reused ).ContainsAnyExcept( 0f ), Is.False );
		}
	}

	[Test]
	public void GetValues_RemovedChunk_Throws() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		int index = layout.GetOrCreateChunk( 0, 0 );
		_ = layout.RemoveChunk( 0, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => layer.GetValues( index ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetValues( -1 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetValues( 99 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetNeighbourhood( index ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
		}
	}

	[Test]
	public void GetValues_GetRowValues_GetChunk_Shapes() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		_ = layout.GetOrCreateChunk( 3, 3 );
		_ = layout.SetPopulated( -6, 2 );
		_ = layout.TryGetChunk( -1, 0, out int index );

		layer.GetRowValues( index, 2 )[ 2 ] = 9f;
		GridChunk<float> chunk = layer.GetChunk( index );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layer.GetValues( index ).Length, Is.EqualTo( 64 ) );
			Assert.That( layer.GetValues( index )[ ( 2 * 8 ) + 2 ], Is.EqualTo( 9f ) );
			Assert.That( layer.GetRowValues( index, 2 ).Length, Is.EqualTo( 8 ) );
			Assert.That( () => layer.GetRowValues( index, 8 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( chunk.ChunkIndex, Is.EqualTo( index ) );
			Assert.That( chunk.Column, Is.EqualTo( -8 ) );
			Assert.That( chunk.IsPopulated( 2, 2 ), Is.True );
			Assert.That( chunk.PopulatedCount, Is.EqualTo( 1 ) );
			Assert.That( chunk.Values[ ( 2 * 8 ) + 2 ], Is.EqualTo( 9f ) );
			Assert.That( layout.GetPresence( index ).Overlaps( chunk.Presence ), Is.True );
		}
	}

	[Test]
	public void GetReference_WritesVisible_UnpopulatedThrows() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		_ = layout.SetPopulated( 4, 4 );

		layer.GetReference( 4, 4 ) = 2.5f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layer.TryGet( 4, 4, out float value ), Is.True );
			Assert.That( value, Is.EqualTo( 2.5f ) );
			Assert.That( () => layer.GetReference( 5, 4 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( () => layer.GetReference( 100, 4 ), Throws.InstanceOf<ArgumentOutOfRangeException>() );
			Assert.That( layer.TrySet( 5, 4, 1f ), Is.False );
			Assert.That( layer.TryGet( 5, 4, out float missing ), Is.False );
			Assert.That( missing, Is.Zero );
		}
	}

	[Test]
	public void ClearPopulated_LeavesValue() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		_ = layout.SetPopulated( 1, 1 );
		layer.GetReference( 1, 1 ) = 4f;

		_ = layout.ClearPopulated( 1, 1 );
		_ = layout.TryGetChunk( 0, 0, out int index );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layer.TryGet( 1, 1, out _ ), Is.False );
			Assert.That( layer.GetValues( index )[ 9 ], Is.EqualTo( 4f ) );
		}
	}

	[Test]
	public void SwapStorage_ExchangesValues() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> a = new( layout );
		ChunkedGridLayer<float> b = new( layout );
		_ = layout.SetPopulated( 0, 0 );
		a.GetReference( 0, 0 ) = 1f;
		b.GetReference( 0, 0 ) = 2f;

		a.SwapStorage( b );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( a.TryGet( 0, 0, out float av ) && av == 2f, Is.True );
			Assert.That( b.TryGet( 0, 0, out float bv ) && bv == 1f, Is.True );
		}
	}

	[Test]
	public void SwapStorage_AfterMissedSync_BothLayersCurrent() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> a = new( layout );
		ChunkedGridLayer<float> b = new( layout );
		_ = layout.SetPopulated( 0, 0 );
		a.GetReference( 0, 0 ) = 1f;

		// Neither layer has seen this chunk yet.
		_ = layout.SetPopulated( 20, 0 );
		a.SwapStorage( b );
		b.GetReference( 20, 0 ) = 5f;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( b.TryGet( 0, 0, out float bv ) && bv == 1f, Is.True );
			Assert.That( a.TryGet( 0, 0, out float av ) && av == 0f, Is.True );
			Assert.That( a.TryGet( 20, 0, out float a20 ) && a20 == 0f, Is.True );
			Assert.That( b.TryGet( 20, 0, out float b20 ) && b20 == 5f, Is.True );
		}
	}

	[Test]
	public void SwapStorage_CopyTo_InvalidOther_Throws() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		ChunkedGridLayer<float> foreign = new( new ChunkedGridLayout( 8 ) );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => layer.SwapStorage( layer ), Throws.ArgumentException );
			Assert.That( () => layer.SwapStorage( foreign ), Throws.ArgumentException );
			Assert.That( () => layer.SwapStorage( null! ), Throws.ArgumentNullException );
			Assert.That( () => layer.CopyTo( layer ), Throws.ArgumentException );
			Assert.That( () => layer.CopyTo( foreign ), Throws.ArgumentException );
		}
	}

	[Test]
	public void CopyTo_CopiesEveryChunk_ClearValues_ResetsAll() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> source = new( layout );
		ChunkedGridLayer<float> target = new( layout );
		_ = layout.SetPopulatedRun( -4, 0, 20 );
		foreach( GridChunk<float> chunk in source.EnumerateChunks() ) {
			chunk.Values.Fill( chunk.ChunkIndex + 1 );
		}

		source.CopyTo( target );
		bool copied = true;
		foreach( GridChunk<float> chunk in target.EnumerateChunks() ) {
			copied &= !chunk.Values.ContainsAnyExcept( chunk.ChunkIndex + 1f );
		}
		source.ClearValues();
		bool cleared = true;
		foreach( GridChunk<float> chunk in source.EnumerateChunks() ) {
			cleared &= !chunk.Values.ContainsAnyExcept( 0f );
		}

		using( Assert.EnterMultipleScope() ) {
			Assert.That( copied, Is.True );
			Assert.That( cleared, Is.True );
			Assert.That( layout.IsPopulated( 0, 0 ), Is.True );
		}
	}

	[Test]
	public void EnumerateChunks_EnumerateNeighbourhoods_VisitAllWithLayerValues() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		ChunkedGridLayer<float> other = new( layout );
		_ = layout.SetPopulated( 0, 0 );
		_ = layout.SetPopulated( 8, 0 );
		_ = layout.SetPopulated( 0, -8 );
		layer.GetReference( 8, 0 ) = 7f;
		other.GetReference( 8, 0 ) = -1f;

		int chunks = 0;
		foreach( GridChunk<float> _ in layer.EnumerateChunks() ) {
			chunks++;
		}
		int neighbourhoods = 0;
		float east = 0f;
		foreach( GridChunkNeighbourhood<float> n in layer.EnumerateNeighbourhoods() ) {
			neighbourhoods++;
			if( n.ChunkIndex == 0 ) {
				_ = n.TryGetValue( 8, 0, out east );
			}
		}

		using( Assert.EnterMultipleScope() ) {
			Assert.That( chunks, Is.EqualTo( 3 ) );
			Assert.That( neighbourhoods, Is.EqualTo( 3 ) );
			Assert.That( east, Is.EqualTo( 7f ) );
		}
	}

	[Test]
	public void EnumerateChunks_AllocationDuringEnumeration_Throws() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> layer = new( layout );
		_ = layout.GetOrCreateChunk( 0, 0 );

		Assert.That( () => {
			foreach( GridChunk<float> chunk in layer.EnumerateChunks() ) {
				_ = layout.GetOrCreateChunk( chunk.ChunkColumn + 1, 0 );
			}
		}, Throws.InvalidOperationException );
	}

	[Test]
	public void Ctor_AfterCellsPopulated_ReadsDefault() {
		ChunkedGridLayout layout = new( 8 );
		ChunkedGridLayer<float> first = new( layout );
		_ = layout.SetPopulatedRun( 0, 0, 30 );
		foreach( GridChunk<float> chunk in first.EnumerateChunks() ) {
			chunk.Values.Fill( 1f );
		}

		ChunkedGridLayer<float> late = new( layout );
		bool allDefault = true;
		foreach( GridChunk<float> chunk in late.EnumerateChunks() ) {
			allDefault &= !chunk.Values.ContainsAnyExcept( 0f );
		}

		Assert.That( allDefault, Is.True );
	}
}
