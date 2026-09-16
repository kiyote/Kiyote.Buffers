using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Numerics.Benchmarks;

[MemoryDiagnoser]
public class NumericBufferOperatorBenchmarks {

	private readonly NumericBuffer<float> _floatBuffer;
	private readonly NumericBuffer<int> _intBuffer;
	private readonly NumericBuffer<float> _floatDestination;
	private readonly NumericBuffer<int> _intDestination;
	private readonly INumericBufferOperator _operator;

	public NumericBufferOperatorBenchmarks() {
		_floatBuffer = new NumericBuffer<float>( 1920, 1080, 0.0f );		
		_intBuffer = new NumericBuffer<int>( 1920, 1080, 0 );
		_floatDestination = new NumericBuffer<float>( 1920, 1080, 0.0f );
		_intDestination = new NumericBuffer<int>( 1920, 1080, 0 );
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
	public void AddTo_Float() {
		_operator.AddTo( _floatBuffer, _floatDestination, 1.0f );
	}

	[Benchmark]
	public void AddTo_Int() {
		_operator.AddTo( _intBuffer, _intDestination, 1 );
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
	public void SubtractTo_Float() {
		_operator.SubtractTo( _floatBuffer, _floatDestination, 1.0f );
	}

	[Benchmark]
	public void SubtractTo_Int() {
		_operator.SubtractTo( _intBuffer, _intDestination, 1 );
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
	public void MultiplyTo_Float() {
		_operator.MultiplyTo( _floatBuffer, _floatDestination, 2.0f );
	}

	[Benchmark]
	public void MultiplyTo_Int() {
		_operator.MultiplyTo( _intBuffer, _intDestination, 2 );
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
	public void DivideTo_Float() {
		_operator.DivideTo( _floatBuffer, _floatDestination, 2.0f );
	}

	[Benchmark]
	public void DivideTo_Int() {
		_operator.DivideTo( _intBuffer, _intDestination, 2 );
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
	public void NormalizeTo_Float() {
		_operator.NormalizeTo( _floatBuffer, _floatDestination );
	}

	[Benchmark]
	public void NormalizeTo_Int() {
		_operator.NormalizeTo( _intBuffer, _intDestination );
	}
}
