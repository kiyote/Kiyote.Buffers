namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Extracts the value to store in a <see cref="ChunkedGrid{T}"/> from a
/// populated cell of a source grid being attached.
/// </summary>
/// <remarks>
/// <para>
/// Attaching a grid copies cells between grids that may hold different
/// element types (e.g. a grid of rich cell structs into a grid of
/// <see cref="float"/> heights). The grid cannot know how to convert between
/// them, or which source cells should become populated, so the caller
/// supplies that logic through this interface. Returning <c>false</c> leaves
/// the target cell unpopulated, allowing irregular shapes to be carved out
/// during the copy.
/// </para>
/// <para>
/// Implement this on a <see langword="readonly struct"/> and pass it to the
/// generic <see cref="ChunkedGrid{T}.Attach{TSource, TExtractor}(ChunkedGrid{TSource}, int, int, TExtractor)"/>
/// overload so the JIT can specialize and inline the call, avoiding a delegate
/// invocation per cell.
/// </para>
/// <example>
/// <code>
/// public readonly struct HeightExtractor : ICellExtractor&lt;MapCell, float&gt; {
///     public bool TryExtract( int sourceColumn, int sourceRow, in MapCell source, out float value ) {
///         value = source.Height;
///         return source.Terrain != Terrain.None;
///     }
/// }
///
/// heights.Attach( cells, 128, 64, default( HeightExtractor ) );
/// </code>
/// </example>
/// </remarks>
public interface ICellExtractor<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	/// <param name="sourceColumn">The column in the source grid's own coordinate space.</param>
	/// <param name="sourceRow">The row in the source grid's own coordinate space.</param>
	/// <param name="source">The source cell value.</param>
	/// <param name="value">The value to store in the target grid.</param>
	/// <returns><c>false</c> to leave the target cell unpopulated.</returns>
	bool TryExtract( int sourceColumn, int sourceRow, in TSource source, out T value );

}
