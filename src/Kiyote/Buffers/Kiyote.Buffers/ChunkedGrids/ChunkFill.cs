namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// How many cells of a chunk are populated.
/// </summary>
public enum ChunkFill {

	/// <summary>
	/// No cell is populated.
	/// </summary>
	Empty = 0,

	/// <summary>
	/// Some, but not all, cells are populated.
	/// </summary>
	Partial = 1,

	/// <summary>
	/// Every cell is populated.
	/// </summary>
	Full = 2
}
