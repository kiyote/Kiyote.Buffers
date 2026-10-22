using Kiyote.Buffers.Analyzers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kiyote.Buffers.UnitTests;

[TestFixture]
public class DependencyInjectionUnitTests {

	[TestCase( true )]
	[TestCase( false )]
	public void AddBufferFactories_BothOrders_FirstFactoryWins( bool flatFirst ) {
		ServiceCollection services = new();
		if( flatFirst ) {
			services.AddFlatBuffers().AddRaggedBuffers();
		} else {
			services.AddRaggedBuffers().AddFlatBuffers();
		}

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<IBufferFactory>().Create( 2, 2, 0 ).GetType().Name,
			Is.EqualTo( flatFirst ? "FlatArrayBuffer`1" : "RaggedArrayBuffer`1" ) );
		Assert.That( provider.GetRequiredService<IBufferOperator>(), Is.Not.Null );
	}

	[Test]
	public void AddBufferOperators_WithoutExistingOperator_RegistersDefault() {
		ServiceCollection services = new();
		services.AddBufferOperators();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<IBufferOperator>().GetType().Name, Is.EqualTo( "BufferOperator" ) );
	}

	[Test]
	public void AddBufferFactories_WithExistingFactoryAndOperator_PreservesBoth() {
		ServiceCollection services = new();
		IBufferFactory factory = new SubstituteFactory();
		IBufferOperator operation = DispatchProxy.Create<IBufferOperator, SubstituteProxy>();
		services.AddSingleton( factory );
		services.AddSingleton( operation );
		services.AddFlatBuffers().AddRaggedBuffers();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<IBufferFactory>(), Is.SameAs( factory ) );
		Assert.That( provider.GetRequiredService<IBufferOperator>(), Is.SameAs( operation ) );
	}

	[Test]
	public void AddBufferAnalyzers_WithoutExistingOperator_RegistersDefaultOperatorAndAnalyzer() {
		ServiceCollection services = new();
		services.AddBufferAnalyzers();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<IBufferOperator>().GetType().Name, Is.EqualTo( "BufferOperator" ) );
		Assert.That( provider.GetRequiredService<IBufferAnalyzer>().GetType().Name, Is.EqualTo( "BufferAnalyzer" ) );
	}

	[Test]
	public void AddBufferAnalyzers_WithExistingOperatorAndAnalyzer_PreservesBoth() {
		ServiceCollection services = new();
		IBufferOperator operation = DispatchProxy.Create<IBufferOperator, SubstituteProxy>();
		IBufferAnalyzer analyzer = DispatchProxy.Create<IBufferAnalyzer, SubstituteProxy>();
		services.AddSingleton( operation );
		services.AddSingleton( analyzer );
		services.AddBufferAnalyzers();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<IBufferOperator>(), Is.SameAs( operation ) );
		Assert.That( provider.GetRequiredService<IBufferAnalyzer>(), Is.SameAs( analyzer ) );
	}

	private sealed class SubstituteFactory : IBufferFactory {
		public IBuffer<T> Create<T>( int columns, int rows, T initialValue ) {
			throw new NotSupportedException();
		}
	}

}
