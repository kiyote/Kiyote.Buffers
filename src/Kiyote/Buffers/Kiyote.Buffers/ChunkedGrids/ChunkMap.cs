using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// The shared topology (chunks, presence and hull) and copy algorithms
/// behind <see cref="ChunkedGrid{T}"/> and <see cref="DoubleBufferedChunkedGrid{T}"/>.
/// </summary>
/// <remarks>
/// Value-touching operations take a buffer index; presence is shared by all
/// buffers. <see cref="Version"/> is incremented on every topology change made
/// through chunk allocation/removal or the attach/detach/compact operations so
/// enumerators can detect changes made while iterating.
/// </remarks>
internal sealed class ChunkMap<T> where T : unmanaged {

	public const int MaximumChunkSize = 1 << 12;

	private readonly Dictionary<long, GridChunkStorage<T>> _chunks;

	private GridChunkStorage<T>? _cache;
	private bool _hullDirty;
	private int _minChunkColumn;
	private int _minChunkRow;
	private int _maxChunkColumn;
	private int _maxChunkRow;

	public ChunkMap(
		int chunkSize,
		int bufferCount
	) {
		int lanes = Math.Max( 1, Vector256<byte>.Count / Unsafe.SizeOf<T>() );
		if( chunkSize <= 0
			|| chunkSize > MaximumChunkSize
			|| !BitOperations.IsPow2( chunkSize )
			|| chunkSize % lanes != 0
		) {
			throw new ArgumentOutOfRangeException(
				nameof( chunkSize ),
				chunkSize,
				$"Chunk size must be a power of two between {lanes} and {MaximumChunkSize} and a multiple of {lanes}."
			);
		}

		ChunkSize = chunkSize;
		ChunkShift = BitOperations.Log2( (uint)chunkSize );
		ChunkMask = chunkSize - 1;
		Area = chunkSize * chunkSize;
		WordsPerRow = Math.Max( 1, chunkSize >> 6 );
		PresenceWords = chunkSize * WordsPerRow;
		FullWordMask = chunkSize >= 64 ? ulong.MaxValue : ( 1UL << chunkSize ) - 1;
		BufferCount = bufferCount;
		_chunks = [];
	}

	public int ChunkSize { get; }

	public int ChunkShift { get; }

	public int ChunkMask { get; }

	public int Area { get; }

	public int WordsPerRow { get; }

	public int PresenceWords { get; }

	public ulong FullWordMask { get; }

	public int BufferCount { get; }

	public int Version { get; private set; }

	public int Count => _chunks.Count;

	public Dictionary<long, GridChunkStorage<T>>.ValueCollection Chunks => _chunks.Values;

	public int Column {
		get {
			EnsureHull();
			return Count == 0 ? 0 : _minChunkColumn << ChunkShift;
		}
	}

	public int Row {
		get {
			EnsureHull();
			return Count == 0 ? 0 : _minChunkRow << ChunkShift;
		}
	}

	public int Width {
		get {
			EnsureHull();
			return Count == 0 ? 0 : ( _maxChunkColumn - _minChunkColumn + 1 ) << ChunkShift;
		}
	}

	public int Height {
		get {
			EnsureHull();
			return Count == 0 ? 0 : ( _maxChunkRow - _minChunkRow + 1 ) << ChunkShift;
		}
	}

	public GridChunk<T> CreateView(
		GridChunkStorage<T> storage,
		int buffer
	) {
		return new GridChunk<T>( storage, buffer, ChunkSize, ChunkShift, WordsPerRow, FullWordMask );
	}

	public GridChunkStorage<T>? FindStorage(
		int chunkColumn,
		int chunkRow
	) {
		GridChunkStorage<T>? cache = _cache;
		if( cache is not null
			&& cache.ChunkColumn == chunkColumn
			&& cache.ChunkRow == chunkRow
		) {
			return cache;
		}
		if( _chunks.TryGetValue( Key( chunkColumn, chunkRow ), out GridChunkStorage<T>? chunk ) ) {
			_cache = chunk;
			return chunk;
		}
		return null;
	}

