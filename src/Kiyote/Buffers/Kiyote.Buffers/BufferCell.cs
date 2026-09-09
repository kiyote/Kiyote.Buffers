namespace Kiyote.Buffers;

/// <summary>
/// A single cell of an <see cref="IBuffer{T}"/> along with its location.
/// </summary>
public readonly record struct BufferCell<T>(
	int Column,
	int Row,
	T Value
);
