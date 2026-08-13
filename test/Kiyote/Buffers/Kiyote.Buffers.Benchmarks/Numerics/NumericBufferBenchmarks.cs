using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Numerics.Benchmarks;

[MemoryDiagnoser]
public class NumericBufferBenchmarks {

	private readonly NumericBuffer<int> _input1;
	private readonly NumericBuffer<int> _input2;
	private readonly NumericBuffer<int> _output;
	private readonly IBufferOperator _op;

	public NumericBufferBenchmarks() {
		_input1 = new NumericBuffer<int>( 1000, 1000, 0 );
		_input2 = new NumericBuffer<int>( 1000, 1000, 0 );
		_output = new NumericBuffer<int>( 1000, 1000, 0 );
		_op = new NumericBufferOperator();
	}

	[Benchmark]
	public void Perform_SingleInputSet() {
		_op.Perform(
			_input1,
			( int val ) => {
				return 1;
			},
			_output
		);
	}

	[Benchmark]
	public void Perform_SingleInputAdd() {
		_op.Perform(
			_input1,
			( int val ) => {
				return val + 1;
			},
			_output
		);
	}

	[Benchmark]
	public void Perform_TwoInputSet() {
		_op.Perform(
			_input1,
			_input2,
			( int a, int b ) => {
				return 1;
			},
			_output
		);
	}

	[Benchmark]
	public void Perform_TwoInputAdd() {
		_op.Perform(
			_input1,
			_input2,
			( int a, int b ) => {
				return a + b;
			},
			_output
		);
	}
}
