using System.Runtime.CompilerServices;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Reads and writes runs of up to 64 presence bits starting at an arbitrary
/// bit offset within a row of presence words.
/// </summary>
internal static class PresenceBits {

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ulong Mask(
		int count
	) {
		return count >= 64 ? ulong.MaxValue : ( 1UL << count ) - 1;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ulong Read(
		ulong[] presence,
		int rowStart,
		int bitOffset,
		int count
	) {
		int word = rowStart + ( bitOffset >> 6 );
		int shift = bitOffset & 63;
		ulong bits = presence[ word ] >> shift;
		if( shift != 0 && shift + count > 64 ) {
			bits |= presence[ word + 1 ] << ( 64 - shift );
		}
		return bits & Mask( count );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Or(
		ulong[] presence,
		int rowStart,
		int bitOffset,
		ulong bits
	) {
		int word = rowStart + ( bitOffset >> 6 );
		int shift = bitOffset & 63;
		presence[ word ] |= bits << shift;
		if( shift != 0 ) {
			ulong carry = bits >> ( 64 - shift );
			if( carry != 0 ) {
				presence[ word + 1 ] |= carry;
			}
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void AndNot(
		ulong[] presence,
		int rowStart,
		int bitOffset,
		ulong bits
	) {
		int word = rowStart + ( bitOffset >> 6 );
		int shift = bitOffset & 63;
		presence[ word ] &= ~( bits << shift );
		if( shift != 0 ) {
			ulong carry = bits >> ( 64 - shift );
			if( carry != 0 ) {
				presence[ word + 1 ] &= ~carry;
			}
		}
	}
}
