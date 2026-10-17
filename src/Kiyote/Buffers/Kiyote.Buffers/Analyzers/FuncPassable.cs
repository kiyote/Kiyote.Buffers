namespace Kiyote.Buffers.Analyzers;

/// <summary>
/// Adapts a <see cref="Func{T, TResult}"/> to <see cref="ICellEval{T}"/> so the
/// delegate-based API can share the generic implementation.
/// </summary>
internal readonly struct FuncPassable<T> : ICellEval<T> {

	private readonly Func<T, bool> _isPassable;

	public FuncPassable(
		Func<T, bool> isPassable
	) {
		_isPassable = isPassable;
	}

	public bool Evaluate(
		T value
	) {
		return _isPassable( value );
	}
}
