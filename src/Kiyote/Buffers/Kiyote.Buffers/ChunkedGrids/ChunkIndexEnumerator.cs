namespace Kiyote.Buffers.ChunkedGrids;

/// <summary>
/// Enumerates the allocated chunk indices of a <see cref="ChunkedGridLayout"/>
/// in ascending order. Throws <see cref="InvalidOperationException"/> if a
/// chunk is allocated or removed during enumeration.
/// </summary>
public ref struct ChunkIndexEnumerator {

	private readonly ChunkedGridLayout _layout;
	private readonly int _version;
	private int _index;

	internal ChunkIndexEnumerator(
		ChunkedGridLayout layout
	) {
		_layout = layout;
		_version = layout.Version;
		_index = -1;
	}

	/// <summary>
	/// The current chunk index.
	/// </summary>
	public readonly int Current {
		get {
			if( _layout.Version != _version ) {
				throw new InvalidOperationException( "The grid topology changed during enumeration." );
			}
			return _index;
		}
	}

	/// <summary>
	/// Advances to the next allocated chunk index.
	/// </summary>
	/// <exception cref="InvalidOperationException">A chunk was allocated or removed during enumeration.</exception>
	public bool MoveNext() {
		if( _layout.Version != _version ) {
			throw new InvalidOperationException( "The grid topology changed during enumeration." );
		}
		int capacity = _layout.ChunkCapacity;
		while( ++_index < capacity ) {
			if( _layout.IsAllocated( _index ) ) {
				return true;
			}
		}
		_index = capacity;
		return false;
	}

	/// <summary>
	/// Returns this enumerator, enabling <see langword="foreach"/>.
	/// </summary>
	public readonly ChunkIndexEnumerator GetEnumerator() {
		return this;
	}
}
