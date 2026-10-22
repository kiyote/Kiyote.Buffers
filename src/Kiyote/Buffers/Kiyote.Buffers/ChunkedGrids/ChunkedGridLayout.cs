using System.Numerics;
using System.Runtime.CompilerServices;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// The topology of a sparse chunked grid: which square, power-of-two sized
/// chunks are allocated and which of their cells are populated. Holds no
/// values; any number of <see cref="ChunkedGridLayer{T}"/> instances of any
/// element type can share one layout.
/// </summary>
/// <remarks>
/// <para>
/// Every allocated chunk has a dense integer index in the range
/// <c>[0, <see cref="ChunkCapacity"/>)</c>. An index is stable while its chunk
/// stays allocated, so callers can keep per-chunk side arrays indexed by it.
/// Removing a chunk frees its index, and a later allocation may reuse it; a
/// reused index always starts with no populated cells. <see cref="Version"/>
/// changes on every allocation and removal, and never on cell-level changes.
/// </para>
/// <para>
/// Presence stores one bit per cell, <see cref="PresenceWordsPerRow"/> words
/// per row, with bit <c>c &amp; 63</c> of word <c>c &gt;&gt; 6</c> describing
/// local column <c>c</c>. Presence spans are writable; a freed chunk's spans
/// refer to orphaned memory rather than to whichever chunk reuses its index.
/// </para>
/// <para>
/// The layout is not thread-safe. Concurrent reads are safe only while no
/// chunk is allocated or removed.
/// </para>
/// </remarks>
public sealed class ChunkedGridLayout {

	/// <summary>
	/// The chunk size used when none is specified.
	/// </summary>
	public const int DefaultChunkSize = 64;

	/// <summary>
	/// The largest supported chunk size.
	/// </summary>
	public const int MaximumChunkSize = 1 << 12;

	internal const int NeighbourCount = 9;
	internal const int CentreNeighbour = 4;
	internal const int NoChunk = -1;

	private readonly Dictionary<long, ChunkEntry> _lookup;
	private readonly Stack<int> _free;
	private ChunkEntry?[] _entries;
	private ulong[]?[] _presence;
	private int[] _generations;
	private int[] _neighbours;
	// A single immutable reference so a racing reader sees either the old or
	// the new entry, never a torn mix of coordinates and index.
	private ChunkEntry? _cache;
	private bool _hullDirty;
	private int _minChunkColumn;
	private int _minChunkRow;
	private int _maxChunkColumn;
	private int _maxChunkRow;

	/// <summary>
	/// Creates an empty layout.
	/// </summary>
	/// <param name="chunkSize">The width and height of a chunk in cells.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkSize"/> is not a power of two between 1 and <see cref="MaximumChunkSize"/>.
	/// </exception>
	public ChunkedGridLayout(
		int chunkSize = DefaultChunkSize
	) {
		if( chunkSize <= 0
			|| chunkSize > MaximumChunkSize
			|| !BitOperations.IsPow2( chunkSize )
		) {
			throw new ArgumentOutOfRangeException(
				nameof( chunkSize ),
				chunkSize,
				$"Chunk size must be a power of two between 1 and {MaximumChunkSize}."
			);
		}

		ChunkSize = chunkSize;
		ChunkShift = BitOperations.Log2( (uint)chunkSize );
		ChunkMask = chunkSize - 1;
		PresenceWordsPerRow = Math.Max( 1, chunkSize >> 6 );
		PresenceWordsPerChunk = chunkSize * PresenceWordsPerRow;
		FullWordMask = PresenceBits.Mask( chunkSize );
		_lookup = [];
		_free = new Stack<int>();
		_entries = [];
		_presence = [];
		_generations = [];
		_neighbours = [];
	}

	/// <summary>
	/// The width and height of a chunk in cells; a power of two.
	/// </summary>
	public int ChunkSize { get; }

