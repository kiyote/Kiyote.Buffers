
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kiyote.Buffers.Benchmarks;
using Kiyote.Buffers.Numerics.Benchmarks;

ManualConfig config = DefaultConfig.Instance
    .AddJob(Job
         .MediumRun
         .WithLaunchCount(1)
         .WithToolchain(InProcessNoEmitToolchain.Instance));

BenchmarkSwitcher
    .FromTypes( [
        typeof( ArrayBufferBenchmarks ),
        typeof( BufferOperatorBenchmarks ),
        typeof( NumericBufferOperatorBenchmarks )
    ] )
    .RunAll(config, args);

