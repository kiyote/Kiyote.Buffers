namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Read-only views of the values of a chunk and its eight neighbours, as
/// consumed by <see cref="ChunkPadding"/>. An empty span means the chunk is
/// not allocated; every other span holds <c>size * size</c> elements, row-major.
/// </summary>
public readonly ref struct ChunkNeighbourSpans<T> {

	/// <summary>
	/// The chunk at offset (-1, -1).
	/// </summary>
	public ReadOnlySpan<T> NorthWest { get; init; }

	/// <summary>
	/// The chunk at offset (0, -1).
	/// </summary>
	public ReadOnlySpan<T> North { get; init; }

	/// <summary>
	/// The chunk at offset (1, -1).
	/// </summary>
	public ReadOnlySpan<T> NorthEast { get; init; }

	/// <summary>
	/// The chunk at offset (-1, 0).
	/// </summary>
	public ReadOnlySpan<T> West { get; init; }

	/// <summary>
	/// The centre chunk; must not be empty.
	/// </summary>
	public ReadOnlySpan<T> Centre { get; init; }

	/// <summary>
	/// The chunk at offset (1, 0).
	/// </summary>
	public ReadOnlySpan<T> East { get; init; }

	/// <summary>
	/// The chunk at offset (-1, 1).
	/// </summary>
	public ReadOnlySpan<T> SouthWest { get; init; }

	/// <summary>
	/// The chunk at offset (0, 1).
	/// </summary>
	public ReadOnlySpan<T> South { get; init; }

	/// <summary>
	/// The chunk at offset (1, 1).
	/// </summary>
	public ReadOnlySpan<T> SouthEast { get; init; }

	/// <summary>
	/// Gets a span by neighbour index <c>( dy + 1 ) * 3 + ( dx + 1 )</c>.
	/// </summary>
	internal ReadOnlySpan<T> this[ int neighbour ] => neighbour switch {
		0 => NorthWest,
		1 => North,
		2 => NorthEast,
		3 => West,
		4 => Centre,
		5 => East,
		6 => SouthWest,
		7 => South,
		8 => SouthEast,
		_ => throw new ArgumentOutOfRangeException( nameof( neighbour ) )
	};
}
