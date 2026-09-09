namespace Kiyote.Buffers;

/// <summary>
/// Determines whether a given buffer value can be traversed.
/// </summary>
/// <remarks>
/// Implement this on a <see langword="readonly struct"/> and pass it to the
/// generic analyzer overloads so the JIT can specialize and inline the test,
/// avoiding a delegate invocation per cell.
/// </remarks>
public interface IPassable<T> {

	bool IsPassable( T value );

}
