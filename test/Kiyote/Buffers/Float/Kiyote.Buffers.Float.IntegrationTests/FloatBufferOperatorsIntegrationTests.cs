namespace Kiyote.Buffers.Float.UnitTests;

public class FloatBufferOperatorsIntegrationTests {

	private IServiceProvider _provider;
	private IServiceScope _scope;
	private IBufferFactory _bufferFactory;
	private IBuffer<float> _buffer;
	private IFloatBufferOperators _operators;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		var services = new ServiceCollection();
		services.AddBuffers();

		_provider = services.BuildServiceProvider();
	}

	[SetUp]
	public void Setup() {
		_scope = _provider.CreateScope();

		_bufferFactory = _scope.ServiceProvider.GetRequiredService<IBufferFactory>();
		_buffer = _bufferFactory.Create<float>( 2, 2 );

		_operators = new FloatBufferOperators(
			_scope.ServiceProvider.GetRequiredService<IBufferOperator>()
		);
	}

	[TearDown]
	public void TearDown() {
		_scope.Dispose();
	}

	[Test]
	public void Normalize_NegativeToPostive_ResultInRange() {
		_buffer[0, 0] = -1.0f;
		_buffer[1, 0] = 0.0f;
		_buffer[0, 1] = 0.0f;
		_buffer[1, 1] = 1.0f;

		IBuffer<float> actual = _bufferFactory.Create<float>( _buffer.Size );

		_operators.Normalize( _buffer, 0.0f, 1.0f, actual );

		for( int r = 0; r < _buffer.Size.Height; r++ ) {
			for( int c = 0; c < _buffer.Size.Width; c++ ) {
				Assert.That( actual[c, r], Is.GreaterThanOrEqualTo( 0.0f ) );
				Assert.That( actual[c, r], Is.LessThanOrEqualTo( 1.0f ) );
			}
		}

		Assert.That( actual[0, 0], Is.EqualTo( 0.0f ) );
		Assert.That( actual[1, 0], Is.EqualTo( 0.5f ) );
		Assert.That( actual[0, 1], Is.EqualTo( 0.5f ) );
		Assert.That( actual[1, 1] , Is.EqualTo( 1.0f ) );
	}
}
