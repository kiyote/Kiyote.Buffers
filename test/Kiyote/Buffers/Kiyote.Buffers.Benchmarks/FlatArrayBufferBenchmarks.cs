using BenchmarkDotNet.Attributes;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class FlatArrayBufferBenchmarks
{

    private readonly FlatArrayBuffer<int> _input1;
    private readonly FlatArrayBuffer<int> _input2;
    private readonly FlatArrayBuffer<int> _output;
    private readonly IBufferOperator _op;

    public FlatArrayBufferBenchmarks()
    {
        _input1 = new FlatArrayBuffer<int>(new Point(1000, 1000));
        _input2 = new FlatArrayBuffer<int>(new Point(1000, 1000));
        _output = new FlatArrayBuffer<int>(new Point(1000, 1000));
        _op = new BufferOperator();
    }

    [Benchmark]
    public void Perform_SingleInputSet()
    {
        _op.Perform(
            _input1,
            (int val) =>
            {
                return 1;
            },
            _output
        );
    }

    [Benchmark]
    public void Perform_SingleInputAdd()
    {
        _op.Perform(
            _input1,
            (int val) =>
            {
                return val + 1;
            },
            _output
        );
    }

    [Benchmark]
    public void Perform_TwoInputSet()
    {
        _op.Perform(
            _input1,
            _input2,
            (int a, int b) =>
            {
                return 1;
            },
            _output
        );
    }

    [Benchmark]
    public void Perform_TwoInputAdd()
    {
        _op.Perform(
            _input1,
            _input2,
            (int a, int b) =>
            {
                return a + b;
            },
            _output
        );
    }
}
