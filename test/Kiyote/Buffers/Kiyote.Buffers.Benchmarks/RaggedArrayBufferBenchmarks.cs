using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class RaggedArrayBufferBenchmarks {

	private readonly RaggedArrayBuffer<int> _input1;
	private readonly RaggedArrayBuffer<int> _input2;
	private readonly RaggedArrayBuffer<int> _output;
	private readonly IBufferOperator _op;

	public RaggedArrayBufferBenchmarks() {
		_input1 = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_input2 = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_output = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_op = new BufferOperator();
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
