using BenchmarkDotNet.Attributes;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class BufferOperatorBenchmarks {

	private readonly RaggedArrayBuffer<int> _input1;
	private readonly RaggedArrayBuffer<int> _input2;
	private readonly RaggedArrayBuffer<int> _output1;
	private readonly RaggedArrayBuffer<float> _output2;
	private readonly IBufferOperator _op;

	public BufferOperatorBenchmarks() {
		_op = new BufferOperator();
		_input1 = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_input2 = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_output1 = new RaggedArrayBuffer<int>( 1000, 1000, 0 );
		_output2 = new RaggedArrayBuffer<float>( 1000, 1000, 0.0f );
	}

	[Benchmark]
	public void Perform_OneInputBuffer() {
		_op.Perform(
			_input1,
			( int val ) => {
				return val;
			},
			_output1
		);
	}

	[Benchmark]
	public void Perform_TwoIputBuffers() {
		_op.Perform(
			_input1,
			_input2,
			( int a, int b ) => {
				return b;
			},
			_output1
		);
	}

	[Benchmark]
	public void Perform_OneInputSourceReference() {
		_op.Perform(
			_input1,
			( int a, int b, IBuffer<int> inp, int val ) => {
				return val;
			},
			_output1
		);
	}

	[Benchmark]
	public void Perform_OneInputTransformOutput() {
		_op.Perform<int, float>(
			_input1,
			( int a, int b, int val ) => {
				return (float)val;
			},
			_output2
		);
	}
}
