namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public sealed class BufferAnalyzerUnitTests {

	private IBufferAnalyzer _analyzer;

	[OneTimeSetUp]
	public void SetUp() {
		_analyzer = new BufferAnalyzer();
	}

	[Test]
	public void IsSealed_SealedArea_ReturnsTrue() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );

		buffer[ 1, 1 ] = false;
		buffer[ 2, 1 ] = false;
		buffer[ 3, 1 ] = false;
		buffer[ 1, 2 ] = false;
		buffer[ 3, 2 ] = false;
		buffer[ 1, 3 ] = false;
		buffer[ 2, 3 ] = false;
		buffer[ 3, 3 ] = false;

		bool result = _analyzer.IsSealed( buffer, 2, 2, isPassable => isPassable );

		Assert.That( result, Is.True );
	}

	[Test]
	public void IsSealed_UnsealedArea_ReturnsFalse() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );

		buffer[ 1, 1 ] = false;
		buffer[ 2, 1 ] = true;
		buffer[ 3, 1 ] = false;
		buffer[ 1, 2 ] = false;
		buffer[ 3, 2 ] = false;
		buffer[ 1, 3 ] = false;
		buffer[ 2, 3 ] = false;
		buffer[ 3, 3 ] = false;

		bool result = _analyzer.IsSealed( buffer, 2, 2, isPassable => isPassable );

		Assert.That( result, Is.False );
	}

	[Test]
	public void IsSealed_ImpassableStart_ReturnsTrue() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );

		buffer[ 2, 2 ] = false;

		bool result = _analyzer.IsSealed( buffer, 2, 2, isPassable => isPassable );

		Assert.That( result, Is.True );
	}

	[Test]
	public void IsSealed_StructPassable_SealedArea_ReturnsTrue() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();

		bool result = _analyzer.IsSealed( buffer, 2, 2, new BoolPassable() );

		Assert.That( result, Is.True );
	}

	[Test]
	public void IsSealed_StructPassable_UnsealedArea_ReturnsFalse() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();
		buffer[ 2, 1 ] = true;

		bool result = _analyzer.IsSealed( buffer, 2, 2, new BoolPassable() );

		Assert.That( result, Is.False );
	}

	[Test]
	public void TryGetSealedArea_SingleCellArea_ReturnsCell() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			isPassable => isPassable,
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( area, Is.EquivalentTo( [new BufferCell<bool>( 2, 2, true )] ) );
		} );
	}

	[Test]
	public void TryGetSealedArea_MultiCellArea_ReturnsAllCells() {
		// A 5x3 room carved out of a 7x5 wall of impassable cells
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 7, 5, false );
		for( int r = 1; r <= 3; r++ ) {
			for( int c = 1; c <= 5; c++ ) {
				buffer[ c, r ] = true;
			}
		}

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			3,
			2,
			isPassable => isPassable,
			out IReadOnlyList<BufferCell<bool>> area
		);

		List<BufferCell<bool>> expected = [];
		for( int r = 1; r <= 3; r++ ) {
			for( int c = 1; c <= 5; c++ ) {
				expected.Add( new BufferCell<bool>( c, r, true ) );
			}
		}

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( area, Has.Count.EqualTo( 15 ) );
			Assert.That( area, Is.EquivalentTo( expected ) );
		} );
	}

	[Test]
	public void TryGetSealedArea_ValuesMatchBuffer() {
		// Distinct values so the reported cell values can be verified by position
		RaggedArrayBuffer<int> buffer = new RaggedArrayBuffer<int>( 5, 5, 0 );
		for( int r = 0; r < 5; r++ ) {
			for( int c = 0; c < 5; c++ ) {
				buffer[ c, r ] = ( r * 5 ) + c;
			}
		}
		// Wall the ring around the 2,2 / 2,3 pair off with a sentinel
		buffer[ 1, 1 ] = -1;
		buffer[ 2, 1 ] = -1;
		buffer[ 3, 1 ] = -1;
		buffer[ 1, 2 ] = -1;
		buffer[ 3, 2 ] = -1;
		buffer[ 1, 3 ] = -1;
		buffer[ 3, 3 ] = -1;
		buffer[ 2, 4 ] = -1;

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			value => value != -1,
			out IReadOnlyList<BufferCell<int>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( area, Is.EquivalentTo( [
				new BufferCell<int>( 2, 2, 12 ),
				new BufferCell<int>( 2, 3, 17 )
			] ) );
		} );
	}

	[Test]
	public void TryGetSealedArea_UnsealedArea_ReturnsFalseAndEmptyArea() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();
		buffer[ 2, 1 ] = true;

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			isPassable => isPassable,
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.False );
			Assert.That( area, Is.Empty );
		} );
	}

	[Test]
	public void TryGetSealedArea_ImpassableStart_ReturnsTrueAndEmptyArea() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );

		buffer[ 2, 2 ] = false;

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			isPassable => isPassable,
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( area, Is.Empty );
		} );
	}

	[Test]
	public void TryGetSealedArea_StructPassable_SealedArea_ReturnsCells() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			new BoolPassable(),
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( area, Is.EquivalentTo( [new BufferCell<bool>( 2, 2, true )] ) );
		} );
	}

	[Test]
	public void TryGetSealedArea_StructPassable_UnsealedArea_ReturnsFalseAndEmptyArea() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();
		buffer[ 2, 1 ] = true;

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			new BoolPassable(),
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.False );
			Assert.That( area, Is.Empty );
		} );
	}

	[Test]
	public void TryGetSealedArea_EntireBufferPassable_ReturnsFalse() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );

		bool result = _analyzer.TryGetSealedArea(
			buffer,
			2,
			2,
			isPassable => isPassable,
			out IReadOnlyList<BufferCell<bool>> area
		);

		Assert.Multiple( () => {
			Assert.That( result, Is.False );
			Assert.That( area, Is.Empty );
		} );
	}

	[Test]
	public void TryGetSealedArea_RepeatedCalls_ReturnConsistentResults() {
		// The fill relies on pooled state, so it must be correct across calls
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();

		for( int i = 0; i < 5; i++ ) {
			bool result = _analyzer.TryGetSealedArea(
				buffer,
				2,
				2,
				isPassable => isPassable,
				out IReadOnlyList<BufferCell<bool>> area
			);

			Assert.Multiple( () => {
				Assert.That( result, Is.True );
				Assert.That( area, Is.EquivalentTo( [new BufferCell<bool>( 2, 2, true )] ) );
			} );
		}
	}

	[Test]
	public void TryVisitSealedArea_SealedArea_VisitsEveryCell() {
		// A 5x3 room carved out of a 7x5 wall of impassable cells
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 7, 5, false );
		for( int r = 1; r <= 3; r++ ) {
			for( int c = 1; c <= 5; c++ ) {
				buffer[ c, r ] = true;
			}
		}

		List<BufferCell<bool>> visited = [];
		var visitor = new CollectingVisitor( visited );
		bool result = _analyzer.TryVisitSealedArea( buffer, 3, 2, new BoolPassable(), ref visitor );

		List<BufferCell<bool>> expected = [];
		for( int r = 1; r <= 3; r++ ) {
			for( int c = 1; c <= 5; c++ ) {
				expected.Add( new BufferCell<bool>( c, r, true ) );
			}
		}

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( visited, Is.EquivalentTo( expected ) );
		} );
	}

	[Test]
	public void TryVisitSealedArea_UnsealedArea_ReturnsFalse() {
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();
		buffer[ 2, 1 ] = true;

		List<BufferCell<bool>> visited = [];
		var visitor = new CollectingVisitor( visited );
		bool result = _analyzer.TryVisitSealedArea( buffer, 2, 2, new BoolPassable(), ref visitor );

		Assert.That( result, Is.False );
	}

	[Test]
	public void TryVisitSealedArea_MutationsVisibleToCaller() {
		// The visitor is passed by ref, so state it accumulates must survive
		RaggedArrayBuffer<bool> buffer = BuildSealedBuffer();

		var visitor = new CountingVisitor();
		bool result = _analyzer.TryVisitSealedArea( buffer, 2, 2, new BoolPassable(), ref visitor );

		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( visitor.Count, Is.EqualTo( 1 ) );
		} );
	}

	[Test]
	public void IsSealed_BufferLargerThanPoolMaximum_ReturnsTrue() {
		// 1200x1200 exceeds the 2^20 element ArrayPool bucket limit, which the
		// chunked queue and bitset are designed to keep working
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 1200, 1200, true );
		for( int i = 0; i < 1200; i++ ) {
			buffer[ i, 1 ] = false;
			buffer[ 1, i ] = false;
			buffer[ 1198, i ] = false;
			buffer[ i, 1198 ] = false;
		}

		bool result = _analyzer.IsSealed( buffer, 600, 600, new BoolPassable() );

		Assert.That( result, Is.True );
	}

	[Test]
	public void TryVisitSealedArea_LargeBuffer_VisitsExpectedCellCount() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 1200, 1200, true );
		for( int i = 0; i < 1200; i++ ) {
			buffer[ i, 1 ] = false;
			buffer[ 1, i ] = false;
			buffer[ 1198, i ] = false;
			buffer[ i, 1198 ] = false;
		}

		var visitor = new CountingVisitor();
		bool result = _analyzer.TryVisitSealedArea( buffer, 600, 600, new BoolPassable(), ref visitor );

		// The open interior spans columns 2..1197 and rows 2..1197
		Assert.Multiple( () => {
			Assert.That( result, Is.True );
			Assert.That( visitor.Count, Is.EqualTo( 1196 * 1196 ) );
		} );
	}

	/// <summary>
	/// Builds a 5x5 buffer whose only passable cell is 2,2, walled in by an
	/// impassable ring.
	/// </summary>
	private static RaggedArrayBuffer<bool> BuildSealedBuffer() {
		RaggedArrayBuffer<bool> buffer = new RaggedArrayBuffer<bool>( 5, 5, true );


		buffer[ 1, 1 ] = false;
		buffer[ 2, 1 ] = false;
		buffer[ 3, 1 ] = false;
		buffer[ 1, 2 ] = false;
		buffer[ 3, 2 ] = false;
		buffer[ 1, 3 ] = false;
		buffer[ 2, 3 ] = false;
		buffer[ 3, 3 ] = false;

		return buffer;
	}

	private readonly struct BoolPassable : IPassable<bool> {

		public bool IsPassable(
			bool value
		) {
			return value;
		}
	}

	private readonly struct CollectingVisitor : ICellVisitor<bool> {

		private readonly List<BufferCell<bool>> _cells;

		public CollectingVisitor(
			List<BufferCell<bool>> cells
		) {
			_cells = cells;
		}

		public readonly void Visit(
			int column,
			int row,
			bool value
		) {
			_cells.Add( new BufferCell<bool>( column, row, value ) );
		}
	}

	private struct CountingVisitor : ICellVisitor<bool> {

		public int Count { get; private set; }

		public void Visit(
			int column,
			int row,
			bool value
		) {
			Count++;
		}
	}
}
