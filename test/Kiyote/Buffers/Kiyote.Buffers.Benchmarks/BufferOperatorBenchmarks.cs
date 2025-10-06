using BenchmarkDotNet.Attributes;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class BufferOperatorBenchmarks {

	private readonly ArrayBuffer<int> _input1;
	private readonly ArrayBuffer<int> _input2;
	private readonly ArrayBuffer<int> _output1;
	private readonly ArrayBuffer<float> _output2;
	private readonly IBufferOperator _op;

	public BufferOperatorBenchmarks() {
		_op = new BufferOperator();
		_input1 = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
		_input2 = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
		_output1 = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
		_output2 = new ArrayBuffer<float>( new Point( 1000, 1000 ), 0.0f );
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
