using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Buffers.Benchmarks.ChunkedGrids;

/// <summary>
/// Benchmarks <see cref="ChunkedGrid{T}"/> using two 100x100 blocks, one
/// attached at (0, 0) and one at (100, 98). The first is chunk-aligned and
/// uses the fast path; the second is not, and overlaps the first by two rows
/// of chunk space so the two only share a handful of rows.
/// </summary>
/// <remarks>
/// Attach/Detach overwrite or clear cells idempotently, so mutating
/// benchmarks run repeatedly against a persistent grid and measure the
/// steady state (no chunk allocation after the first invocation).
/// </remarks>
[MemoryDiagnoser( displayGenColumns: false )]
public class ChunkedGridBenchmarks {

	private const int BlockSize = 100;
	private const int SecondColumn = 100;
	private const int SecondRow = 98;

	private ChunkedGrid<float> _block = null!;
	private ChunkedGrid<float> _attachTarget = null!;
	private ChunkedGrid<float> _detachTarget = null!;
	private ChunkedGrid<float> _composed = null!;

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

		_attachTarget = Compose();
		_detachTarget = Compose();
		_composed = Compose();
	}

	[Benchmark]
	public ChunkedGrid<float> Compose_NewGrid() {
		return Compose();
	}

	[Benchmark]
	public void Attach_Aligned() {
		_attachTarget.Attach( _block, 0, 0 );
	}

	[Benchmark]
	public void Attach_Unaligned() {
		_attachTarget.Attach( _block, SecondColumn, SecondRow );
	}

	[Benchmark]
	public void Attach_StructExtractor_Unaligned() {
		_attachTarget.Attach( _block, SecondColumn, SecondRow, default( ScaleExtractor ) );
	}

	[Benchmark]
	public void Attach_FuncExtractor_Unaligned() {
		_attachTarget.Attach<float>( _block, SecondColumn, SecondRow, static v => (true, v * 0.5f) );
	}

	[Benchmark]
	public bool HasOverlap_Unaligned_NoOverlap() {
		// Placed to the left of both blocks, so every cell is checked.
		return _composed.HasOverlap( _block, -BlockSize, SecondRow );
	}

	[Benchmark]
	public void Detach_Unaligned() {
		_detachTarget.Detach( _block, SecondColumn, SecondRow );
	}

	[Benchmark]
	public float Indexer_Read() {
		float sum = 0f;
		for( int r = 0; r < SecondRow + BlockSize; r++ ) {
			for( int c = 0; c < SecondColumn + BlockSize; c++ ) {
				sum += _composed[ c, r ];
			}
		}
		return sum;
	}

	[Benchmark]
	public void Indexer_Write() {
		for( int r = 0; r < BlockSize; r++ ) {
			for( int c = 0; c < BlockSize; c++ ) {
				_attachTarget[ SecondColumn + c, SecondRow + r ] = c;
			}
		}
	}

	[Benchmark]
	public int TryGet_Populated() {
		int count = 0;
		for( int r = 0; r < SecondRow + BlockSize; r++ ) {
			for( int c = 0; c < SecondColumn + BlockSize; c++ ) {
				if( _composed.TryGet( c, r, out _ ) ) {
					count++;
				}
			}
		}
		return count;
	}

	[Benchmark]
	public float EnumerateChunks_MaskedVectorSum() {
		Vector256<float> total = Vector256<float>.Zero;
		Span<uint> mask = stackalloc uint[ Vector256<float>.Count ];
		foreach( GridChunk<float> chunk in _composed.EnumerateChunks() ) {
			for( int r = 0; r < chunk.Size; r++ ) {
				Span<float> row = chunk.GetRowValues( r );
				Span<ulong> presence = chunk.GetPresenceRow( r );
				for( int c = 0; c < row.Length; c += Vector256<float>.Count ) {
					PresenceMask.Expand( presence, c, mask );
					Vector256<float> values = Vector256.Create<float>( row.Slice( c, Vector256<float>.Count ) );
					Vector256<float> selected = Vector256.BitwiseAnd(
						values,
						Vector256.Create<uint>( mask ).AsSingle()
					);
					total += selected;
				}
			}
		}
		return Vector256.Sum( total );
	}

	[Benchmark]
	public float EnumerateNeighbourhoods_VerticalStencil() {
		float total = 0f;
		foreach( GridChunkNeighbourhood<float> n in _composed.EnumerateNeighbourhoods() ) {
			total += VerticalStencil( n );
		}
		return total;
	}

	[Benchmark]
	public int EnumerateNeighbourhoods_CopyColumns() {
		int found = 0;
		Span<float> values = stackalloc float[ ChunkSize ];
		Span<ulong> presence = stackalloc ulong[ Math.Max( 1, ChunkSize >> 6 ) ];
		foreach( GridChunkNeighbourhood<float> n in _composed.EnumerateNeighbourhoods() ) {
			if( n.CopyColumn( -1, values, presence ) ) {
				found++;
			}
			if( n.CopyColumn( n.Size, values, presence ) ) {
				found++;
			}
		}
		return found;
	}

	internal static float VerticalStencil(
		GridChunkNeighbourhood<float> n
	) {
		Vector256<float> total = Vector256<float>.Zero;
		Vector256<float> third = Vector256.Create( 1f / 3f );
		for( int r = 0; r < n.Size; r++ ) {
			Span<float> centre = n.GetRowValues( r );
			Span<float> up = n.GetRowValues( r - 1 );
			Span<float> down = n.GetRowValues( r + 1 );
			if( up.IsEmpty ) {
				up = centre;
			}
			if( down.IsEmpty ) {
				down = centre;
			}
			for( int c = 0; c < centre.Length; c += Vector256<float>.Count ) {
				Vector256<float> sum = Vector256.Create<float>( up.Slice( c, Vector256<float>.Count ) )
					+ Vector256.Create<float>( centre.Slice( c, Vector256<float>.Count ) )
					+ Vector256.Create<float>( down.Slice( c, Vector256<float>.Count ) );
				total += sum * third;
			}
		}
		return Vector256.Sum( total );
	}

	private ChunkedGrid<float> Compose() {
		ChunkedGrid<float> grid = new( ChunkSize );
		grid.Attach( _block, 0, 0 );
		grid.Attach( _block, SecondColumn, SecondRow );
		return grid;
	}

	private readonly struct ScaleExtractor : ICellExtractor<float, float> {
		public bool TryExtract(
			int sourceColumn,
			int sourceRow,
			in float source,
			out float value
		) {
			value = source * 0.5f;
			return true;
		}
	}
}
