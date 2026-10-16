using System.Numerics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A private <see cref="ChunkedGridLayout"/> with one or more value buffers,
/// plus the copy algorithms behind <see cref="ChunkedGrid{T}"/>,
/// <see cref="DoubleBufferedChunkedGrid{T}"/> and <see cref="TripleBufferedChunkedGrid{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Value-touching operations take a buffer index; presence is shared by all
/// buffers through the layout.
/// </para>
/// <para>
/// Every buffer is synchronised eagerly, on the owning thread, by each
/// operation that allocates or removes chunks. Read paths therefore never
/// synchronise, which keeps them free of writes to shared state so a
/// <see cref="TripleBufferedLease{T}"/> can read from another thread.
/// </para>
/// <para>
/// Cell access by coordinates goes through a per-chunk <see cref="ChunkSlot"/>
/// holding the chunk's presence and every buffer's values, behind a
/// one-entry cache, so a cache hit costs one reference check.
/// </para>
/// </remarks>
internal sealed class ChunkMap<T> where T : unmanaged {

	private readonly ChunkedGridLayout _layout;
	private readonly ChunkValues<T>[] _values;
	private ChunkSlot?[] _slots;
	// A single immutable reference so a racing reader sees either the old or
	// the new slot, never a torn mix.
	private ChunkSlot? _slotCache;

	public ChunkMap(
		int chunkSize,
		int bufferCount
	) {
		ChunkValues<T>.ValidateChunkSize( chunkSize, nameof( chunkSize ) );
		_layout = new ChunkedGridLayout( chunkSize );
		_values = new ChunkValues<T>[ bufferCount ];
		for( int i = 0; i < bufferCount; i++ ) {
			_values[ i ] = new ChunkValues<T>( _layout );
		}
		_slots = [];
		ChunkSize = chunkSize;
		ChunkShift = _layout.ChunkShift;
		ChunkMask = _layout.ChunkMask;
		WordsPerRow = _layout.PresenceWordsPerRow;
		PresenceWords = _layout.PresenceWordsPerChunk;
		FullWordMask = _layout.FullWordMask;
	}

	public ChunkedGridLayout Layout => _layout;

	public int ChunkSize { get; }

	public int ChunkShift { get; }

	public int ChunkMask { get; }

	public int WordsPerRow { get; }

	public int PresenceWords { get; }

	public ulong FullWordMask { get; }

	public int Count => _layout.ChunkCount;

	public int Column => _layout.Column;

	public int Row => _layout.Row;

	public int Width => _layout.Width;

	public int Height => _layout.Height;

	public ChunkValues<T> GetBuffer(
		int buffer
	) {
		return _values[ buffer ];
	}

	public GridChunk<T> CreateView(
		int chunkIndex,
		int buffer
	) {
		return new GridChunk<T>( _layout, chunkIndex, _values[ buffer ].Get( chunkIndex ) );
	}

	public GridChunkNeighbourhood<T> CreateNeighbourhood(
		int chunkIndex,
		int buffer
	) {
		return new GridChunkNeighbourhood<T>( _layout, _values[ buffer ], chunkIndex );
	}

	public int FindChunk(
		int chunkColumn,
		int chunkRow
	) {
		ChunkSlot? slot = FindSlot( chunkColumn, chunkRow );
		return slot is null ? ChunkedGridLayout.NoChunk : slot.Index;
	}

	public int GetOrCreateChunk(
		int chunkColumn,
		int chunkRow
	) {
		return GetOrCreateSlot( chunkColumn, chunkRow ).Index;
	}

	public bool Remove(
		int chunkColumn,
		int chunkRow
	) {
		int index = _layout.FindChunk( chunkColumn, chunkRow );
		if( index == ChunkedGridLayout.NoChunk ) {
			return false;
		}
		int previous = _layout.Version;
		_ = _layout.RemoveChunk( chunkColumn, chunkRow );
		foreach( ChunkValues<T> values in _values ) {
			values.OnRemoved( index, previous );
		}
		_slots[ index ] = null;
		_slotCache = null;
		return true;
	}

	public int Compact() {
		int removed = _layout.Compact();
		if( removed != 0 ) {
			foreach( ChunkValues<T> values in _values ) {
				values.Synchronize();
			}
			for( int i = 0; i < _slots.Length; i++ ) {
				if( _slots[ i ] is not null && !_layout.IsAllocated( i ) ) {
					_slots[ i ] = null;
				}
			}
			_slotCache = null;
		}
		return removed;
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		ChunkSlot? slot = FindSlot( column >> ChunkShift, row >> ChunkShift );
		return slot is not null
			&& IsBitSet( slot.Presence, column & ChunkMask, row & ChunkMask );
	}