	/// <summary>
	/// <c>Log2( <see cref="ChunkSize"/> )</c>; a grid coordinate shifted right by
	/// this is a chunk coordinate.
	/// </summary>
	public int ChunkShift { get; }

	/// <summary>
	/// <c><see cref="ChunkSize"/> - 1</c>; a grid coordinate masked with this is
	/// a local coordinate.
	/// </summary>
	public int ChunkMask { get; }

	/// <summary>
	/// The number of presence words per chunk row: <c>Math.Max( 1, <see cref="ChunkSize"/> &gt;&gt; 6 )</c>.
	/// </summary>
	public int PresenceWordsPerRow { get; }

	/// <summary>
	/// The number of presence words per chunk: <c><see cref="ChunkSize"/> * <see cref="PresenceWordsPerRow"/></c>.
	/// </summary>
	public int PresenceWordsPerChunk { get; }

	/// <summary>
	/// The number of allocated chunks.
	/// </summary>
	public int ChunkCount => _lookup.Count;

	/// <summary>
	/// One more than the highest chunk index ever issued. Never shrinks; size
	/// per-chunk side arrays with this.
	/// </summary>
	public int ChunkCapacity { get; private set; }

	/// <summary>
	/// Incremented on every chunk allocation and removal.
	/// </summary>
	public int Version { get; private set; }

	/// <summary>
	/// <see langword="true"/> when no chunk is allocated.
	/// </summary>
	public bool IsEmpty => _lookup.Count == 0;

