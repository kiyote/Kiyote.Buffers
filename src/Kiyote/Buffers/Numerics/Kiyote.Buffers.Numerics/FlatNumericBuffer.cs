using System.Numerics;

namespace Kiyote.Buffers.Numerics;

public sealed class FlatNumericBuffer<T> : INumericBuffer<T> where T : struct, INumber<T> {

	private readonly int _columns;
	private readonly int _rows;
	internal readonly T[] Content;
	private readonly int _allocWidth;
	internal readonly int OpCount;

	public FlatNumericBuffer(
		int columns,
		int rows,
		T defaultValue
	) {
		_columns = columns;
		_rows = rows;
		Content = new T[ rows * _allocWidth ];
		if( columns % Vector<T>.Count == 0 ) {
			_allocWidth = columns;
		} else {
			_allocWidth = ( ( columns / Vector<T>.Count ) + 1 ) * Vector<T>.Count;
		}
		Content = new T[ rows * _allocWidth ];
		OpCount = _allocWidth / Vector<T>.Count;
		for( int i = 0; i < rows; i++ ) {
			if( defaultValue != default ) {
				Array.Fill( Content, defaultValue, i * _allocWidth, _allocWidth );
			}
		}
	}

	T IBuffer<T>.this[ int column, int row ] { get => Content[ (row * _allocWidth) + column ]; set => Content[ (row * _allocWidth) + column ] = value; }

	int IBuffer<T>.Columns => _columns;

	int IBuffer<T>.Rows => _rows;

	Span<T> IBuffer<T>.GetRowSpan( int row ) {
		// Rows are allocated wider than the logical column count so they can be
		// processed as whole vectors; the padding must never be exposed.
		return Content.AsSpan( row * _allocWidth, _columns );
	}

}