	public bool TryGet(
		int column,
		int row,
		int buffer,
		out T value
	) {
		ChunkSlot? slot = FindSlot( column >> ChunkShift, row >> ChunkShift );
		if( slot is not null ) {
			int localColumn = column & ChunkMask;
			int localRow = row & ChunkMask;
			if( IsBitSet( slot.Presence, localColumn, localRow ) ) {
				value = slot.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ];
				return true;
			}
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Writes a value to an already populated cell without changing topology.
	/// </summary>
	public bool TrySetPopulated(
		int column,
		int row,
		int buffer,
		T value
	) {
		ChunkSlot? slot = FindSlot( column >> ChunkShift, row >> ChunkShift );
		if( slot is null ) {
			return false;
		}
		int localColumn = column & ChunkMask;
		int localRow = row & ChunkMask;
		if( !IsBitSet( slot.Presence, localColumn, localRow ) ) {
			return false;
		}
		slot.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ] = value;
		return true;
	}

	/// <summary>
	/// Writes a value and marks the cell populated, allocating the chunk if required.
	/// </summary>
	public void SetCell(
		int column,
		int row,
		int buffer,
		T value
	) {
		ChunkSlot slot = GetOrCreateSlot( column >> ChunkShift, row >> ChunkShift );
		int localColumn = column & ChunkMask;
		int localRow = row & ChunkMask;
		slot.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ] = value;
		slot.Presence[ ( localRow * WordsPerRow ) + ( localColumn >> 6 ) ] |= 1UL << ( localColumn & 63 );
	}

	public void ClearCell(
		int column,
		int row
	) {
		_ = _layout.ClearPopulated( column, row );
	}

	public void CopyBuffer(
		int from,
		int to
	) {
		_values[ from ].CopyTo( _values[ to ] );
	}

	public bool HasOverlap<TSource>(
		ChunkMap<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		if( IsAlignedWith( source, column, row ) ) {
			ChunkedGridLayout sourceLayout = source.Layout;
			int chunkColumnOffset = column >> ChunkShift;
			int chunkRowOffset = row >> ChunkShift;
			foreach( int sourceIndex in sourceLayout.EnumerateChunks() ) {
				int target = _layout.FindChunk(
					sourceLayout.GetChunkColumn( sourceIndex ) + chunkColumnOffset,
					sourceLayout.GetChunkRow( sourceIndex ) + chunkRowOffset
				);
				if( target == ChunkedGridLayout.NoChunk ) {
					continue;
				}
				ulong[] sourcePresence = sourceLayout.GetPresenceArray( sourceIndex );
				ulong[] targetPresence = _layout.GetPresenceArray( target );
				for( int i = 0; i < PresenceWords; i++ ) {
					if( ( sourcePresence[ i ] & targetPresence[ i ] ) != 0 ) {
						return true;
					}
				}
			}
			return false;
		}

		OverlapRunOperation<TSource, T> operation = default;
		return !WalkRuns( source, 0, column, row, create: false, ref operation );
	}

	/// <summary>
	/// Copies populated cells of <paramref name="source"/> buffer
	/// <paramref name="sourceBuffer"/> into <paramref name="buffer"/>,
	/// populating the target cells.
	/// </summary>
	public void Attach(
		ChunkMap<T> source,
		int sourceBuffer,
		int column,
		int row,
		int buffer
	) {
		if( !IsAlignedWith( source, column, row ) ) {
			CopyRunOperation<T> copy = new() {
				Buffer = buffer
			};
			_ = WalkRuns( source, sourceBuffer, column, row, create: true, ref copy );
			return;
		}

		ChunkedGridLayout sourceLayout = source.Layout;
		ChunkValues<T> sourceBufferValues = source.GetBuffer( sourceBuffer );
		int chunkColumnOffset = column >> ChunkShift;
		int chunkRowOffset = row >> ChunkShift;
		foreach( int sourceIndex in sourceLayout.EnumerateChunks() ) {
			ChunkSlot target = GetOrCreateSlot(
				sourceLayout.GetChunkColumn( sourceIndex ) + chunkColumnOffset,
				sourceLayout.GetChunkRow( sourceIndex ) + chunkRowOffset
			);
			CopyChunk(
				sourceLayout.GetPresenceArray( sourceIndex ),
				sourceBufferValues.Get( sourceIndex ),
				target.Presence,
				target.Buffers[ buffer ]
			);
		}
	}

	public void Attach<TSource, TExtractor>(
		ChunkMap<TSource> source,
		int sourceBuffer,
		int column,
		int row,
		int buffer,
		TExtractor extractor
	) where TSource : unmanaged
	  where TExtractor : struct, ICellExtractor<TSource, T> {
		ExtractRunOperation<TSource, T, TExtractor> operation = new() {
			Extractor = extractor,
			Buffer = buffer
		};
		_ = WalkRuns( source, sourceBuffer, column, row, create: true, ref operation );
	}

	/// <summary>
	/// Unpopulates every cell matching a populated cell of <paramref name="source"/>.
	/// </summary>
	public void Detach<TSource>(
		ChunkMap<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		if( !IsAlignedWith( source, column, row ) ) {
			ClearRunOperation<TSource, T> clear = default;
			_ = WalkRuns( source, 0, column, row, create: false, ref clear );
			return;
		}

		ChunkedGridLayout sourceLayout = source.Layout;
		int chunkColumnOffset = column >> ChunkShift;
		int chunkRowOffset = row >> ChunkShift;
		foreach( int sourceIndex in sourceLayout.EnumerateChunks() ) {
			int target = _layout.FindChunk(
				sourceLayout.GetChunkColumn( sourceIndex ) + chunkColumnOffset,
				sourceLayout.GetChunkRow( sourceIndex ) + chunkRowOffset
			);
			if( target == ChunkedGridLayout.NoChunk ) {
				continue;
			}
			ClearPresence( sourceLayout.GetPresenceArray( sourceIndex ), _layout.GetPresenceArray( target ) );
		}
	}

	public void Detach<TSource, TExtractor>(
		ChunkMap<TSource> source,
		int sourceBuffer,
		int column,
		int row,
		TExtractor extractor
	) where TSource : unmanaged
	  where TExtractor : struct, ICellExtractor<TSource, T> {
		ExtractClearRunOperation<TSource, T, TExtractor> operation = new() {
			Extractor = extractor
		};
		_ = WalkRuns( source, sourceBuffer, column, row, create: false, ref operation );
	}

	/// <summary>
	/// Splits every populated source row into runs of at most 64 cells that
	/// land inside a single target chunk row, resolving the target chunk once
	/// per run instead of once per cell.
	/// </summary>
	/// <returns><see langword="false"/> if the operation stopped the walk.</returns>
	private bool WalkRuns<TSource, TOperation>(
		ChunkMap<TSource> source,
		int sourceBuffer,
		int column,
		int row,
		bool create,
		ref TOperation operation
	) where TSource : unmanaged
	  where TOperation : struct, IChunkRunOperation<TSource, T> {
		ChunkedGridLayout sourceLayout = source.Layout;
		ChunkValues<TSource> sourceBufferValues = source.GetBuffer( sourceBuffer );
		int sourceShift = source.ChunkShift;
		int sourceSize = source.ChunkSize;
		int sourceWords = source.WordsPerRow;
		foreach( int sourceIndex in sourceLayout.EnumerateChunks() ) {
			int sourceBaseColumn = sourceLayout.GetChunkColumn( sourceIndex ) << sourceShift;
			int sourceBaseRow = sourceLayout.GetChunkRow( sourceIndex ) << sourceShift;
			ulong[] sourcePresence = sourceLayout.GetPresenceArray( sourceIndex );
			TSource[] sourceValues = sourceBufferValues.Get( sourceIndex );
			for( int localRow = 0; localRow < sourceSize; localRow++ ) {
				ReadOnlySpan<ulong> sourceRowPresence = sourcePresence.AsSpan( localRow * sourceWords, sourceWords );
				if( !sourceRowPresence.ContainsAnyExcept( 0UL ) ) {
					continue;
				}

				int sourceRow = sourceBaseRow + localRow;
				int targetRow = sourceRow + row;
				int targetChunkRow = targetRow >> ChunkShift;
				int targetLocalRow = targetRow & ChunkMask;
				int targetPresenceRow = targetLocalRow * WordsPerRow;
				int targetRowStart = targetLocalRow << ChunkShift;
				int sourceRowStart = localRow << sourceShift;

				int sourceLocalColumn = 0;
				while( sourceLocalColumn < sourceSize ) {
					int targetColumn = sourceBaseColumn + sourceLocalColumn + column;
					int targetLocalColumn = targetColumn & ChunkMask;
					int count = Math.Min(
						Math.Min( 64, sourceSize - sourceLocalColumn ),
						ChunkSize - targetLocalColumn
					);
					ulong bits = PresenceBits.Read( sourceRowPresence, sourceLocalColumn, count );
					if( bits != 0 ) {
						int targetChunkColumn = targetColumn >> ChunkShift;
						ChunkSlot? target = create
							? GetOrCreateSlot( targetChunkColumn, targetChunkRow )
							: FindSlot( targetChunkColumn, targetChunkRow );
						if( target is not null
							&& !operation.Process(
								sourceValues,
								sourceRowStart + sourceLocalColumn,
								target.Buffers,
								target.Presence,
								targetRowStart + targetLocalColumn,
								targetPresenceRow,
								targetLocalColumn,
								bits,
								count,
								sourceBaseColumn + sourceLocalColumn,
								sourceRow
							)
						) {
							return false;
						}
					}
					sourceLocalColumn += count;
				}
			}
		}
		return true;
	}

	private ChunkSlot? FindSlot(
		int chunkColumn,
		int chunkRow
	) {
		ChunkSlot? cache = _slotCache;
		if( cache is not null
			&& cache.Column == chunkColumn
			&& cache.Row == chunkRow
		) {
			return cache;
		}
		ChunkedGridLayout.ChunkEntry? entry = _layout.LookupEntry( chunkColumn, chunkRow );
		if( entry is null ) {
			return null;
		}
		ChunkSlot slot = _slots[ entry.Index ]!;
		_slotCache = slot;
		return slot;
	}

	private ChunkSlot GetOrCreateSlot(
		int chunkColumn,
		int chunkRow
	) {
		return FindSlot( chunkColumn, chunkRow ) ?? AllocateSlot( chunkColumn, chunkRow );
	}

	private ChunkSlot AllocateSlot(
		int chunkColumn,
		int chunkRow
	) {
		int previous = _layout.Version;
		ChunkedGridLayout.ChunkEntry entry = _layout.GetOrCreateEntry( chunkColumn, chunkRow );
		int index = entry.Index;
		T[][] buffers = new T[ _values.Length ][];
		for( int i = 0; i < _values.Length; i++ ) {
			_values[ i ].OnAllocated( index, previous );
			buffers[ i ] = _values[ i ].Get( index );
		}
		if( _slots.Length <= index ) {
			Array.Resize( ref _slots, Math.Max( index + 1, Math.Max( 4, _slots.Length * 2 ) ) );
		}
		ChunkSlot slot = new( entry, buffers );
		_slots[ index ] = slot;
		_slotCache = slot;
		return slot;
	}

	private bool IsBitSet(
		ulong[] presence,
		int localColumn,
		int localRow
	) {
		ulong word = presence[ ( localRow * WordsPerRow ) + ( localColumn >> 6 ) ];
		return ( word & ( 1UL << ( localColumn & 63 ) ) ) != 0;
	}

	/// <summary>
	/// Copies the populated cells of one chunk into a chunk-aligned target.
	/// </summary>
	private void CopyChunk(
		ulong[] sourcePresence,
		T[] sourceValues,
		ulong[] targetPresence,
		T[] targetValues
	) {
		int size = ChunkSize;
		int shift = ChunkShift;
		int words = WordsPerRow;
		ulong full = FullWordMask;
		int segment = Math.Min( 64, size );
		for( int localRow = 0; localRow < size; localRow++ ) {
			int rowStart = localRow << shift;
			int presenceStart = localRow * words;
			for( int w = 0; w < words; w++ ) {
				ulong bits = sourcePresence[ presenceStart + w ];
				if( bits == 0 ) {
					continue;
				}
				int start = rowStart + ( w << 6 );
				if( bits == full ) {
					sourceValues.AsSpan( start, segment ).CopyTo( targetValues.AsSpan( start, segment ) );
				} else {
					ulong remaining = bits;
					while( remaining != 0 ) {
						int index = start + BitOperations.TrailingZeroCount( remaining );
						remaining &= remaining - 1;
						targetValues[ index ] = sourceValues[ index ];
					}
				}
				targetPresence[ presenceStart + w ] |= bits;
			}
		}
	}

	private static void ClearPresence(
		ulong[] sourcePresence,
		ulong[] targetPresence
	) {
		for( int i = 0; i < targetPresence.Length; i++ ) {
			targetPresence[ i ] &= ~sourcePresence[ i ];
		}
	}

	private bool IsAlignedWith<TSource>(
		ChunkMap<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		return source.ChunkSize == ChunkSize
			&& ( column & ChunkMask ) == 0
			&& ( row & ChunkMask ) == 0;
	}

	/// <summary>
	/// The coordinates, presence and every buffer's values of one chunk.
	/// Immutable, so it can be cached as a single reference.
	/// </summary>
	private sealed class ChunkSlot(
		ChunkedGridLayout.ChunkEntry entry,
		T[][] buffers
	) {
		public int Column { get; } = entry.Column;

		public int Row { get; } = entry.Row;

		public int Index { get; } = entry.Index;

		public ulong[] Presence { get; } = entry.Presence;

		public T[][] Buffers { get; } = buffers;
	}
}
