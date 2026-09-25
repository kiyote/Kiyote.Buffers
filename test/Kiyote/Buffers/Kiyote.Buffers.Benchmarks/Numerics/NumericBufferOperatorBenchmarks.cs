using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Numerics.Benchmarks;

[MemoryDiagnoser]
public class NumericBufferOperatorBenchmarks {

	private readonly RaggedNumericBuffer<float> _floatBuffer;
	private readonly RaggedNumericBuffer<int> _intBuffer;
	private readonly RaggedNumericBuffer<float> _floatDestination;
	private readonly RaggedNumericBuffer<int> _intDestination;
	private readonly INumericBufferOperator _operator;

	public NumericBufferOperatorBenchmarks() {
		_floatBuffer = new RaggedNumericBuffer<float>( 1920, 1080, 0.0f );		
		_intBuffer = new RaggedNumericBuffer<int>( 1920, 1080, 0 );
		_floatDestination = new RaggedNumericBuffer<float>( 1920, 1080, 0.0f );
		_intDestination = new RaggedNumericBuffer<int>( 1920, 1080, 0 );
		_operator = new NumericBufferOperator();

		_operator.Add( _floatBuffer, 10.0f );
		_operator.Add( _intBuffer, 10 );
	}

	[Benchmark]
	public void Add_Float() {
		_operator.Add( _floatBuffer, 1.0f );
	}

	[Benchmark]
	public void Add_Int() {
		_operator.Add( _intBuffer, 1 );
	}

	[Benchmark]
	public void Add_FloatToDestination() {
		_operator.Add( _floatBuffer, _floatDestination, 1.0f );
	}

	[Benchmark]
	public void Add_IntToDestination() {
		_operator.Add( _intBuffer, _intDestination, 1 );
	}

	[Benchmark]
	public void Subtract_Float() {
		_operator.Subtract( _floatBuffer, 1.0f );
	}

	[Benchmark]
	public void Subtract_Int() {
		_operator.Subtract( _intBuffer, 1 );
	}

	[Benchmark]
	public void Subtract_FloatToDestination() {
		_operator.Subtract( _floatBuffer, _floatDestination, 1.0f );
	}

	[Benchmark]
	public void Subtract_IntToDestination() {
		_operator.Subtract( _intBuffer, _intDestination, 1 );
	}

	[Benchmark]
	public void Max_Float() {
		_operator.Max( _floatBuffer );
	}

	[Benchmark]
	public void Max_Int() {
		_ = _operator.Max( _intBuffer );
	}

	[Benchmark]
	public void Min_Float() {
		_operator.Min( _floatBuffer );
	}

	[Benchmark]
	public void Min_Int() {
		_ = _operator.Min( _intBuffer );
	}

	[Benchmark]
	public void MinMax_Float() {
		_ = _operator.MinMax( _floatBuffer );
	}

	[Benchmark]
	public void MinMax_Int() {
		_ = _operator.MinMax( _intBuffer );
	}

	[Benchmark]
	public void Multiply_Float() {
		_operator.Multiply( _floatBuffer, 2.0f );
	}

	[Benchmark]
	public void Multiply_Int() {
		_operator.Multiply( _intBuffer, 2 );
	}

	[Benchmark]
	public void Multiply_FloatToDestination() {
		_operator.Multiply( _floatBuffer, _floatDestination, 2.0f );
	}

	[Benchmark]
	public void Multiply_IntToDestination() {
		_operator.Multiply( _intBuffer, _intDestination, 2 );
	}

	[Benchmark]
	public void Divide_Float() {
		_operator.Divide( _floatBuffer, 2.0f );
	}

	[Benchmark]
	public void Divide_Int() {
		_operator.Divide( _intBuffer, 2 );
	}

	[Benchmark]
	public void Divide_FloatToDestination() {
		_operator.Divide( _floatBuffer, _floatDestination, 2.0f );
	}

	[Benchmark]
	public void Divide_IntToDestination() {
		_operator.Divide( _intBuffer, _intDestination, 2 );
	}

	[Benchmark]
	public void Normalize_Float() {
		_operator.Normalize( _floatBuffer );
	}

	[Benchmark]
	public void Normalize_Int() {
		_operator.Normalize( _intBuffer );
	}

	[Benchmark]
	public void Normalize_FloatToDestination() {
		_operator.Normalize( _floatBuffer, _floatDestination );
	}

	[Benchmark]
	public void Normalize_IntToDestination() {
		_operator.Normalize( _intBuffer, _intDestination );
	}

	[Benchmark]
	public void ScaleToRange_Float() {
		_operator.ScaleToRange( _floatBuffer, _floatDestination );
	}

	[Benchmark]
	public void ScaleToRange_Int() {
		_operator.ScaleToRange( _intBuffer, _intDestination );
	}
}
