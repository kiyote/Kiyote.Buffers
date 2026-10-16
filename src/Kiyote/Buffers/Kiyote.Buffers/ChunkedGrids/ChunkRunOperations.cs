using System.Numerics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// An operation applied to a run of up to 64 source cells that all land in
/// the same row of one target chunk.
/// </summary>
internal interface IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	/// <param name="sourceValues">The values of the source chunk.</param>
	/// <param name="sourceIndex">Value index of the first cell of the run in <paramref name="sourceValues"/>.</param>
	/// <param name="targetBuffers">The value buffers of the target chunk.</param>
	/// <param name="targetPresence">The presence of the target chunk.</param>
	/// <param name="targetIndex">Value index of the first cell of the run in each target buffer.</param>
	/// <param name="targetPresenceRow">Index of the first presence word of the target row.</param>
	/// <param name="targetBit">Bit offset of the run within the target presence row.</param>
	/// <param name="bits">Source presence of the run; bit <c>i</c> is cell <c>i</c>.</param>
	/// <param name="count">Number of cells in the run (1 to 64).</param>
	/// <param name="sourceColumn">Grid column of the first source cell.</param>
	/// <param name="sourceRow">Grid row of the source cells.</param>
	/// <returns><see langword="false"/> to stop walking.</returns>
	bool Process(
		TSource[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	);
}

internal struct CopyRunOperation<T> : IChunkRunOperation<T, T> where T : unmanaged {

	public int Buffer;

	public readonly bool Process(
		T[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		T[] targetValues = targetBuffers[ Buffer ];
		if( bits == PresenceBits.Mask( count ) ) {
			sourceValues.AsSpan( sourceIndex, count ).CopyTo( targetValues.AsSpan( targetIndex, count ) );
		} else {
			ulong remaining = bits;
			while( remaining != 0 ) {
				int i = BitOperations.TrailingZeroCount( remaining );
				remaining &= remaining - 1;
				targetValues[ targetIndex + i ] = sourceValues[ sourceIndex + i ];
			}
		}
		PresenceBits.Or( targetPresence.AsSpan( targetPresenceRow ), targetBit, bits );
		return true;
	}
}

internal struct ExtractRunOperation<TSource, T, TExtractor> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged
	where TExtractor : struct, ICellExtractor<TSource, T> {

	public TExtractor Extractor;
	public int Buffer;

	public bool Process(
		TSource[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		T[] targetValues = targetBuffers[ Buffer ];
		ulong accepted = 0;
		ulong remaining = bits;
		while( remaining != 0 ) {
			int i = BitOperations.TrailingZeroCount( remaining );
			remaining &= remaining - 1;
			if( Extractor.TryExtract( sourceColumn + i, sourceRow, in sourceValues[ sourceIndex + i ], out T value ) ) {
				targetValues[ targetIndex + i ] = value;
				accepted |= 1UL << i;
			}
		}
		if( accepted != 0 ) {
			PresenceBits.Or( targetPresence.AsSpan( targetPresenceRow ), targetBit, accepted );
		}
		return true;
	}
}

internal struct ClearRunOperation<TSource, T> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	public readonly bool Process(
		TSource[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		PresenceBits.AndNot( targetPresence.AsSpan( targetPresenceRow ), targetBit, bits );
		return true;
	}
}

internal struct ExtractClearRunOperation<TSource, T, TExtractor> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged
	where TExtractor : struct, ICellExtractor<TSource, T> {

	public TExtractor Extractor;

	public bool Process(
		TSource[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		Span<ulong> presenceRow = targetPresence.AsSpan( targetPresenceRow );
		ulong accepted = 0;
		ulong remaining = bits & PresenceBits.Read( presenceRow, targetBit, count );
		while( remaining != 0 ) {
			int i = BitOperations.TrailingZeroCount( remaining );
			remaining &= remaining - 1;
			if( Extractor.TryExtract( sourceColumn + i, sourceRow, in sourceValues[ sourceIndex + i ], out _ ) ) {
				accepted |= 1UL << i;
			}
		}
		if( accepted != 0 ) {
			PresenceBits.AndNot( presenceRow, targetBit, accepted );
		}
		return true;
	}
}

internal struct OverlapRunOperation<TSource, T> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	public readonly bool Process(
		TSource[] sourceValues,
		int sourceIndex,
		T[][] targetBuffers,
		ulong[] targetPresence,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		return ( PresenceBits.Read( targetPresence.AsSpan( targetPresenceRow ), targetBit, count ) & bits ) == 0;
	}
}
