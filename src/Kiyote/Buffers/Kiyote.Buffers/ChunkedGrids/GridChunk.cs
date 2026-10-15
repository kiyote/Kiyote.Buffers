namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// A view over one value buffer of a single allocated chunk of a
/// <see cref="ChunkedGrid{T}"/> or <see cref="DoubleBufferedChunkedGrid{T}"/>.
/// </summary>
/// <remarks>
/// <see cref="Values"/> is row-major with exactly <see cref="Size"/> elements per
/// row, and <see cref="Size"/> is always a multiple of the number of
/// <typeparamref name="T"/> lanes in a <see cref="System.Runtime.Intrinsics.Vector256{T}"/>,
/// so every row can be processed with whole vectors. <see cref="Presence"/>
/// stores one bit per cell, <see cref="PresenceWordsPerRow"/> words per row,
/// with bit <c>c &amp; 63</c> of word <c>c &gt;&gt; 6</c> describing local column <c>c</c>.
/// </remarks>
public readonly ref struct GridChunk<T> where T : unmanaged {

	private readonly int _shift;
	private readonly ulong _fullWordMask;

	internal GridChunk(
		GridChunkStorage<T> storage,
		int buffer,
		int size,
		int shift,
		int presenceWordsPerRow,
		ulong fullWordMask
	) {
		ChunkColumn = storage.ChunkColumn;
		ChunkRow = storage.ChunkRow;
		Size = size;
		PresenceWordsPerRow = presenceWordsPerRow;
		_shift = shift;
		_fullWordMask = fullWordMask;
		Values = new Span<T>( storage.Buffers[ buffer ], 0, size * size );
		Presence = new Span<ulong>( storage.Presence, 0, size * presenceWordsPerRow );
	}

	public int ChunkColumn { get; }

	public int ChunkRow { get; }

	/// <summary>
	/// The grid column of the first cell in this chunk.
	/// </summary>
	public int Column => ChunkColumn << _shift;

	/// <summary>
	/// The grid row of the first cell in this chunk.
	/// </summary>
	public int Row => ChunkRow << _shift;

	public int Size { get; }

	public int PresenceWordsPerRow { get; }

	public Span<T> Values { get; }

	public Span<ulong> Presence { get; }

	public Span<T> GetRowValues(
		int localRow
	) {
		return Values.Slice( localRow << _shift, Size );
	}

	public Span<ulong> GetPresenceRow(
		int localRow
	) {
		return Presence.Slice( localRow * PresenceWordsPerRow, PresenceWordsPerRow );
	}

	public bool IsPopulated(
		int localColumn,
		int localRow
	) {
		ulong word = Presence[ ( localRow * PresenceWordsPerRow ) + ( localColumn >> 6 ) ];
		return ( word & ( 1UL << ( localColumn & 63 ) ) ) != 0;
	}

	/// <summary>
	/// <c>true</c> when every cell in the chunk is populated, allowing the
	/// caller to skip masking entirely.
	/// </summary>
	public bool IsFull {
		get {
			ReadOnlySpan<ulong> presence = Presence;
			for( int i = 0; i < presence.Length; i++ ) {
				if( presence[ i ] != _fullWordMask ) {
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// <c>true</c> when no cell in the chunk is populated.
	/// </summary>
	public bool IsEmpty => !Presence.ContainsAnyExcept( 0UL );
}
