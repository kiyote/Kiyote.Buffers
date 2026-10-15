using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Buffers.Benchmarks.ChunkedGrids;

/// <summary>
/// Benchmarks <see cref="DoubleBufferedChunkedGrid{T}"/> using the same
/// layout as <see cref="ChunkedGridBenchmarks"/>: a 100x100 block attached
/// at (0, 0) and another at (100, 98).
/// </summary>
[MemoryDiagnoser( displayGenColumns: false )]
public class DoubleBufferedChunkedGridBenchmarks {

	private const int BlockSize = 100;
	private const int SecondColumn = 100;
	private const int SecondRow = 98;

	private ChunkedGrid<float> _block = null!;
	private DoubleBufferedChunkedGrid<float> _grid = null!;

	[Params( 32, 64, 128 )]
	public int ChunkSize { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_block = new ChunkedGrid<float>( ChunkSize );
		for( int r = 0; r < BlockSize; r++ ) {
			for( int c = 0; c < BlockSize; c++ ) {
				_block[ c, r ] = ( r * BlockSize ) + c;
			}
		}
		_grid = new DoubleBufferedChunkedGrid<float>( ChunkSize );
		_grid.Attach( _block, 0, 0 );
		_grid.Attach( _block, SecondColumn, SecondRow );
		_grid.CopyCurrentToAlternate();
	}

	[GlobalCleanup]
	public void GlobalCleanup() {
		_block.Dispose();
		_grid.Dispose();
	}

	[Benchmark]
	public void Attach_Aligned() {
		_grid.Attach( _block, 0, 0 );
	}

	[Benchmark]
	public void Attach_Unaligned() {
		_grid.Attach( _block, SecondColumn, SecondRow );
	}

	[Benchmark]
	public void CopyCurrentToAlternate() {
		_grid.CopyCurrentToAlternate();
	}

	[Benchmark]
	public int TrySet_Populated() {
		int written = 0;
		for( int r = 0; r < BlockSize; r++ ) {
			for( int c = 0; c < BlockSize; c++ ) {
				if( _grid.TrySet( SecondColumn + c, SecondRow + r, GridBuffer.Current, c ) ) {
					written++;
				}
			}
		}
		return written;
	}

	/// <summary>
	/// A full simulation step: a vertical 3-tap stencil reading the alternate
	/// buffer (previous state) and writing the current buffer, then a swap.
	/// </summary>
	[Benchmark]
	public void Step_VerticalDiffusion() {
		Vector256<float> third = Vector256.Create( 1f / 3f );
		foreach( BufferedNeighbourhood<float> n in _grid.EnumerateNeighbourhoods() ) {
			GridChunkNeighbourhood<float> previous = n.Alternate;
			GridChunk<float> target = n.Current.Center;
			for( int r = 0; r < target.Size; r++ ) {
				Span<float> centre = previous.GetRowValues( r );
				Span<float> up = previous.GetRowValues( r - 1 );
				Span<float> down = previous.GetRowValues( r + 1 );
				if( up.IsEmpty ) {
					up = centre;
				}
				if( down.IsEmpty ) {
					down = centre;
				}
				Span<float> output = target.GetRowValues( r );
				for( int c = 0; c < output.Length; c += Vector256<float>.Count ) {
					Vector256<float> sum = Vector256.Create<float>( up.Slice( c, Vector256<float>.Count ) )
						+ Vector256.Create<float>( centre.Slice( c, Vector256<float>.Count ) )
						+ Vector256.Create<float>( down.Slice( c, Vector256<float>.Count ) );
					( sum * third ).CopyTo( output.Slice( c, Vector256<float>.Count ) );
				}
			}
		}
		_grid.Swap();
	}
}
