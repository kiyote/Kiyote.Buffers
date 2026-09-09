using System.Buffers;

namespace Kiyote.Buffers;

/// <summary>
/// A FIFO queue of cell indices backed by fixed size pooled blocks.
/// </summary>
/// <remarks>
/// Every block is exactly <see cref="BlockSize"/> elements, which is the largest
/// bucket <see cref="ArrayPool{T}"/> will pool. Renting a single array sized to
/// the whole buffer would fall out of the pool once the buffer exceeded that
/// size and allocate on the large object heap on every call; blocks are always
/// pooled regardless of how large the buffer is. Blocks are also rented only as
/// the frontier demands and returned as soon as they are fully consumed, so the
/// live footprint tracks the frontier rather than the buffer.
/// </remarks>
internal ref struct ChunkedIndexQueue {

	// Largest bucket ArrayPool<int>.Shared will pool, as a power of two so the
	// block/offset arithmetic reduces to a shift and a mask.
	private const int BlockShift = 20;
	private const int BlockSize = 1 << BlockShift;
	private const int BlockMask = BlockSize - 1;

	private int[]?[] _blocks;
	private int _head;
	private int _tail;

	public ChunkedIndexQueue(
		int capacity
	) {
		int blockCount = ( capacity + BlockMask ) >>> BlockShift;
		if( blockCount < 1 ) {
			blockCount = 1;
		}
		_blocks = ArrayPool<int[]?>.Shared.Rent( blockCount );
		Array.Clear( _blocks, 0, _blocks.Length );
		_head = 0;
		_tail = 0;
	}

	public readonly bool IsEmpty => _head >= _tail;

	public void Enqueue(
		int index
	) {
		int block = _tail >>> BlockShift;
		int[]? storage = _blocks[ block ];
		if( storage is null ) {
			storage = ArrayPool<int>.Shared.Rent( BlockSize );
			_blocks[ block ] = storage;
		}

		storage[ _tail & BlockMask ] = index;
		_tail++;
	}

	public int Dequeue() {
		int block = _head >>> BlockShift;
		int offset = _head & BlockMask;
		int index = _blocks[ block ]![ offset ];
		_head++;

		// Release the block as soon as it is fully consumed so the live
		// footprint follows the frontier rather than the total enqueue count.
		if( offset == BlockMask ) {
			ArrayPool<int>.Shared.Return( _blocks[ block ]! );
			_blocks[ block ] = null;
		}

		return index;
	}

	public void Dispose() {
		int[]?[] blocks = _blocks;
		if( blocks is null ) {
			return;
		}

		for( int i = 0; i < blocks.Length; i++ ) {
			int[]? storage = blocks[ i ];
			if( storage is not null ) {
				ArrayPool<int>.Shared.Return( storage );
				blocks[ i ] = null;
			}
		}

		ArrayPool<int[]?>.Shared.Return( blocks );
		_blocks = null!;
	}
}
