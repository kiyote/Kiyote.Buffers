
namespace Kiyote.Buffers.Analyzers;

/// <summary>
/// Receives each cell reached by a flood fill.
/// </summary>
/// <remarks>
/// Implement this on a <see langword="struct"/> and pass it to
/// <see cref="IBufferAnalyzer.TryVisitSealedArea{T, TPassable, TVisitor}(IBuffer{T}, int, int, TPassable, ref TVisitor)"/>
/// to process a sealed region without ever materializing it into a collection.
/// The visitor is passed by <see langword="ref"/>, so any state it accumulates
/// is visible to the caller once the fill completes.
/// <para>
/// Implementing a struct visitor, rather than collecting cells into a list,
/// lets the JIT inline <see cref="Visit"/> and avoids allocating storage for
/// regions that may contain millions of cells.
/// </para>
/// <example>
/// <code>
/// public struct AreaCounter : ICellVisitor&lt;Tile&gt; {
///     public int Count;
///
///     public void Visit( int column, int row, Tile value ) {
///         Count++;
///     }
/// }
///
/// AreaCounter counter = default;
/// if( analyzer.TryVisitSealedArea( buffer, column, row, default( OpenFloor ), ref counter ) ) {
///     Console.WriteLine( counter.Count );
/// }
/// </code>
/// </example>
/// </remarks>
public interface ICellVisitor<T> {

	void Visit( int column, int row, T value );

}

