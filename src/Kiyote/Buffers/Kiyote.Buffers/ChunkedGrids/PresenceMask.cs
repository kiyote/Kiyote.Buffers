using System.Numerics;
using System.Runtime.Intrinsics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Expands the one-bit-per-cell presence of a <see cref="GridChunk{T}"/> row
/// into lane masks suitable for <c>ConditionalSelect</c> and bitwise blending.
/// </summary>
/// <remarks>
/// Use an integer mask type of the same width as the grid element, e.g.
/// <see cref="uint"/> for <see cref="float"/> grids (then reinterpret with
/// <c>Vector256.As</c>) or <see cref="byte"/> for byte-sized enum grids.
/// </remarks>
public static class PresenceMask {

	/// <summary>
	/// Writes <c>AllBitsSet</c> for populated cells and zero for unpopulated
	/// cells, starting at local column zero, for <c>mask.Length</c> cells.
	/// </summary>
	public static void Expand<TMask>(
		ReadOnlySpan<ulong> presenceRow,
		Span<TMask> mask
	) where TMask : unmanaged, IBinaryInteger<TMask> {
		Expand( presenceRow, 0, mask );
	}

	/// <summary>
	/// Writes <c>AllBitsSet</c> for populated cells and zero for unpopulated
	/// cells, starting at local column <paramref name="offset"/>.
	/// </summary>
	public static void Expand<TMask>(
		ReadOnlySpan<ulong> presenceRow,
		int offset,
		Span<TMask> mask
	) where TMask : unmanaged, IBinaryInteger<TMask> {
		for( int i = 0; i < mask.Length; i++ ) {
			int bit = offset + i;
			mask[ i ] = ( ( presenceRow[ bit >> 6 ] >> ( bit & 63 ) ) & 1UL ) != 0
				? TMask.AllBitsSet
				: TMask.Zero;
		}
	}

	/// <summary>
	/// Builds a <see cref="Vector256{T}"/> mask covering the
	/// <c>Vector256&lt;TMask&gt;.Count</c> cells starting at local column
	/// <paramref name="offset"/>.
	/// </summary>
	public static Vector256<TMask> ExpandVector256<TMask>(
		ReadOnlySpan<ulong> presenceRow,
		int offset
	) where TMask : unmanaged, IBinaryInteger<TMask> {
		Span<TMask> lanes = stackalloc TMask[ Vector256<TMask>.Count ];
		Expand( presenceRow, offset, lanes );
		return Vector256.Create<TMask>( lanes );
	}
}
