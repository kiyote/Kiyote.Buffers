
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kiyote.Buffers.Analyzers.Benchmarks;
using Kiyote.Buffers.Benchmarks;
using Kiyote.Buffers.Benchmarks.ChunkedGrids;
using Kiyote.Buffers.Numerics.Benchmarks;

ManualConfig config = DefaultConfig.Instance
	.AddJob( Job
		 .MediumRun
		 .WithLaunchCount( 1 )
		 .WithToolchain( InProcessNoEmitToolchain.Instance ) );

BenchmarkSwitcher
	.FromTypes( [
		typeof( RaggedArrayBufferBenchmarks ),
		typeof( FlatArrayBufferBenchmarks ),
		typeof( BufferAccessBenchmarks ),
		typeof( BufferOperatorBenchmarks ),
		typeof( BufferAnalyzerBenchmarks ),
		typeof( RaggedNumericBufferBenchmarks ),
		typeof( FlatNumericBufferBenchmarks ),
		typeof( NumericBufferOperatorBenchmarks ),
		typeof( NumericLayoutBenchmarks ),
		typeof( ChunkedGridBenchmarks ),
		typeof( DoubleBufferedChunkedGridBenchmarks ),
		typeof( TripleBufferedChunkedGridBenchmarks ),
		typeof( ChunkedGridLayerBenchmarks ),
	] )
	.Run( args, config );

