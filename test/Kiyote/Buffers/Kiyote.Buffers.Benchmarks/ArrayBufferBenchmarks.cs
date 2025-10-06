using BenchmarkDotNet.Attributes;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class ArrayBufferBenchmarks {

	private readonly ArrayBuffer<int> _input1;
	private readonly ArrayBuffer<int> _input2;
	private readonly ArrayBuffer<int> _output;
	private readonly IBufferOperator _op;

	public ArrayBufferBenchmarks() {
		_input1 = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
		_input2 = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
		_output = new ArrayBuffer<int>( new Point( 1000, 1000 ), 0 );
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
