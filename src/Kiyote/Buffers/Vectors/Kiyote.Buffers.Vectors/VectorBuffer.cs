using System.Numerics;
using System.Runtime.InteropServices;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Vectors;

internal class VectorBuffer<T> : IBuffer<T> where T : struct, INumber<T> {

	private readonly ISize _size;
	private readonly T[][] _content;
	private readonly int _allocWidth;
	private readonly int _opCount;

	public VectorBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		_size = new Point( columns, rows );
		_content = new T[ rows ][];
		if (columns % Vector<float>.Count == 0) {
			_allocWidth = columns;
		} else {
			_allocWidth = ( ( columns / Vector<float>.Count ) + 1 ) * Vector<float>.Count;
		}
		_opCount = _allocWidth / Vector<T>.Count;
		for( int i = 0; i < rows; i++ ) {
			_content[ i ] = new T[ _allocWidth ];
			if (defaultValue != default) {
				Array.Fill( _content[ i ], defaultValue );
			}
		}
	}

	T IBuffer<T>.this[ int column, int row ] { get => _content[row][column]; set => _content[ row ][ column ] = value; }

	public T this[int column, int row] { get => _content[ row ][ column ]; set => _content[ row ][ column ] = value; }

	ISize IBuffer<T>.Size => _size;

	public void Add(
		T amount
	) {
		Vector<T> amounts = Vector.Create( amount );

		for( int row = 0; row < _size.Height; row++ ) {
			Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( _content[row].AsSpan() );

			for (int i = 0; i < _opCount; i++) {
				vcontent[ i ] = vcontent[ i ] + amounts;
			}
		}
	}
}
