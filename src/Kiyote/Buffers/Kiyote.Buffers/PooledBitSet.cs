using System.Buffers;

namespace Kiyote.Buffers;

/// <summary>
/// A pooled bitset used to track which cells a fill has already reached.
/// </summary>
/// <remarks>
/// Storing one bit per cell rather than one <see cref="bool"/> uses an eighth of
/// the memory, makes the initial clear eight times cheaper, and keeps far more
/// of the map resident in cache during the neighbour probes.
/// </remarks>
internal ref struct PooledBitSet {

	private const int BitShift = 6;
	private const int BitMask = ( 1 << BitShift ) - 1;

	private ulong[] _bits;

	public PooledBitSet(
		int capacity
	) {
		int wordCount = ( capacity + BitMask ) >>> BitShift;
		if( wordCount < 1 ) {
			wordCount = 1;
		}
		_bits = ArrayPool<ulong>.Shared.Rent( wordCount );
		Array.Clear( _bits, 0, wordCount );
	}

	public readonly bool IsSet(
		int index
	) {
		return ( _bits[ index >>> BitShift ] & ( 1UL << ( index & BitMask ) ) ) != 0;
	}

	public readonly void Set(
		int index
	) {
		_bits[ index >>> BitShift ] |= 1UL << ( index & BitMask );
	}

	public void Dispose() {
		ulong[] bits = _bits;
		if( bits is null ) {
			return;
		}

		ArrayPool<ulong>.Shared.Return( bits );
		_bits = null!;
	}
}
