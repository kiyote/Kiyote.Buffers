using BenchmarkDotNet.Attributes;
using Kiyote.Geometry;

namespace Kiyote.Buffers.Benchmarks;

[MemoryDiagnoser]
public class BufferOperatorBenchmarks{

    private readonly FlatArrayBuffer<int> _input1;
    private readonly FlatArrayBuffer<int> _input2;
    private readonly FlatArrayBuffer<int> _output1;
    private readonly FlatArrayBuffer<float> _output2;
    private readonly IBufferOperator _op;

    public BufferOperatorBenchmarks()
    {
        _op = new BufferOperator();
        _input1 = new FlatArrayBuffer<int>(new Point(1000, 1000), 0);
        _input2 = new FlatArrayBuffer<int>(new Point(1000, 1000), 0);
        _output1 = new FlatArrayBuffer<int>(new Point(1000, 1000), 0);
        _output2 = new FlatArrayBuffer<float>(new Point(1000, 1000), 0.0f);
    }

    [Benchmark]
    public void Perform_OneInputBuffer()
    {
        _op.Perform(
            _input1,
            (int val) =>
            {
                return val;
            },
            _output1
        );
    }

    [Benchmark]
    public void Perform_TwoIputBuffers()
    {
        _op.Perform(
            _input1,
            _input2,
            (int a, int b) =>
            {
                return b;
            },
            _output1
        );
    }

    [Benchmark]
    public void Perform_OneInputSourceReference()
    {
        _op.Perform(
            _input1,
            (int a, int b, IBuffer<int> inp, int val) =>
            {
                return val;
            },
            _output1
        );
    }

    [Benchmark]
    public void Perform_OneInputTransformOutput()
    {
        _op.Perform<int, float>(
            _input1,
            (int a, int b, int val) =>
            {
                return (float)val;
            },
            _output2
        );
    }
}