	public GridChunkStorage<T> GetOrCreateStorage(
		int chunkColumn,
		int chunkRow
	) {
		GridChunkStorage<T>? chunk = FindStorage( chunkColumn, chunkRow );
		if( chunk is not null ) {
			return chunk;
		}

		chunk = new GridChunkStorage<T>( chunkColumn, chunkRow, Area, PresenceWords, BufferCount );
		bool wasEmpty = _chunks.Count == 0;
		_chunks.Add( Key( chunkColumn, chunkRow ), chunk );
		_cache = chunk;
		Version++;

		if( wasEmpty ) {
			_minChunkColumn = _maxChunkColumn = chunkColumn;
			_minChunkRow = _maxChunkRow = chunkRow;
			_hullDirty = false;
		} else if( !_hullDirty ) {
			_minChunkColumn = Math.Min( _minChunkColumn, chunkColumn );
			_maxChunkColumn = Math.Max( _maxChunkColumn, chunkColumn );
			_minChunkRow = Math.Min( _minChunkRow, chunkRow );
			_maxChunkRow = Math.Max( _maxChunkRow, chunkRow );
		}
		return chunk;
	}

	public bool Remove(
		int chunkColumn,
		int chunkRow
	) {
		if( !_chunks.Remove( Key( chunkColumn, chunkRow ) ) ) {
			return false;
		}
		_cache = null;
		_hullDirty = true;
		Version++;
		return true;
	}

	public int Compact() {
		int removed = 0;
		foreach( KeyValuePair<long, GridChunkStorage<T>> pair in _chunks ) {
			if( !pair.Value.Presence.AsSpan().ContainsAnyExcept( 0UL ) ) {
				_ = _chunks.Remove( pair.Key );
				removed++;
			}
		}
		if( removed != 0 ) {
			_cache = null;
			_hullDirty = true;
			Version++;
		}
		return removed;
	}

	public bool IsBitSet(
		GridChunkStorage<T> chunk,
		int localColumn,
		int localRow
	) {
		ulong word = chunk.Presence[ ( localRow * WordsPerRow ) + ( localColumn >> 6 ) ];
		return ( word & ( 1UL << ( localColumn & 63 ) ) ) != 0;
	}

	public bool IsPopulated(
		int column,
		int row
	) {
		GridChunkStorage<T>? chunk = FindStorage( column >> ChunkShift, row >> ChunkShift );
		return chunk is not null
			&& IsBitSet( chunk, column & ChunkMask, row & ChunkMask );
	}

	public bool TryGet(
		int column,
		int row,
		int buffer,
		out T value
	) {
		GridChunkStorage<T>? chunk = FindStorage( column >> ChunkShift, row >> ChunkShift );
		if( chunk is not null ) {
			int localColumn = column & ChunkMask;
			int localRow = row & ChunkMask;
			if( IsBitSet( chunk, localColumn, localRow ) ) {
				value = chunk.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ];
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
		GridChunkStorage<T>? chunk = FindStorage( column >> ChunkShift, row >> ChunkShift );
		if( chunk is null ) {
			return false;
		}
		int localColumn = column & ChunkMask;
		int localRow = row & ChunkMask;
		if( !IsBitSet( chunk, localColumn, localRow ) ) {
			return false;
		}
		chunk.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ] = value;
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
		GridChunkStorage<T> chunk = GetOrCreateStorage( column >> ChunkShift, row >> ChunkShift );
		int localColumn = column & ChunkMask;
		int localRow = row & ChunkMask;
		chunk.Buffers[ buffer ][ ( localRow << ChunkShift ) + localColumn ] = value;
		chunk.Presence[ ( localRow * WordsPerRow ) + ( localColumn >> 6 ) ] |= 1UL << ( localColumn & 63 );
	}

