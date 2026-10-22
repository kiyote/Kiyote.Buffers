using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Numerics.Benchmarks;

[MemoryDiagnoser( displayGenColumns: false )]
public class NumericLayoutBenchmarks {

	private INumericBuffer<float> _source = null!;
	private INumericBuffer<float> _destination = null!;
	private INumericBufferOperator _operator = null!;

	[Params( false, true )]
	public bool Ragged { get; set; }

	[Params( 1000 )]
	public int Columns { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_source = CreateBuffer( 1f );
		_destination = CreateBuffer( 0f );
		_source[ Columns - 1, 999 ] = 2f;
		_operator = new NumericBufferOperator();
	}

	[Benchmark]
	public INumericBuffer<float> Create() {
		return CreateBuffer( 1f );
	}

	[Benchmark]
	public void AddToDestination() {
		_operator.Add( _source, _destination, 1f );
	}

	[Benchmark]
	public float Max() {
		return _operator.Max( _source );
	}

	[Benchmark]
	public void NormalizeToDestination() {
		_operator.Normalize( _source, _destination );
	}

	private INumericBuffer<float> CreateBuffer( float value ) {
		return Ragged
			? new RaggedNumericBuffer<float>( Columns, 1000, value )
			: new FlatNumericBuffer<float>( Columns, 1000, value );
	}
}
