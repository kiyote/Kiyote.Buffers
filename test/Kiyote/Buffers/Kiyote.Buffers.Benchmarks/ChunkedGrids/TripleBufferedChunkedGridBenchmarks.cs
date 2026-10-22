using BenchmarkDotNet.Attributes;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Buffers.Benchmarks.ChunkedGrids;

[MemoryDiagnoser]
public class TripleBufferedChunkedGridBenchmarks {

	private TripleBufferedChunkedGrid<float> _grid = null!;

	[GlobalSetup]
	public void GlobalSetup() {
		_grid = new TripleBufferedChunkedGrid<float>();
		ChunkedGrid<float> source = new();
		source[ 0, 0 ] = 1f;
		_grid.Attach( source, 0, 0 );
		_grid.Swap();
	}

	[Benchmark]
	public void Swap_NoLease() {
		_grid.Swap();
	}

	[Benchmark]
	public int Acquire_Dispose() {
		using TripleBufferedLease<float> lease = _grid.Acquire();
		return lease.ChunkCount;
	}

	[Benchmark]
	public void Swap_WhileLeased() {
		using TripleBufferedLease<float> lease = _grid.Acquire();
		_grid.Swap();
	}
}
