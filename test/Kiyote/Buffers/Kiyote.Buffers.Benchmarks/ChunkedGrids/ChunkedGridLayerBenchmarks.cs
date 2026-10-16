using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Buffers.Benchmarks.ChunkedGrids;

/// <summary>
/// Benchmarks <see cref="ChunkedGridLayer{T}"/> over a fully populated
/// 256x256 <see cref="ChunkedGridLayout"/> with one <see cref="float"/> layer,
/// comparing per-cell neighbourhood reads with a padded gather.
/// </summary>
[MemoryDiagnoser( displayGenColumns: false )]
public class ChunkedGridLayerBenchmarks {

	private const int Extent = 256;

	private ChunkedGridLayout _layout = null!;
	private ChunkedGridLayer<float> _layer = null!;
	private float[] _scratch = null!;

	[Params( 32, 64 )]
	public int ChunkSize { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_layout = new ChunkedGridLayout( ChunkSize );
		_layer = new ChunkedGridLayer<float>( _layout );
		for( int r = 0; r < Extent; r++ ) {
			_ = _layout.SetPopulatedRun( 0, r, Extent );
		}
		for( int r = 0; r < Extent; r++ ) {
			for( int c = 0; c < Extent; c++ ) {
				_layer.GetReference( c, r ) = ( ( r * 31 ) + c ) % 17;
			}
		}
		_scratch = new float[ ChunkPadding.GetPaddedLength( ChunkSize, 1 ) ];
	}

	[Benchmark]
	public float ElementWise_GetValues() {
		Vector256<float> total = Vector256<float>.Zero;
		foreach( int index in _layout.EnumerateChunks() ) {
			Span<float> values = _layer.GetValues( index );
			for( int i = 0; i < values.Length; i += Vector256<float>.Count ) {
				total += Vector256.Create<float>( values.Slice( i, Vector256<float>.Count ) );
			}
		}
		return Vector256.Sum( total );
	}

	[Benchmark]
	public float Stencil_TryGetValue() {
		float total = 0f;
		foreach( GridChunkNeighbourhood<float> n in _layer.EnumerateNeighbourhoods() ) {
			int size = n.Size;
			for( int r = 0; r < size; r++ ) {
				for( int c = 0; c < size; c++ ) {
					_ = n.TryGetValue( c, r, out float centre );
					float sum = centre;
					sum += n.TryGetValue( c, r - 1, out float north ) ? north : centre;
					sum += n.TryGetValue( c, r + 1, out float south ) ? south : centre;
					sum += n.TryGetValue( c - 1, r, out float west ) ? west : centre;
					sum += n.TryGetValue( c + 1, r, out float east ) ? east : centre;
					total += sum * 0.2f;
				}
			}
		}
		return total;
	}

	[Benchmark]
	public float Stencil_CopyPadded() {
		float total = 0f;
		float[] scratch = _scratch;
		foreach( GridChunkNeighbourhood<float> n in _layer.EnumerateNeighbourhoods() ) {
			int size = n.Size;
			int stride = size + 2;
			n.CopyPadded( 1, scratch );
			for( int r = 0; r < size; r++ ) {
				int row = ( ( r + 1 ) * stride ) + 1;
				for( int c = 0; c < size; c++ ) {
					int i = row + c;
					float sum = scratch[ i ]
						+ scratch[ i - stride ]
						+ scratch[ i + stride ]
						+ scratch[ i - 1 ]
						+ scratch[ i + 1 ];
					total += sum * 0.2f;
				}
			}
		}
		return total;
	}

	[Benchmark]
	public int ReadPresenceBits_VacancyMask() {
		int vacant = 0;
		int size = _layout.ChunkSize;
		int count = Math.Min( 64, size );
		foreach( int index in _layout.EnumerateChunks() ) {
			for( int r = 0; r < size; r++ ) {
				for( int c = 0; c < size; c += count ) {
					ulong north = _layout.ReadPresenceBits( index, c, r - 1, count );
					ulong centre = _layout.ReadPresenceBits( index, c, r, count );
					vacant += BitOperations.PopCount( centre & ~north );
				}
			}
		}
		return vacant;
	}
}