	public void ClearCell(
		int column,
		int row
	) {
		GridChunkStorage<T>? chunk = FindStorage( column >> ChunkShift, row >> ChunkShift );
		if( chunk is null ) {
			return;
		}
		int localColumn = column & ChunkMask;
		int localRow = row & ChunkMask;
		chunk.Presence[ ( localRow * WordsPerRow ) + ( localColumn >> 6 ) ] &= ~( 1UL << ( localColumn & 63 ) );
	}

	public void CopyBuffer(
		int from,
		int to
	) {
		foreach( GridChunkStorage<T> chunk in _chunks.Values ) {
			chunk.Buffers[ from ].AsSpan().CopyTo( chunk.Buffers[ to ] );
		}
	}

	public NeighbourChunks<T> GatherNeighbours(
		GridChunkStorage<T> center
	) {
		NeighbourChunks<T> chunks = default;
		for( int dy = -1; dy <= 1; dy++ ) {
			for( int dx = -1; dx <= 1; dx++ ) {
				chunks[ ( ( dy + 1 ) * 3 ) + dx + 1 ] = dx == 0 && dy == 0
					? center
					: FindStorage( center.ChunkColumn + dx, center.ChunkRow + dy );
			}
		}
		return chunks;
	}

	public bool HasOverlap<TSource>(
		ChunkMap<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		if( IsAlignedWith( source, column, row ) ) {
			int chunkColumnOffset = column >> ChunkShift;
			int chunkRowOffset = row >> ChunkShift;
			foreach( GridChunkStorage<TSource> sourceChunk in source.Chunks ) {
				GridChunkStorage<T>? target = FindStorage(
					sourceChunk.ChunkColumn + chunkColumnOffset,
					sourceChunk.ChunkRow + chunkRowOffset
				);
				if( target is null ) {
					continue;
				}
				for( int i = 0; i < PresenceWords; i++ ) {
					if( ( sourceChunk.Presence[ i ] & target.Presence[ i ] ) != 0 ) {
						return true;
					}
				}
			}
			return false;
		}

		OverlapRunOperation<TSource, T> operation = default;
		return !WalkRuns( source, column, row, create: false, ref operation );
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
				SourceBuffer = sourceBuffer,
				Buffer = buffer
			};
			_ = WalkRuns( source, column, row, create: true, ref copy );
			Version++;
			return;
		}

		int chunkColumnOffset = column >> ChunkShift;
		int chunkRowOffset = row >> ChunkShift;
		int segment = Math.Min( 64, ChunkSize );
		foreach( GridChunkStorage<T> sourceChunk in source.Chunks ) {
			GridChunkStorage<T> target = GetOrCreateStorage(
				sourceChunk.ChunkColumn + chunkColumnOffset,
				sourceChunk.ChunkRow + chunkRowOffset
			);
			T[] sourceValues = sourceChunk.Buffers[ sourceBuffer ];
			T[] targetValues = target.Buffers[ buffer ];
			for( int localRow = 0; localRow < ChunkSize; localRow++ ) {
				int rowStart = localRow << ChunkShift;
				int presenceStart = localRow * WordsPerRow;
				for( int w = 0; w < WordsPerRow; w++ ) {
					ulong bits = sourceChunk.Presence[ presenceStart + w ];
					if( bits == 0 ) {
						continue;
					}
					int start = rowStart + ( w << 6 );
					if( bits == FullWordMask ) {
						sourceValues.AsSpan( start, segment ).CopyTo( targetValues.AsSpan( start, segment ) );
					} else {
						ulong remaining = bits;
						while( remaining != 0 ) {
							int index = start + BitOperations.TrailingZeroCount( remaining );
							remaining &= remaining - 1;
							targetValues[ index ] = sourceValues[ index ];
						}
					}
					target.Presence[ presenceStart + w ] |= bits;
				}
			}
		}
		Version++;
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
			SourceBuffer = sourceBuffer,
			Buffer = buffer
		};
		_ = WalkRuns( source, column, row, create: true, ref operation );
		Version++;
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
			_ = WalkRuns( source, column, row, create: false, ref clear );
			Version++;
			return;
		}

		int chunkColumnOffset = column >> ChunkShift;
		int chunkRowOffset = row >> ChunkShift;
		foreach( GridChunkStorage<TSource> sourceChunk in source.Chunks ) {
			GridChunkStorage<T>? target = FindStorage(
				sourceChunk.ChunkColumn + chunkColumnOffset,
				sourceChunk.ChunkRow + chunkRowOffset
			);
			if( target is null ) {
				continue;
			}
			for( int i = 0; i < PresenceWords; i++ ) {
				target.Presence[ i ] &= ~sourceChunk.Presence[ i ];
			}
		}
		Version++;
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
			Extractor = extractor,
			SourceBuffer = sourceBuffer
		};
		_ = WalkRuns( source, column, row, create: false, ref operation );
		Version++;
	}

	/// <summary>
	/// Splits every populated source row into runs of at most 64 cells that
	/// land inside a single target chunk row, resolving the target chunk once
	/// per run instead of once per cell.
	/// </summary>
	/// <returns><see langword="false"/> if the operation stopped the walk.</returns>
	private bool WalkRuns<TSource, TOperation>(
		ChunkMap<TSource> source,
		int column,
		int row,
		bool create,
		ref TOperation operation
	) where TSource : unmanaged
	  where TOperation : struct, IChunkRunOperation<TSource, T> {
		int sourceShift = source.ChunkShift;
		int sourceSize = source.ChunkSize;
		int sourceWords = source.WordsPerRow;
		foreach( GridChunkStorage<TSource> sourceChunk in source.Chunks ) {
			int sourceBaseColumn = sourceChunk.ChunkColumn << sourceShift;
			int sourceBaseRow = sourceChunk.ChunkRow << sourceShift;
			ulong[] sourcePresence = sourceChunk.Presence;
			for( int localRow = 0; localRow < sourceSize; localRow++ ) {
				int sourcePresenceRow = localRow * sourceWords;
				if( !sourcePresence.AsSpan( sourcePresenceRow, sourceWords ).ContainsAnyExcept( 0UL ) ) {
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
					ulong bits = PresenceBits.Read( sourcePresence, sourcePresenceRow, sourceLocalColumn, count );
					if( bits != 0 ) {
						int targetChunkColumn = targetColumn >> ChunkShift;
						GridChunkStorage<T>? target = create
							? GetOrCreateStorage( targetChunkColumn, targetChunkRow )
							: FindStorage( targetChunkColumn, targetChunkRow );
						if( target is not null
							&& !operation.Process(
								sourceChunk,
								sourceRowStart + sourceLocalColumn,
								target,
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

	private bool IsAlignedWith<TSource>(
		ChunkMap<TSource> source,
		int column,
		int row
	) where TSource : unmanaged {
		return source.ChunkSize == ChunkSize
			&& ( column & ChunkMask ) == 0
			&& ( row & ChunkMask ) == 0;
	}

	private void EnsureHull() {
		if( !_hullDirty ) {
			return;
		}
		_minChunkColumn = int.MaxValue;
		_minChunkRow = int.MaxValue;
		_maxChunkColumn = int.MinValue;
		_maxChunkRow = int.MinValue;
		foreach( GridChunkStorage<T> chunk in _chunks.Values ) {
			_minChunkColumn = Math.Min( _minChunkColumn, chunk.ChunkColumn );
			_maxChunkColumn = Math.Max( _maxChunkColumn, chunk.ChunkColumn );
			_minChunkRow = Math.Min( _minChunkRow, chunk.ChunkRow );
			_maxChunkRow = Math.Max( _maxChunkRow, chunk.ChunkRow );
		}
		_hullDirty = false;
	}

	private static long Key(
		int chunkColumn,
		int chunkRow
	) {
		return ( (long)chunkColumn << 32 ) | (uint)chunkRow;
	}
}
