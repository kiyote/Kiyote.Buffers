namespace Kiyote.Buffers;

/// <summary>
/// Adapts a <see cref="Func{T, TResult}"/> to <see cref="IPassable{T}"/> so the
/// delegate-based API can share the generic implementation.
/// </summary>
internal readonly struct FuncPassable<T> : IPassable<T> {

	private readonly Func<T, bool> _isPassable;

	public FuncPassable(
		Func<T, bool> isPassable
	) {
		_isPassable = isPassable;
	}

	public bool IsPassable(
		T value
	) {
		return _isPassable( value );
	}
}
