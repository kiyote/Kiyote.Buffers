using System.Numerics;
using System.Runtime.InteropServices;

namespace Kiyote.Buffers.Vectors;

internal class VectorBuffer<T> : IBuffer<T> where T : struct, INumber<T> {

	private readonly int _columns;
	private readonly int _rows;
	private readonly T[][] _content;
	private readonly int _allocWidth;
	private readonly int _opCount;

	public VectorBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		_columns = columns;
		_rows = rows;
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

	int IBuffer<T>.Columns => _columns;

	int IBuffer<T>.Rows => _rows;

	public void Add(
		T amount
	) {
		Vector<T> amounts = Vector.Create( amount );

		for( int row = 0; row < _rows; row++ ) {
			Span<Vector<T>> vcontent = MemoryMarshal.Cast<T, Vector<T>>( _content[row].AsSpan() );

			for (int i = 0; i < _opCount; i++) {
				vcontent[ i ] = vcontent[ i ] + amounts;
			}
		}
	}
}
