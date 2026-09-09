namespace Kiyote.Buffers;

public interface IBufferAnalyzer {

	bool IsSealed<T>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		Func<T, bool> isPassable
	);

	/// <summary>
	/// Identical to <see cref="IsSealed{T}(IBuffer{T}, int, int, Func{T, bool})"/>
	/// but takes the passability test as a struct so the JIT can inline it,
	/// removing the per-cell delegate invocation.
	/// </summary>
	bool IsSealed<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable
	) where TPassable : struct, IPassable<T>;

	/// <summary>
	/// Finds every cell reachable from the starting point without crossing an
	/// impassable cell, provided that region never touches the buffer edge.
	/// </summary>
	/// <param name="area">
	/// The cells making up the sealed region, or an empty list when the region
	/// is not sealed.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the region is sealed, otherwise <see langword="false"/>.
	/// </returns>
	bool TryGetSealedArea<T>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		Func<T, bool> isPassable,
		out IReadOnlyList<BufferCell<T>> area
	);

	/// <summary>
	/// Identical to <see cref="TryGetSealedArea{T}(IBuffer{T}, int, int, Func{T, bool}, out IReadOnlyList{BufferCell{T}})"/>
	/// but takes the passability test as a struct so the JIT can inline it.
	/// </summary>
	bool TryGetSealedArea<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		out IReadOnlyList<BufferCell<T>> area
	) where TPassable : struct, IPassable<T>;

	/// <summary>
	/// Reports every cell of the sealed region to <paramref name="visitor"/>
	/// without materializing the region into a collection.
	/// </summary>
	/// <remarks>
	/// Prefer this over
	/// <see cref="TryGetSealedArea{T}(IBuffer{T}, int, int, Func{T, bool}, out IReadOnlyList{BufferCell{T}})"/>
	/// for large regions; it performs no allocation at all. When the region is
	/// not sealed the fill stops as soon as it reaches the buffer edge, so the
	/// visitor will have been given only part of the region.
	/// </remarks>
	/// <returns>
	/// <see langword="true"/> if the region is sealed, otherwise <see langword="false"/>.
	/// </returns>
	bool TryVisitSealedArea<T, TPassable, TVisitor>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		ref TVisitor visitor
	)
		where TPassable : struct, IPassable<T>
		where TVisitor : struct, ICellVisitor<T>;

}
