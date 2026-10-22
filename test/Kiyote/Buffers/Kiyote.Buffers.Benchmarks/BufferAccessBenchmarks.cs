using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser( displayGenColumns: false )]
public class BufferAccessBenchmarks {

	private IBuffer<int> _buffer = null!;

	[Params( false, true )]
	public bool Ragged { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_buffer = Ragged
			? new RaggedArrayBuffer<int>( 1000, 1000, 1 )
			: new FlatArrayBuffer<int>( 1000, 1000, 1 );
	}

	[Benchmark]
	public long IndexerRead() {
		long sum = 0;
		for( int row = 0; row < 1000; row++ ) {
			for( int column = 0; column < 1000; column++ ) {
				sum += _buffer[ column, row ];
			}
		}
		return sum;
	}

	[Benchmark]
	public long RowSpanRead() {
		long sum = 0;
		for( int row = 0; row < 1000; row++ ) {
			foreach( int value in _buffer.GetRowSpan( row ) ) {
				sum += value;
			}
		}
		return sum;
	}
}
