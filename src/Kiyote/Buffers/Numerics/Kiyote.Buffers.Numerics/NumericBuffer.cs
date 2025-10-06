using System.Numerics;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Numerics;

internal class NumericBuffer<T> : INumericBuffer<T> where T : struct, INumber<T> {

	private readonly ISize _size;
	internal readonly T[][] Content;
	private readonly int _allocWidth;
	internal readonly int OpCount;

	public NumericBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		_size = new Point( columns, rows );
		Content = new T[ rows ][];
		if (columns % Vector<float>.Count == 0) {
			_allocWidth = columns;
		} else {
			_allocWidth = ( ( columns / Vector<float>.Count ) + 1 ) * Vector<float>.Count;
		}
		OpCount = _allocWidth / Vector<T>.Count;
		for( int i = 0; i < rows; i++ ) {
			Content[ i ] = new T[ _allocWidth ];
			if (defaultValue != default) {
				Array.Fill( Content[ i ], defaultValue );
			}
		}
	}

	T IBuffer<T>.this[ int column, int row ] { get => Content[row][column]; set => Content[ row ][ column ] = value; }

	ISize IBuffer<T>.Size => _size;

}
