using System.Numerics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// An operation applied to a run of up to 64 source cells that all land in
/// the same row of one target chunk.
/// </summary>
internal interface IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	/// <param name="source">The source chunk.</param>
	/// <param name="sourceIndex">Value index of the first cell of the run in <paramref name="source"/>.</param>
	/// <param name="target">The target chunk.</param>
	/// <param name="targetIndex">Value index of the first cell of the run in <paramref name="target"/>.</param>
	/// <param name="targetPresenceRow">Index of the first presence word of the target row.</param>
	/// <param name="targetBit">Bit offset of the run within the target presence row.</param>
	/// <param name="bits">Source presence of the run; bit <c>i</c> is cell <c>i</c>.</param>
	/// <param name="count">Number of cells in the run (1 to 64).</param>
	/// <param name="sourceColumn">Grid column of the first source cell.</param>
	/// <param name="sourceRow">Grid row of the source cells.</param>
	/// <returns><see langword="false"/> to stop walking.</returns>
	bool Process(
		GridChunkStorage<TSource> source,
		int sourceIndex,
		GridChunkStorage<T> target,
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

	public int SourceBuffer;
	public int Buffer;

	public readonly bool Process(
		GridChunkStorage<T> source,
		int sourceIndex,
		GridChunkStorage<T> target,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		T[] from = source.Buffers[ SourceBuffer ];
		T[] to = target.Buffers[ Buffer ];
		if( bits == PresenceBits.Mask( count ) ) {
			from.AsSpan( sourceIndex, count ).CopyTo( to.AsSpan( targetIndex, count ) );
		} else {
			ulong remaining = bits;
			while( remaining != 0 ) {
				int i = BitOperations.TrailingZeroCount( remaining );
				remaining &= remaining - 1;
				to[ targetIndex + i ] = from[ sourceIndex + i ];
			}
		}
		PresenceBits.Or( target.Presence, targetPresenceRow, targetBit, bits );
		return true;
	}
}

internal struct ExtractRunOperation<TSource, T, TExtractor> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged
	where TExtractor : struct, ICellExtractor<TSource, T> {

	public TExtractor Extractor;
	public int SourceBuffer;
	public int Buffer;

	public bool Process(
		GridChunkStorage<TSource> source,
		int sourceIndex,
		GridChunkStorage<T> target,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		TSource[] from = source.Buffers[ SourceBuffer ];
		T[] to = target.Buffers[ Buffer ];
		ulong accepted = 0;
		ulong remaining = bits;
		while( remaining != 0 ) {
			int i = BitOperations.TrailingZeroCount( remaining );
			remaining &= remaining - 1;
			if( Extractor.TryExtract( sourceColumn + i, sourceRow, in from[ sourceIndex + i ], out T value ) ) {
				to[ targetIndex + i ] = value;
				accepted |= 1UL << i;
			}
		}
		if( accepted != 0 ) {
			PresenceBits.Or( target.Presence, targetPresenceRow, targetBit, accepted );
		}
		return true;
	}
}

internal struct ClearRunOperation<TSource, T> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	public readonly bool Process(
		GridChunkStorage<TSource> source,
		int sourceIndex,
		GridChunkStorage<T> target,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		PresenceBits.AndNot( target.Presence, targetPresenceRow, targetBit, bits );
		return true;
	}
}

internal struct ExtractClearRunOperation<TSource, T, TExtractor> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged
	where TExtractor : struct, ICellExtractor<TSource, T> {

	public TExtractor Extractor;
	public int SourceBuffer;

	public bool Process(
		GridChunkStorage<TSource> source,
		int sourceIndex,
		GridChunkStorage<T> target,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		TSource[] from = source.Buffers[ SourceBuffer ];
		ulong accepted = 0;
		ulong remaining = bits & PresenceBits.Read( target.Presence, targetPresenceRow, targetBit, count );
		while( remaining != 0 ) {
			int i = BitOperations.TrailingZeroCount( remaining );
			remaining &= remaining - 1;
			if( Extractor.TryExtract( sourceColumn + i, sourceRow, in from[ sourceIndex + i ], out _ ) ) {
				accepted |= 1UL << i;
			}
		}
		if( accepted != 0 ) {
			PresenceBits.AndNot( target.Presence, targetPresenceRow, targetBit, accepted );
		}
		return true;
	}
}

internal struct OverlapRunOperation<TSource, T> : IChunkRunOperation<TSource, T>
	where TSource : unmanaged
	where T : unmanaged {

	public readonly bool Process(
		GridChunkStorage<TSource> source,
		int sourceIndex,
		GridChunkStorage<T> target,
		int targetIndex,
		int targetPresenceRow,
		int targetBit,
		ulong bits,
		int count,
		int sourceColumn,
		int sourceRow
	) {
		return ( PresenceBits.Read( target.Presence, targetPresenceRow, targetBit, count ) & bits ) == 0;
	}
}
