using System.Runtime.CompilerServices;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// The value and presence arrays of a chunk and its eight neighbours, indexed
/// <c>( dy + 1 ) * 3 + ( dx + 1 )</c>; absent chunks have null arrays.
/// </summary>
[InlineArray( Length )]
internal struct NeighbourChunks<T> where T : unmanaged {

	public const int Length = ChunkedGridLayout.NeighbourCount;

	private NeighbourChunk<T> _element0;
}

/// <summary>
/// One resolved chunk of a <see cref="NeighbourChunks{T}"/>.
/// </summary>
internal readonly struct NeighbourChunk<T>(
	int index,
	T[]? values,
	ulong[]? presence
) where T : unmanaged {

	public int Index { get; } = index;

	public T[]? Values { get; } = values;

	public ulong[]? Presence { get; } = presence;
}
