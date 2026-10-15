namespace Kiyote.Buffers;

/// <summary>
/// Determines whether a given buffer value can be traversed.
/// </summary>
/// <remarks>
/// <para>
/// The analyzer needs to know which cells a fill may move through, but that
/// decision belongs to the caller's domain (e.g. "is this tile walkable?").
/// Implement this on a <see langword="readonly struct"/> and pass it to the
/// generic analyzer overloads so the JIT can specialize and inline the test,
/// avoiding a delegate invocation per cell.
/// </para>
/// <example>
/// <code>
/// public readonly struct OpenFloor : IPassable&lt;Tile&gt; {
///     public bool IsPassable( Tile value ) {
///         return value == Tile.Floor;
///     }
/// }
///
/// bool isSealed = analyzer.IsSealed( buffer, column, row, default( OpenFloor ) );
/// </code>
/// </example>
/// </remarks>
public interface IPassable<T> {

	bool IsPassable( T value );

}
