using System.Buffers;

namespace Kiyote.Buffers;

internal sealed class BufferAnalyzer : IBufferAnalyzer {
	bool IBufferAnalyzer.IsSealed<T>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		Func<T, bool> isPassable
	) {
		return IsSealed( buffer, startColumn, startRow, new FuncPassable<T>( isPassable ) );
	}

	bool IBufferAnalyzer.IsSealed<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable
	) {
		return IsSealed( buffer, startColumn, startRow, isPassable );
	}

	bool IBufferAnalyzer.TryGetSealedArea<T>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		Func<T, bool> isPassable,
		out IReadOnlyList<BufferCell<T>> area
	) {
		return TryGetSealedArea( buffer, startColumn, startRow, new FuncPassable<T>( isPassable ), out area );
	}

	bool IBufferAnalyzer.TryGetSealedArea<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		out IReadOnlyList<BufferCell<T>> area
	) {
		return TryGetSealedArea( buffer, startColumn, startRow, isPassable, out area );
	}

	bool IBufferAnalyzer.TryVisitSealedArea<T, TPassable, TVisitor>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		ref TVisitor visitor
	) {
		return Fill<T, TPassable, TVisitor>( buffer, startColumn, startRow, isPassable, ref visitor );
	}

	private static bool IsSealed<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable
	) where TPassable : struct, IPassable<T> {
		NullCellVisitor<T> visitor = default;
		return Fill<T, TPassable, NullCellVisitor<T>>( buffer, startColumn, startRow, isPassable, ref visitor );
	}

	private static bool TryGetSealedArea<T, TPassable>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		out IReadOnlyList<BufferCell<T>> area
	) where TPassable : struct, IPassable<T> {
		List<BufferCell<T>> cells = [];
		var visitor = new ListCellVisitor<T>( cells );
		bool sealedArea = Fill<T, TPassable, ListCellVisitor<T>>( buffer, startColumn, startRow, isPassable, ref visitor );

		if( !sealedArea ) {
			cells.Clear();
		}

		area = cells;
		return sealedArea;
	}

	/// <summary>
	/// Flood fills from the starting point, reporting every reached cell to
	/// <paramref name="visitor"/>, and returns whether the region is sealed.
	/// </summary>
	/// <remarks>
	/// When the region is not sealed the fill stops early, so the visitor will
	/// have seen only a partial region.
	/// </remarks>
	private static bool Fill<T, TPassable, TVisitor>(
		IBuffer<T> buffer,
		int startColumn,
		int startRow,
		TPassable isPassable,
		ref TVisitor visitor
	)
		where TPassable : struct, IPassable<T>
		where TVisitor : struct, ICellVisitor<T> {
		// Early exit if starting point is already an impassable wall
		if( !isPassable.IsPassable( buffer[ startColumn, startRow ] ) ) {
			return true;
		}

		int columns = buffer.Columns;
		int rows = buffer.Rows;
		int cellCount = columns * rows;

		// Pooled state so the steady-state allocation is zero, at any buffer size
		PooledBitSet visited = new PooledBitSet( cellCount );
		ChunkedIndexQueue queue = new ChunkedIndexQueue( cellCount );
		try {
			int startIndex = ( startRow * columns ) + startColumn;
			visited.Set( startIndex );
			queue.Enqueue( startIndex );

			// 4-way connectivity movements
			ReadOnlySpan<int> dc = [ 0, 0, -1, 1 ];
			ReadOnlySpan<int> dr = [ -1, 1, 0, 0 ];

			while( !queue.IsEmpty ) {
				int index = queue.Dequeue();
				int c = index % columns;
				int r = index / columns;

				visitor.Visit( c, r, buffer[ c, r ] );

				for( int i = 0; i < 4; i++ ) {
					int nc = c + dc[ i ];
					int nr = r + dr[ i ];

					// 1. Check map boundaries
					if( nc < 0 || nc >= columns || nr < 0 || nr >= rows ) {
						return false;
					}

					// 2. Process valid internal neighbors
					int neighbourIndex = ( nr * columns ) + nc;
					if( !visited.IsSet( neighbourIndex ) && isPassable.IsPassable( buffer[ nc, nr ] ) ) {
						visited.Set( neighbourIndex );
						queue.Enqueue( neighbourIndex );
					}
				}
			}

			return true;
		} finally {
			queue.Dispose();
			visited.Dispose();
		}
	}
}
