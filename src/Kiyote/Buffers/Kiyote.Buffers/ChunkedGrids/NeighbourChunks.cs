using System.Runtime.CompilerServices;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// The storage of a chunk and its eight neighbours, indexed
/// <c>( dy + 1 ) * 3 + ( dx + 1 )</c>; absent chunks are <c>null</c>.
/// </summary>
[InlineArray( Length )]
internal struct NeighbourChunks<T> where T : unmanaged {

	public const int Length = 9;
	public const int CenterIndex = 4;

	private GridChunkStorage<T>? _element0;
}