	/// <summary>
	/// The left-most column of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	public int Column {
		get {
			EnsureHull();
			return IsEmpty ? 0 : _minChunkColumn << ChunkShift;
		}
	}

	/// <summary>
	/// The top-most row of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	public int Row {
		get {
			EnsureHull();
			return IsEmpty ? 0 : _minChunkRow << ChunkShift;
		}
	}

	/// <summary>
	/// The width of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	/// <exception cref="OverflowException">The bounding box is wider than <see cref="int.MaxValue"/> cells.</exception>
	public int Width {
		get {
			EnsureHull();
			return IsEmpty ? 0 : checked( (int)( ( (long)_maxChunkColumn - _minChunkColumn + 1 ) * ChunkSize ) );
		}
	}

	/// <summary>
	/// The height of the chunk-aligned bounding box of allocated chunks.
	/// </summary>
	/// <exception cref="OverflowException">The bounding box is taller than <see cref="int.MaxValue"/> cells.</exception>
	public int Height {
		get {
			EnsureHull();
			return IsEmpty ? 0 : checked( (int)( ( (long)_maxChunkRow - _minChunkRow + 1 ) * ChunkSize ) );
		}
	}

	/// <summary>
	/// A presence word with every cell of a row populated; only the low
	/// <see cref="ChunkSize"/> bits are set when the chunk is narrower than 64.
	/// </summary>
	internal ulong FullWordMask { get; }

	/// <summary>
	/// Gets the index of a chunk, allocating it if required.
	/// </summary>
	public int GetOrCreateChunk(
		int chunkColumn,
		int chunkRow
	) {
		return GetOrCreateEntry( chunkColumn, chunkRow ).Index;
	}

	/// <summary>
	/// Gets the index of an allocated chunk.
	/// </summary>
	/// <param name="chunkIndex">The chunk index, or <c>-1</c> if the chunk is not allocated.</param>
	public bool TryGetChunk(
		int chunkColumn,
		int chunkRow,
		out int chunkIndex
	) {
		chunkIndex = FindChunk( chunkColumn, chunkRow );
		return chunkIndex != NoChunk;
	}

	/// <summary>
	/// Determines whether <paramref name="chunkIndex"/> refers to an allocated
	/// chunk. Never throws.
	/// </summary>
	public bool IsAllocated(
		int chunkIndex
	) {
		return (uint)chunkIndex < (uint)ChunkCapacity
			&& _entries[ chunkIndex ] is not null;
	}

	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public int GetChunkColumn(
		int chunkIndex
	) {
		return GetEntry( chunkIndex ).Column;
	}

	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public int GetChunkRow(
		int chunkIndex
	) {
		return GetEntry( chunkIndex ).Row;
	}

	/// <summary>
	/// Removes a chunk and frees its index for reuse.
	/// </summary>
	/// <returns><see langword="false"/> if the chunk was not allocated.</returns>
	public bool RemoveChunk(
		int chunkColumn,
		int chunkRow
	) {
		if( !_lookup.Remove( Key( chunkColumn, chunkRow ), out ChunkEntry? entry ) ) {
			return false;
		}
		Free( entry );
		return true;
	}

	/// <summary>
	/// Removes every chunk that has no populated cells.
	/// </summary>
	/// <returns>The number of chunks removed.</returns>
	public int Compact() {
		int removed = 0;
		for( int i = 0; i < ChunkCapacity; i++ ) {
			ChunkEntry? entry = _entries[ i ];
			if( entry is not null
				&& !_presence[ i ].AsSpan().ContainsAnyExcept( 0UL )
			) {
				_ = _lookup.Remove( Key( entry.Column, entry.Row ) );
				Free( entry );
				removed++;
			}
		}
		return removed;
	}

	/// <summary>
	/// Gets the indices of a chunk and its eight neighbours. Entry
	/// <c>( dy + 1 ) * 3 + ( dx + 1 )</c> is the chunk offset by
	/// (<c>dx</c>, <c>dy</c>), so entry 4 is <paramref name="chunkIndex"/>
	/// itself; entries are <c>-1</c> where no chunk is allocated.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public ReadOnlySpan<int> GetNeighbours(
		int chunkIndex
	) {
		_ = GetEntry( chunkIndex );
		return new ReadOnlySpan<int>( _neighbours, chunkIndex * NeighbourCount, NeighbourCount );
	}

	/// <summary>
	/// Determines whether a cell's chunk is allocated and the cell is populated.
	/// </summary>
	public bool IsPopulated(
		int column,
		int row
	) {
		ChunkEntry? entry = FindEntry( column >> ChunkShift, row >> ChunkShift );
		return entry is not null
			&& IsBitSet( entry.Presence, column & ChunkMask, row & ChunkMask );
	}

	/// <summary>
	/// Finds the chunk index and local value index of a populated cell.
	/// </summary>
	/// <param name="chunkIndex">The index of the cell's chunk, or <c>-1</c>.</param>
	/// <param name="localIndex">
	/// <c>( localRow &lt;&lt; <see cref="ChunkShift"/> ) + localColumn</c>, the
	/// index of the cell's value within its chunk, or <c>-1</c>.
	/// </param>
	/// <returns>
	/// <see langword="true"/> when the cell's chunk is allocated and the cell is populated.
	/// </returns>
	public bool TryLocate(
		int column,
		int row,
		out int chunkIndex,
		out int localIndex
	) {
		ChunkEntry? entry = FindEntry( column >> ChunkShift, row >> ChunkShift );
		if( entry is not null ) {
			int localColumn = column & ChunkMask;
			int localRow = row & ChunkMask;
			if( IsBitSet( entry.Presence, localColumn, localRow ) ) {
				chunkIndex = entry.Index;
				localIndex = ( localRow << ChunkShift ) + localColumn;
				return true;
			}
		}
		chunkIndex = NoChunk;
		localIndex = NoChunk;
		return false;
	}

	/// <summary>
	/// Marks a cell populated, allocating its chunk if required.
	/// </summary>
	/// <returns><see langword="true"/> if the cell was previously unpopulated.</returns>
	public bool SetPopulated(
		int column,
		int row
	) {
		ChunkEntry entry = GetOrCreateEntry( column >> ChunkShift, row >> ChunkShift );
		int localColumn = column & ChunkMask;
		ref ulong word = ref entry.Presence[ ( ( row & ChunkMask ) * PresenceWordsPerRow ) + ( localColumn >> 6 ) ];
		ulong bit = 1UL << ( localColumn & 63 );
		bool changed = ( word & bit ) == 0;
		word |= bit;
		return changed;
	}

	/// <summary>
	/// Marks a cell unpopulated. Never allocates, and leaves the chunk
	/// allocated even if it becomes empty; see <see cref="Compact"/>.
	/// </summary>
	/// <returns><see langword="true"/> if the cell was previously populated.</returns>
	public bool ClearPopulated(
		int column,
		int row
	) {
		ChunkEntry? entry = FindEntry( column >> ChunkShift, row >> ChunkShift );
		if( entry is null ) {
			return false;
		}
		int localColumn = column & ChunkMask;
		ref ulong word = ref entry.Presence[ ( ( row & ChunkMask ) * PresenceWordsPerRow ) + ( localColumn >> 6 ) ];
		ulong bit = 1UL << ( localColumn & 63 );
		bool changed = ( word & bit ) != 0;
		word &= ~bit;
		return changed;
	}

	/// <summary>
	/// Marks a horizontal run of <paramref name="length"/> cells starting at
	/// (<paramref name="column"/>, <paramref name="row"/>) populated,
	/// allocating chunks as required. The run may cross any number of chunks.
	/// </summary>
	/// <returns>The number of cells that were previously unpopulated.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative or the run exceeds <see cref="int.MaxValue"/>.</exception>
	public int SetPopulatedRun(
		int column,
		int row,
		int length
	) {
		ArgumentOutOfRangeException.ThrowIfNegative( length );
		if( length > 0 && (long)column + length - 1 > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( length ) );
		}
		int changed = 0;
		int chunkRow = row >> ChunkShift;
		int rowStart = ( row & ChunkMask ) * PresenceWordsPerRow;
		while( length > 0 ) {
			int localColumn = column & ChunkMask;
			int count = Math.Min( length, ChunkSize - localColumn );
			changed += SetBits( GetOrCreateEntry( column >> ChunkShift, chunkRow ).Presence, rowStart, localColumn, count );
			length -= count;
			if( length > 0 ) {
				column += count;
			}
		}
		return changed;
	}

	/// <summary>
	/// Marks a horizontal run of <paramref name="length"/> cells starting at
	/// (<paramref name="column"/>, <paramref name="row"/>) unpopulated. Never
	/// allocates or removes chunks.
	/// </summary>
	/// <returns>The number of cells that were previously populated.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative or the run exceeds <see cref="int.MaxValue"/>.</exception>
	public int ClearPopulatedRun(
		int column,
		int row,
		int length
	) {
		ArgumentOutOfRangeException.ThrowIfNegative( length );
		if( length > 0 && (long)column + length - 1 > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( length ) );
		}
		int changed = 0;
		int chunkRow = row >> ChunkShift;
		int rowStart = ( row & ChunkMask ) * PresenceWordsPerRow;
		while( length > 0 ) {
			int localColumn = column & ChunkMask;
			int count = Math.Min( length, ChunkSize - localColumn );
			ChunkEntry? entry = FindEntry( column >> ChunkShift, chunkRow );
			if( entry is not null ) {
				changed += ClearBits( entry.Presence, rowStart, localColumn, count );
			}
			length -= count;
			if( length > 0 ) {
				column += count;
			}
		}
		return changed;
	}

	/// <summary>
	/// Gets the writable presence words of a chunk, <see cref="PresenceWordsPerRow"/>
	/// words per row.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public Span<ulong> GetPresence(
		int chunkIndex
	) {
		_ = GetEntry( chunkIndex );
		return _presence[ chunkIndex ]!;
	}

	/// <summary>
	/// Gets the writable presence words of one row of a chunk.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkIndex"/> is not allocated, or <paramref name="localRow"/>
	/// is outside the chunk.
	/// </exception>
	public Span<ulong> GetPresenceRow(
		int chunkIndex,
		int localRow
	) {
		_ = GetEntry( chunkIndex );
		ArgumentOutOfRangeException.ThrowIfNegative( localRow );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, ChunkSize );
		return new Span<ulong>( _presence[ chunkIndex ], localRow * PresenceWordsPerRow, PresenceWordsPerRow );
	}

	/// <summary>
	/// Counts the populated cells of a chunk. Computed on each call.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public int GetPopulatedCount(
		int chunkIndex
	) {
		_ = GetEntry( chunkIndex );
		return CountPopulated( _presence[ chunkIndex ]! );
	}

	/// <summary>
	/// Determines whether a chunk is empty, partially populated or full.
	/// Computed on each call.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkIndex"/> is not allocated.</exception>
	public ChunkFill GetFill(
		int chunkIndex
	) {
		_ = GetEntry( chunkIndex );
		ReadOnlySpan<ulong> presence = _presence[ chunkIndex ]!;
		if( !presence.ContainsAnyExcept( 0UL ) ) {
			return ChunkFill.Empty;
		}
		return presence.ContainsAnyExcept( FullWordMask )
			? ChunkFill.Partial
			: ChunkFill.Full;
	}

	/// <summary>
	/// Reads <paramref name="count"/> presence bits of one row, relative to
	/// chunk <paramref name="chunkIndex"/>, that may extend into its
	/// neighbouring chunks. Cells of unallocated chunks read as unpopulated.
	/// </summary>
	/// <param name="localColumn">
	/// The first column, in <c>[-<see cref="ChunkSize"/>, 2 * <see cref="ChunkSize"/>)</c>;
	/// <c>localColumn + count</c> must not exceed <c>2 * <see cref="ChunkSize"/></c>.
	/// </param>
	/// <param name="localRow">The row, in <c>[-<see cref="ChunkSize"/>, 2 * <see cref="ChunkSize"/>)</c>.</param>
	/// <param name="count">The number of bits to read, 1 to 64.</param>
	/// <returns>Bit <c>i</c> is the presence of local column <c>localColumn + i</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkIndex"/> is not allocated, or an argument is outside its range.
	/// </exception>
	public ulong ReadPresenceBits(
		int chunkIndex,
		int localColumn,
		int localRow,
		int count
	) {
		_ = GetEntry( chunkIndex );
		ArgumentOutOfRangeException.ThrowIfLessThan( count, 1 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( count, 64 );
		ArgumentOutOfRangeException.ThrowIfLessThan( localColumn, -ChunkSize );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( localColumn, ( 2 * ChunkSize ) - count );
		ArgumentOutOfRangeException.ThrowIfLessThan( localRow, -ChunkSize );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( localRow, 2 * ChunkSize );

		int neighbourRow = ( chunkIndex * NeighbourCount ) + ( ( ( localRow >> ChunkShift ) + 1 ) * 3 ) + 1;
		int rowStart = ( localRow & ChunkMask ) * PresenceWordsPerRow;
		ulong result = 0;
		int read = 0;
		while( read < count ) {
			int column = localColumn + read;
			int chunkColumn = column & ChunkMask;
			int bit = chunkColumn & 63;
			int segment = Math.Min( count - read, Math.Min( ChunkSize - chunkColumn, 64 - bit ) );
			int neighbour = _neighbours[ neighbourRow + ( column >> ChunkShift ) ];
			if( neighbour != NoChunk ) {
				ulong word = _presence[ neighbour ]![ rowStart + ( chunkColumn >> 6 ) ];
				result |= ( ( word >> bit ) & PresenceBits.Mask( segment ) ) << read;
			}
			read += segment;
		}
		return result;
	}

	/// <summary>
	/// Gets the exact bounding box of the populated cells. Computed on each call.
	/// </summary>
	/// <returns><see langword="false"/> when no cell is populated.</returns>
	/// <exception cref="OverflowException">The populated extent cannot be represented as an <see cref="int"/>.</exception>
	public bool TryGetPopulatedBounds(
		out int column,
		out int row,
		out int width,
		out int height
	) {
		int minColumn = int.MaxValue;
		int minRow = int.MaxValue;
		int maxColumn = int.MinValue;
		int maxRow = int.MinValue;
		bool foundPopulated = false;
		int words = PresenceWordsPerRow;
		Span<ulong> columns = stackalloc ulong[ words ];
		for( int i = 0; i < ChunkCapacity; i++ ) {
			ChunkEntry? entry = _entries[ i ];
			if( entry is null ) {
				continue;
			}
			ReadOnlySpan<ulong> presence = _presence[ i ]!;
			int first = presence.IndexOfAnyExcept( 0UL );
			if( first < 0 ) {
				continue;
			}
			foundPopulated = true;
			int last = presence.LastIndexOfAnyExcept( 0UL );
			int baseColumn = entry.Column << ChunkShift;
			int baseRow = entry.Row << ChunkShift;
			minRow = Math.Min( minRow, baseRow + ( first / words ) );
			maxRow = Math.Max( maxRow, baseRow + ( last / words ) );

			columns.Clear();
			for( int r = first / words; r <= last / words; r++ ) {
				ReadOnlySpan<ulong> presenceRow = presence.Slice( r * words, words );
				for( int w = 0; w < words; w++ ) {
					columns[ w ] |= presenceRow[ w ];
				}
			}
			int firstWord = columns.IndexOfAnyExcept( 0UL );
			int lastWord = columns.LastIndexOfAnyExcept( 0UL );
			minColumn = Math.Min(
				minColumn,
				baseColumn + ( firstWord << 6 ) + BitOperations.TrailingZeroCount( columns[ firstWord ] )
			);
			maxColumn = Math.Max(
				maxColumn,
				baseColumn + ( lastWord << 6 ) + 63 - BitOperations.LeadingZeroCount( columns[ lastWord ] )
			);
		}

		if( !foundPopulated ) {
			column = 0;
			row = 0;
			width = 0;
			height = 0;
			return false;
		}
		column = minColumn;
		row = minRow;
		width = checked( (int)( (long)maxColumn - minColumn + 1 ) );
		height = checked( (int)( (long)maxRow - minRow + 1 ) );
		return true;
	}

	/// <summary>
	/// Enumerates the allocated chunk indices in ascending order. The layout
	/// must not have chunks allocated or removed during enumeration.
	/// </summary>
	public ChunkIndexEnumerator EnumerateChunks() {
		return new ChunkIndexEnumerator( this );
	}

	/// <summary>
	/// Finds a chunk index without allocating; <c>-1</c> if not allocated.
	/// </summary>
	internal int FindChunk(
		int chunkColumn,
		int chunkRow
	) {
		ChunkEntry? entry = FindEntry( chunkColumn, chunkRow );
		return entry is null ? NoChunk : entry.Index;
	}

	/// <summary>
	/// Finds an allocated chunk without allocating, checking the one-entry
	/// lookup cache inline before falling back to the dictionary.
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	internal ChunkEntry? FindEntry(
		int chunkColumn,
		int chunkRow
	) {
		ChunkEntry? cache = _cache;
		return cache is not null
			&& cache.Column == chunkColumn
			&& cache.Row == chunkRow
				? cache
				: FindEntryUncached( chunkColumn, chunkRow );
	}

	/// <summary>
	/// Gets an allocated chunk, allocating it if required.
	/// </summary>
	internal ChunkEntry GetOrCreateEntry(
		int chunkColumn,
		int chunkRow
	) {
		return FindEntry( chunkColumn, chunkRow ) ?? AllocateChunk( chunkColumn, chunkRow );
	}

	/// <summary>
	/// The presence array of an allocated chunk, unchecked.
	/// </summary>
	internal ulong[] GetPresenceArray(
		int chunkIndex
	) {
		return _presence[ chunkIndex ]!;
	}

	/// <summary>
	/// The index of a neighbour of an allocated chunk, unchecked.
	/// </summary>
	internal int GetNeighbour(
		int chunkIndex,
		int neighbour
	) {
		return _neighbours[ ( chunkIndex * NeighbourCount ) + neighbour ];
	}

	/// <summary>
	/// Incremented every time <paramref name="chunkIndex"/> is (re)allocated, so
	/// storage synchronised to the layout can detect an index that was freed
	/// and reused between two synchronisations.
	/// </summary>
	internal int GetGeneration(
		int chunkIndex
	) {
		return _generations[ chunkIndex ];
	}

	internal bool IsBitSet(
		ulong[] presence,
		int localColumn,
		int localRow
	) {
		ulong word = presence[ ( localRow * PresenceWordsPerRow ) + ( localColumn >> 6 ) ];
		return ( word & ( 1UL << ( localColumn & 63 ) ) ) != 0;
	}

	internal static int CountPopulated(
		ReadOnlySpan<ulong> presence
	) {
		int count = 0;
		for( int i = 0; i < presence.Length; i++ ) {
			count += BitOperations.PopCount( presence[ i ] );
		}
		return count;
	}

	private ChunkEntry GetEntry(
		int chunkIndex
	) {
		ChunkEntry? entry = (uint)chunkIndex < (uint)ChunkCapacity
			? _entries[ chunkIndex ]
			: null;
		return entry ?? throw new ArgumentOutOfRangeException(
			nameof( chunkIndex ),
			chunkIndex,
			"The chunk index is not allocated."
		);
	}

	/// <summary>
	/// Finds an allocated chunk in the lookup dictionary, bypassing and not
	/// updating the one-entry cache; for callers that keep their own cache.
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	internal ChunkEntry? LookupEntry(
		int chunkColumn,
		int chunkRow
	) {
		return _lookup.TryGetValue( Key( chunkColumn, chunkRow ), out ChunkEntry? entry )
			? entry
			: null;
	}

	[MethodImpl( MethodImplOptions.NoInlining )]
	private ChunkEntry? FindEntryUncached(
		int chunkColumn,
		int chunkRow
	) {
		if( _lookup.TryGetValue( Key( chunkColumn, chunkRow ), out ChunkEntry? entry ) ) {
			_cache = entry;
			return entry;
		}
		return null;
	}

	private ChunkEntry AllocateChunk(
		int chunkColumn,
		int chunkRow
	) {
		if( ( (long)chunkColumn << ChunkShift ) < int.MinValue
			|| ( ( (long)chunkColumn << ChunkShift ) + ChunkMask ) > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( chunkColumn ) );
		}
		if( ( (long)chunkRow << ChunkShift ) < int.MinValue
			|| ( ( (long)chunkRow << ChunkShift ) + ChunkMask ) > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( chunkRow ) );
		}
		if( !_free.TryPop( out int index ) ) {
			index = ChunkCapacity;
			EnsureCapacity( index + 1 );
			ChunkCapacity = index + 1;
		}

		ChunkEntry entry = new( chunkColumn, chunkRow, index, new ulong[ PresenceWordsPerChunk ] );
		bool wasEmpty = _lookup.Count == 0;
		_lookup.Add( Key( chunkColumn, chunkRow ), entry );
		_entries[ index ] = entry;
		_presence[ index ] = entry.Presence;
		_generations[ index ]++;

		int start = index * NeighbourCount;
		for( int k = 0; k < NeighbourCount; k++ ) {
			if( k == CentreNeighbour ) {
				_neighbours[ start + k ] = index;
				continue;
			}
			long neighbourColumn = (long)chunkColumn + ( k % 3 ) - 1;
			long neighbourRow = (long)chunkRow + ( k / 3 ) - 1;
			int neighbour = neighbourColumn >= int.MinValue && neighbourColumn <= int.MaxValue
				&& neighbourRow >= int.MinValue && neighbourRow <= int.MaxValue
				&& _lookup.TryGetValue( Key( (int)neighbourColumn, (int)neighbourRow ), out ChunkEntry? found )
				? found.Index
				: NoChunk;
			_neighbours[ start + k ] = neighbour;
			if( neighbour != NoChunk ) {
				_neighbours[ ( neighbour * NeighbourCount ) + ( NeighbourCount - 1 - k ) ] = index;
			}
		}

		_cache = entry;
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
		return entry;
	}

	private void Free(
		ChunkEntry entry
	) {
		int index = entry.Index;
		int start = index * NeighbourCount;
		for( int k = 0; k < NeighbourCount; k++ ) {
			int neighbour = _neighbours[ start + k ];
			if( k != CentreNeighbour && neighbour != NoChunk ) {
				_neighbours[ ( neighbour * NeighbourCount ) + ( NeighbourCount - 1 - k ) ] = NoChunk;
			}
			_neighbours[ start + k ] = NoChunk;
		}
		_entries[ index ] = null;
		_presence[ index ] = null;
		_free.Push( index );
		_cache = null;
		_hullDirty = true;
		Version++;
	}

	private void EnsureCapacity(
		int required
	) {
		int length = _entries.Length;
		if( length >= required ) {
			return;
		}
		int size = Math.Max( required, Math.Max( 4, length * 2 ) );
		Array.Resize( ref _entries, size );
		Array.Resize( ref _presence, size );
		Array.Resize( ref _generations, size );
		Array.Resize( ref _neighbours, size * NeighbourCount );
		_neighbours.AsSpan( length * NeighbourCount ).Fill( NoChunk );
	}

	private void EnsureHull() {
		if( !_hullDirty ) {
			return;
		}
		_minChunkColumn = int.MaxValue;
		_minChunkRow = int.MaxValue;
		_maxChunkColumn = int.MinValue;
		_maxChunkRow = int.MinValue;
		for( int i = 0; i < ChunkCapacity; i++ ) {
			ChunkEntry? entry = _entries[ i ];
			if( entry is null ) {
				continue;
			}
			_minChunkColumn = Math.Min( _minChunkColumn, entry.Column );
			_maxChunkColumn = Math.Max( _maxChunkColumn, entry.Column );
			_minChunkRow = Math.Min( _minChunkRow, entry.Row );
			_maxChunkRow = Math.Max( _maxChunkRow, entry.Row );
		}
		_hullDirty = false;
	}

	private static int SetBits(
		ulong[] presence,
		int rowStart,
		int localColumn,
		int count
	) {
		int changed = 0;
		while( count > 0 ) {
			int shift = localColumn & 63;
			int segment = Math.Min( count, 64 - shift );
			ulong bits = PresenceBits.Mask( segment ) << shift;
			ref ulong word = ref presence[ rowStart + ( localColumn >> 6 ) ];
			changed += BitOperations.PopCount( bits & ~word );
			word |= bits;
			localColumn += segment;
			count -= segment;
		}
		return changed;
	}

	private static int ClearBits(
		ulong[] presence,
		int rowStart,
		int localColumn,
		int count
	) {
		int changed = 0;
		while( count > 0 ) {
			int shift = localColumn & 63;
			int segment = Math.Min( count, 64 - shift );
			ulong bits = PresenceBits.Mask( segment ) << shift;
			ref ulong word = ref presence[ rowStart + ( localColumn >> 6 ) ];
			changed += BitOperations.PopCount( bits & word );
			word &= ~bits;
			localColumn += segment;
			count -= segment;
		}
		return changed;
	}

	private static long Key(
		int chunkColumn,
		int chunkRow
	) {
		return ( (long)chunkColumn << 32 ) | (uint)chunkRow;
	}

	/// <summary>
	/// An allocated chunk; immutable so it can be cached as a single reference.
	/// </summary>
	internal sealed class ChunkEntry(
		int column,
		int row,
		int index,
		ulong[] presence
	) {
		public int Column { get; } = column;

		public int Row { get; } = row;

		public int Index { get; } = index;

		public ulong[] Presence { get; } = presence;
	}
}
