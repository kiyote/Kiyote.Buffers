using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser( displayGenColumns: false )]
public class BufferAnalyzerBenchmarks {

	private readonly RaggedArrayBuffer<bool> _buffer100;
	private readonly RaggedArrayBuffer<bool> _buffer1000;
	private readonly RaggedArrayBuffer<bool> _buffer1200;

	private readonly IBufferAnalyzer _analyzer;

	private readonly BoolPassable _boolPassable;

	public BufferAnalyzerBenchmarks() {
		_analyzer = new BufferAnalyzer();
		_boolPassable = new BoolPassable();
		_buffer100 = new RaggedArrayBuffer<bool>( 100, 100, true );
		SealBuffer( _buffer100 );
		_buffer1000 = new RaggedArrayBuffer<bool>( 1000, 1000, true );
		SealBuffer( _buffer1000 );
		_buffer1200 = new RaggedArrayBuffer<bool>( 1200, 1200, true );
		SealBuffer( _buffer1200 );
	}

	private static void SealBuffer(
		RaggedArrayBuffer<bool> buffer
	) {
		// Top
		for (int i = 0; i < buffer.Columns; i++ ) {
			buffer[ i, 1 ] = false;
		}
		// Left
		for( int i = 0; i < buffer.Rows; i++ ) {
			buffer[ 1, i ] = false;
		}
		// Right
		for( int i = 0; i < buffer.Rows; i++ ) {
			buffer[ buffer.Columns - 2, i ] = false;
		}
		// Bottom
		for( int i = 0; i < buffer.Columns; i++ ) {
			buffer[ i, buffer.Rows - 2 ] = false;
		}
	}

	[Benchmark]
	public void IsSealed_100x100_Delegate() {
		_ = _analyzer.IsSealed( _buffer100, 49, 49, isPassable => isPassable);
	}

	[Benchmark]
	public void IsSealed_1000x1000_Delegate() {
		_ = _analyzer.IsSealed( _buffer1000, 499, 499, isPassable => isPassable );
	}

	[Benchmark]
	public void IsSealed_100x100_Predicate() {
		_ = _analyzer.IsSealed( _buffer100, 49, 49, _boolPassable );
	}

	[Benchmark]
	public void IsSealed_1000x1000_Predicate() {
		_ = _analyzer.IsSealed( _buffer1000, 499, 499, _boolPassable );
	}

	[Benchmark]
	public void IsSealed_1200x1200_Predicate() {
		// 1,440,000 cells, past the 2^20 element ArrayPool bucket limit
		_ = _analyzer.IsSealed( _buffer1200, 599, 599, _boolPassable );
	}

	[Benchmark]
	public void TryGetSealedArea_1000x1000_Predicate() {
		_ = _analyzer.TryGetSealedArea( _buffer1000, 499, 499, _boolPassable, out _ );
	}

	[Benchmark]
	public void TryVisitSealedArea_1000x1000_Predicate() {
		var visitor = new CountingVisitor();
		_ = _analyzer.TryVisitSealedArea( _buffer1000, 499, 499, _boolPassable, ref visitor );
	}

	private readonly struct BoolPassable : IPassable<bool> {

		public bool IsPassable(
			bool value
		) {
			return value;
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
