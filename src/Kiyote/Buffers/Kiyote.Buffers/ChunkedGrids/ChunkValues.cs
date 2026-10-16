using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// One value array per allocated chunk of a <see cref="ChunkedGridLayout"/>,
/// indexed by chunk index.
/// </summary>
/// <remarks>
/// <para>
/// The layout holds no reference to its value storage, so storage
/// synchronises itself: <see cref="Synchronize"/> is a single comparison while
/// the layout's <see cref="ChunkedGridLayout.Version"/> is unchanged, and
/// otherwise reconciles every index. A chunk index freed and reused between
/// two synchronisations is detected through the layout's per-index
/// generation, so it never inherits stale values.
/// </para>
/// <para>
/// Arrays are allocated rather than cleared and reused, so a span that
/// outlives its chunk refers to orphaned memory instead of another chunk.
/// </para>
/// </remarks>
internal sealed class ChunkValues<T> where T : unmanaged {

	private readonly ChunkedGridLayout _layout;
	private readonly int _area;
	private T[]?[] _arrays;
	private int[] _generations;
	private int _version;

	public ChunkValues(
		ChunkedGridLayout layout
	) {
		_layout = layout;
		_area = layout.ChunkSize * layout.ChunkSize;
		_arrays = [];
		_generations = [];
		Reconcile();
	}

	public ChunkedGridLayout Layout => _layout;

	/// <summary>
	/// Validates that <paramref name="chunkSize"/> is a valid layout chunk size
	/// whose rows are a whole number of <see cref="Vector256{T}"/> lanes of <typeparamref name="T"/>.
	/// </summary>
	public static void ValidateChunkSize(
		int chunkSize,
		string paramName
	) {
		int lanes = Math.Max( 1, Vector256<byte>.Count / Unsafe.SizeOf<T>() );
		if( chunkSize <= 0
			|| chunkSize > ChunkedGridLayout.MaximumChunkSize
			|| !BitOperations.IsPow2( chunkSize )
			|| chunkSize % lanes != 0
		) {
			throw new ArgumentOutOfRangeException(
				paramName,
				chunkSize,
				$"Chunk size must be a power of two between {lanes} and {ChunkedGridLayout.MaximumChunkSize} and a multiple of {lanes}."
			);
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void Synchronize() {
		if( _version != _layout.Version ) {
			Reconcile();
		}
	}

	/// <summary>
	/// Gets the values of an allocated chunk. Assumes this storage is synchronised.
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public T[] Get(
		int chunkIndex
	) {
		T[]?[] arrays = _arrays;
		T[]? values = (uint)chunkIndex < (uint)arrays.Length
			? arrays[ chunkIndex ]
			: null;
		return values ?? throw new ArgumentOutOfRangeException(
			nameof( chunkIndex ),
			chunkIndex,
			"The chunk index is not allocated."
		);
	}

	/// <summary>
	/// Adds storage for a chunk the layout has just allocated. Avoids a full
	/// reconcile when this storage was synchronised to <paramref name="previousVersion"/>.
	/// </summary>
	public void OnAllocated(
		int chunkIndex,
		int previousVersion
	) {
		if( _version != previousVersion ) {
			Reconcile();
			return;
		}
		EnsureCapacity( _layout.ChunkCapacity );
		_arrays[ chunkIndex ] = new T[ _area ];
		_generations[ chunkIndex ] = _layout.GetGeneration( chunkIndex );
		_version = _layout.Version;
	}

	/// <summary>
	/// Drops the storage of a chunk the layout has just removed. Avoids a full
	/// reconcile when this storage was synchronised to <paramref name="previousVersion"/>.
	/// </summary>
	public void OnRemoved(
		int chunkIndex,
		int previousVersion
	) {
		if( _version != previousVersion ) {
			Reconcile();
			return;
		}
		_arrays[ chunkIndex ] = null;
		_version = _layout.Version;
	}

	/// <summary>
	/// Exchanges all storage with <paramref name="other"/>, which must share this layout.
	/// </summary>
	public void SwapWith(
		ChunkValues<T> other
	) {
		(_arrays, other._arrays) = (other._arrays, _arrays);
		(_generations, other._generations) = (other._generations, _generations);
		(_version, other._version) = (other._version, _version);
	}

	/// <summary>
	/// Copies the values of every allocated chunk into <paramref name="other"/>,
	/// which must share this layout.
	/// </summary>
	public void CopyTo(
		ChunkValues<T> other
	) {
		Synchronize();
		other.Synchronize();
		T[]?[] target = other._arrays;
		int capacity = _layout.ChunkCapacity;
		for( int i = 0; i < capacity; i++ ) {
			T[]? values = _arrays[ i ];
			if( values is not null ) {
				values.AsSpan().CopyTo( target[ i ]! );
			}
		}
	}

	/// <summary>
	/// Sets every value of every allocated chunk to <see langword="default"/>.
	/// </summary>
	public void Clear() {
		Synchronize();
		int capacity = _layout.ChunkCapacity;
		for( int i = 0; i < capacity; i++ ) {
			T[]? values = _arrays[ i ];
			if( values is not null ) {
				Array.Clear( values );
			}
		}
	}

	private void Reconcile() {
		int capacity = _layout.ChunkCapacity;
		EnsureCapacity( capacity );
		for( int i = 0; i < capacity; i++ ) {
			if( !_layout.IsAllocated( i ) ) {
				_arrays[ i ] = null;
				continue;
			}
			int generation = _layout.GetGeneration( i );
			if( _arrays[ i ] is null || _generations[ i ] != generation ) {
				_arrays[ i ] = new T[ _area ];
				_generations[ i ] = generation;
			}
		}
		_version = _layout.Version;
	}

	private void EnsureCapacity(
		int required
	) {
		int length = _arrays.Length;
		if( length >= required ) {
			return;
		}
		int size = Math.Max( required, Math.Max( 4, length * 2 ) );
		Array.Resize( ref _arrays, size );
		Array.Resize( ref _generations, size );
	}
}
