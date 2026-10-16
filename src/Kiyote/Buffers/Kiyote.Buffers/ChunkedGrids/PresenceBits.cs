using System.Runtime.CompilerServices;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Reads and writes runs of up to 64 presence bits starting at an arbitrary
/// bit offset within a row of presence words, where bit <c>c &amp; 63</c> of
/// word <c>c &gt;&gt; 6</c> describes column <c>c</c>.
/// </summary>
/// <remarks>
/// These are unchecked primitives for hot loops: arguments outside the
/// documented ranges produce unspecified bits, and reads or writes past the
/// end of the span throw <see cref="IndexOutOfRangeException"/>.
/// </remarks>
public static class PresenceBits {

	/// <summary>
	/// Gets a mask with the low <paramref name="count"/> bits set.
	/// </summary>
	/// <param name="count">The number of bits, 0 to 64.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ulong Mask(
		int count
	) {
		return count >= 64 ? ulong.MaxValue : ( 1UL << count ) - 1;
	}

	/// <summary>
	/// Reads <paramref name="count"/> bits starting at bit <paramref name="bitOffset"/>.
	/// </summary>
	/// <param name="presence">The presence words of the row.</param>
	/// <param name="bitOffset">The first bit (column) to read; non-negative.</param>
	/// <param name="count">The number of bits to read, 1 to 64.</param>
	/// <returns>Bit <c>i</c> is bit <c>bitOffset + i</c> of the row.</returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ulong Read(
		ReadOnlySpan<ulong> presence,
		int bitOffset,
		int count
	) {
		int word = bitOffset >> 6;
		int shift = bitOffset & 63;
		ulong bits = presence[ word ] >> shift;
		if( shift != 0 && shift + count > 64 ) {
			bits |= presence[ word + 1 ] << ( 64 - shift );
		}
		return bits & Mask( count );
	}

	/// <summary>
	/// Sets the bits of the row that are set in <paramref name="bits"/>,
	/// shifted to start at bit <paramref name="bitOffset"/>.
	/// </summary>
	/// <param name="presence">The presence words of the row.</param>
	/// <param name="bitOffset">The row bit that bit 0 of <paramref name="bits"/> maps to; non-negative.</param>
	/// <param name="bits">The bits to set.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Or(
		Span<ulong> presence,
		int bitOffset,
		ulong bits
	) {
		int word = bitOffset >> 6;
		int shift = bitOffset & 63;
		presence[ word ] |= bits << shift;
		if( shift != 0 ) {
			ulong carry = bits >> ( 64 - shift );
			if( carry != 0 ) {
				presence[ word + 1 ] |= carry;
			}
		}
	}

	/// <summary>
	/// Clears the bits of the row that are set in <paramref name="bits"/>,
	/// shifted to start at bit <paramref name="bitOffset"/>.
	/// </summary>
	/// <param name="presence">The presence words of the row.</param>
	/// <param name="bitOffset">The row bit that bit 0 of <paramref name="bits"/> maps to; non-negative.</param>
	/// <param name="bits">The bits to clear.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void AndNot(
		Span<ulong> presence,
		int bitOffset,
		ulong bits
	) {
		int word = bitOffset >> 6;
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
