namespace Kiyote.Buffers.Numerics.IntegrationTests;

public sealed class NumericBufferOperatorTests {

	private IServiceProvider _provider;
	private IServiceScope _scope;
	private INumericBufferFactory _bufferFactory;
	private INumericBufferOperator _operators;
	private INumericBuffer<float>? _buffer;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		var services = new ServiceCollection();
		_ = services.AddNumericBuffers();

		_provider = services.BuildServiceProvider();
	}

	[SetUp]
	public void SetUp() {
		_scope = _provider.CreateScope();

		_bufferFactory = _scope.ServiceProvider.GetRequiredService<INumericBufferFactory>();
		_operators = _scope.ServiceProvider.GetRequiredService<INumericBufferOperator>();

		_buffer = _bufferFactory.Create( 10, 10, 0.0f );
	}

	[Test]
	public void Add_FixedValue_BufferIncremented() {
		_operators.Add( _buffer!, 1.0f );
		Assert.That( _buffer![0, 0], Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void Max_NumericBuffer_MaximumReturned() {
		_buffer![ 5, 8 ] = 10.0f;

		float max = _operators.Max( _buffer );

		Assert.That( max, Is.EqualTo( 10.0f ) );
	}

	[Test]
	public void Min_NumericBuffer_MinimumReturned() {
		_operators.Add( _buffer!, 10.0f );
		_buffer![ 5, 8 ] = 0.0f;

		float min = _operators.Min( _buffer );

		Assert.That( min, Is.Zero );
	}

	[Test]
	public void MinMax_NumericBuffer_ValuesReturned() {
		_operators.Add( _buffer!, 10.0f );
		_buffer![ 5, 8 ] = 0.0f;
		_buffer![ 5, 1 ] = 20.0f;
		
		(float min, float max) = _operators.MinMax( _buffer );

		Assert.That( min, Is.Zero );
		Assert.That( max, Is.EqualTo( 20.0f ) );
	}

	[Test]
	public void Max_AllValuesBelowPadding_PaddingIgnored() {
		_operators.Add( _buffer!, -1.0f );

		float max = _operators.Max( _buffer! );

		Assert.That( max, Is.EqualTo( -1.0f ) );
	}

	[Test]
	public void Min_AllValuesAbovePadding_PaddingIgnored() {
		_operators.Add( _buffer!, 1.0f );

		float min = _operators.Min( _buffer! );

		Assert.That( min, Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void MinMax_AllValuesBelowPadding_PaddingIgnored() {
		_operators.Add( _buffer!, -5.0f );
		_buffer![ 9, 9 ] = -1.0f;

		(float min, float max) = _operators.MinMax( _buffer );

		Assert.That( min, Is.EqualTo( -5.0f ) );
		Assert.That( max, Is.EqualTo( -1.0f ) );
	}

	[Test]
	public void Max_ValueInTailColumns_ValueReturned() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 10, 10, 0.0f );
		buffer[ 9, 3 ] = 42.0f;

		float max = _operators.Max( buffer );

		Assert.That( max, Is.EqualTo( 42.0f ) );
	}

	[Test]
	public void Min_ValueInTailColumns_ValueReturned() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 10, 10, 0.0f );
		buffer[ 9, 3 ] = -42.0f;

		float min = _operators.Min( buffer );

		Assert.That( min, Is.EqualTo( -42.0f ) );
	}

	[Test]
	public void Normalize_AllValuesBelowPadding_ValuesScaled() {
		_operators.Add( _buffer!, -4.0f );
		_buffer![ 0, 0 ] = -8.0f;

		_operators.Normalize( _buffer );

		Assert.That( _buffer[ 0, 0 ], Is.Zero );
		Assert.That( _buffer[ 1, 0 ], Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void GetRowSpan_PaddedBuffer_LengthMatchesColumns() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 10, 10, 0.0f );

		Span<float> span = buffer.GetRowSpan( 3 );

		Assert.That( span.Length, Is.EqualTo( buffer.Columns ) );
	}

	[Test]
	public void GetRowSpan_ValuesWritten_BufferUpdated() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 10, 10, 0.0f );

		Span<float> span = buffer.GetRowSpan( 3 );
		span.Fill( 7.0f );

		Assert.That( buffer[ 0, 3 ], Is.EqualTo( 7.0f ) );
		Assert.That( buffer[ 9, 3 ], Is.EqualTo( 7.0f ) );
	}

	[Test]
	public void GetRowSpan_ValuesWritten_PaddingUnaffected() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 10, 10, 0.0f );
		_operators.Add( buffer, -1.0f );

		buffer.GetRowSpan( 3 ).Fill( -1.0f );

		// If the span had exposed padding, the fill would have overwritten it
		// and the reduction would still be correct; this guards the reverse
		// case where padding leaks back into the result.
		Assert.That( _operators.Max( buffer ), Is.EqualTo( -1.0f ) );
	}

	[Test]
	public void Multiply_NumericBuffer_ValuesSet() {
		_operators.Add( _buffer!, 2.0f );

		_operators.Multiply( _buffer!, 3.0f );

		Assert.That( _buffer![4, 4], Is.EqualTo( 6.0f ) );
	}

	[Test]
	public void Divide_NumericBuffer_ValuesSet() {
		_operators.Add( _buffer!, 6.0f );

		_operators.Divide( _buffer!, 3.0f );

		Assert.That( _buffer![ 5, 5 ], Is.EqualTo( 2.0f ) );
	}

	[Test]
	public void Normalize_NumericBuffer_ValuesSet() {
		_operators.Add( _buffer!, 6.0f );
		_buffer![ 0, 0 ] = 1.0f;
		_buffer![ 9, 9 ] = 10.0f;

		_operators.Normalize( _buffer! );

		Assert.That( _buffer![ 0, 0 ], Is.Zero );
		Assert.That( _buffer![ 9, 9 ], Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void Normalize_UniformBuffer_ValuesUnchanged() {
		_operators.Add( _buffer!, 6.0f );

		_operators.Normalize( _buffer! );

		Assert.That( _buffer![ 0, 0 ], Is.EqualTo( 6.0f ) );
		Assert.That( _buffer![ 9, 9 ], Is.EqualTo( 6.0f ) );
	}

	[Test]
	public void Clear_NumericBuffer_ValuesSet() {
		_operators.Add( _buffer!, 3.0f );

		_operators.Clear( _buffer!, 1.0f );

		Assert.That( _buffer![ 0, 0 ], Is.EqualTo( 1.0f ) );
		Assert.That( _buffer![ 9, 9 ], Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void Subtract_FixedValue_BufferDecremented() {
		_operators.Add( _buffer!, 10.0f );

		_operators.Subtract( _buffer!, 4.0f );

		Assert.That( _buffer![ 3, 7 ], Is.EqualTo( 6.0f ) );
	}

	[Test]
	public void Clear_UnknownBuffer_ValuesSet() {
		var buffer = new TestNumericBuffer( 3, 3 );

		_operators.Clear( buffer, 5.0f );

		Assert.That( buffer[ 2, 2 ], Is.EqualTo( 5.0f ) );
	}

	[Test]
	public void Add_UnknownBuffer_BufferIncremented() {
		var buffer = new TestNumericBuffer( 3, 3 );

		_operators.Add( buffer, 2.0f );

		Assert.That( buffer[ 2, 2 ], Is.EqualTo( 2.0f ) );
	}

	[Test]
	public void Subtract_UnknownBuffer_BufferDecremented() {
		var buffer = new TestNumericBuffer( 3, 3 );

		_operators.Subtract( buffer, 2.0f );

		Assert.That( buffer[ 2, 2 ], Is.EqualTo( -2.0f ) );
	}

	[Test]
	public void Multiply_UnknownBuffer_ValuesSet() {
		var buffer = new TestNumericBuffer( 3, 3 );
		_operators.Add( buffer, 2.0f );

		_operators.Multiply( buffer, 3.0f );

		Assert.That( buffer[ 2, 2 ], Is.EqualTo( 6.0f ) );
	}

	[Test]
	public void Divide_UnknownBuffer_ValuesSet() {
		var buffer = new TestNumericBuffer( 3, 3 );
		_operators.Add( buffer, 6.0f );

		_operators.Divide( buffer, 3.0f );

		Assert.That( buffer[ 2, 2 ], Is.EqualTo( 2.0f ) );
	}

	[Test]
	public void Max_UnvectorizedBuffer_MaximumReturned() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 2, 2, 1.0f );
		buffer[ 1, 1 ] = 9.0f;

		float max = _operators.Max( buffer );

		Assert.That( max, Is.EqualTo( 9.0f ) );
	}

	[Test]
	public void Min_UnvectorizedBuffer_MinimumReturned() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 2, 2, 5.0f );
		buffer[ 1, 1 ] = 1.0f;

		float min = _operators.Min( buffer );

		Assert.That( min, Is.EqualTo( 1.0f ) );
	}

	[Test]
	public void MinMax_UnvectorizedBuffer_ValuesReturned() {
		INumericBuffer<float> buffer = _bufferFactory.Create( 2, 2, 5.0f );
		buffer[ 0, 1 ] = 1.0f;
		buffer[ 1, 1 ] = 9.0f;

		(float min, float max) = _operators.MinMax( buffer );

		Assert.That( min, Is.EqualTo( 1.0f ) );
		Assert.That( max, Is.EqualTo( 9.0f ) );
	}

	[Test]
	public void Perform_SingleBuffer_ValuesIteratedCorrectly() {
		INumericBuffer<float> output = _bufferFactory.Create( 10, 10, 0.0f );
		_operators.Add( _buffer!, 2.0f );

		_operators.Perform(
			_buffer!,
			value => {
				return value * 2.0f;
			},
			output
		);

		Assert.That( output[ 9, 9 ], Is.EqualTo( 4.0f ) );
	}

	[Test]
	public void Perform_MultiBuffer_ValuesIteratedCorrectly() {
		INumericBuffer<float> other = _bufferFactory.Create( 10, 10, 3.0f );
		INumericBuffer<float> output = _bufferFactory.Create( 10, 10, 0.0f );
		_operators.Add( _buffer!, 2.0f );

		_operators.Perform(
			_buffer!,
			other,
			( left, right ) => {
				return left + right;
			},
			output
		);

		Assert.That( output[ 9, 9 ], Is.EqualTo( 5.0f ) );
	}

	[Test]
	public void Perform_BufferSizesDiffer_ThrowsException() {
		INumericBuffer<float> other = _bufferFactory.Create( 5, 5, 0.0f );
		INumericBuffer<float> output = _bufferFactory.Create( 10, 10, 0.0f );

		Assert.That(
			() => {
				_operators.Perform(
					_buffer!,
					other,
					( left, right ) => {
						return left + right;
					},
					output
				);
			},
			Throws.TypeOf<InvalidOperationException>()
		);
	}

	[Test]
	public void Perform_SourceBufferAndLocation_ValuesIteratedCorrectly() {
		INumericBuffer<float> output = _bufferFactory.Create( 10, 10, 0.0f );
		_operators.Add( _buffer!, 1.0f );

		_operators.Perform(
			_buffer!,
			( column, row, source, value ) => {
				return source[ column, row ] + column + row;
			},
			output
		);

		Assert.That( output[ 2, 3 ], Is.EqualTo( 6.0f ) );
	}

	[Test]
	public void Perform_LocationAndValue_ValuesIteratedCorrectly() {
		INumericBuffer<float> output = _bufferFactory.Create( 10, 10, 0.0f );
		_operators.Add( _buffer!, 1.0f );

		_operators.Perform(
			_buffer!,
			( column, row, value ) => {
				return value + column + row;
			},
			output
		);

		Assert.That( output[ 2, 3 ], Is.EqualTo( 6.0f ) );
	}

	private sealed class TestNumericBuffer : INumericBuffer<float> {

		private readonly float[][] _content;

		public TestNumericBuffer(
			int columns,
			int rows
		) {
			Columns = columns;
			Rows = rows;
			_content = new float[ rows ][];
			for( int r = 0; r < rows; r++ ) {
				_content[ r ] = new float[ columns ];
			}
		}

		public int Columns { get; }

		public int Rows { get; }

		public float this[ int column, int row ] {
			get => _content[ row ][ column ];
			set => _content[ row ][ column ] = value;
		}

		public Span<float> GetRowSpan( int row ) {
			return _content[ row ].AsSpan();
		}
	}
}
