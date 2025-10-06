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
		services.AddNumericBuffers();

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

		Assert.That( min, Is.EqualTo( 0.0f ) );
	}

	[Test]
	public void MinMax_NumericBuffer_ValuesReturned() {
		_operators.Add( _buffer!, 10.0f );
		_buffer![ 5, 8 ] = 0.0f;
		_buffer![ 5, 1 ] = 20.0f;
		
		(float min, float max) = _operators.MinMax( _buffer );

		Assert.That( min, Is.EqualTo( 0.0f ) );
		Assert.That( max, Is.EqualTo( 20.0f ) );
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

		Assert.That( _buffer![ 0, 0 ], Is.EqualTo( 0.0f ) );
		Assert.That( _buffer![ 9, 9 ], Is.EqualTo( 1.0f ) );
	}
}
