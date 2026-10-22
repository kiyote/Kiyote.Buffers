using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kiyote.Buffers.Numerics.IntegrationTests;

[TestFixture]
public class NumericDependencyInjectionTests {

	[TestCase( true )]
	[TestCase( false )]
	public void AddNumericFactories_BothOrders_FirstFactoryWins( bool flatFirst ) {
		ServiceCollection services = new();
		if( flatFirst ) {
			services.AddFlatNumericBuffers().AddRaggedNumericBuffers();
		} else {
			services.AddRaggedNumericBuffers().AddFlatNumericBuffers();
		}

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<INumericBufferFactory>().Create( 2, 2, 0 ).GetType().Name,
			Is.EqualTo( flatFirst ? "FlatNumericBuffer`1" : "RaggedNumericBuffer`1" ) );
		Assert.That( provider.GetRequiredService<INumericBufferOperator>(), Is.Not.Null );
	}

	[Test]
	public void AddNumericBuffers_WithoutExistingOperator_RegistersDefaultOnly() {
		ServiceCollection services = new();
		services.AddNumericBuffers();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<INumericBufferOperator>().GetType().Name, Is.EqualTo( "NumericBufferOperator" ) );
		Assert.That( provider.GetService<INumericBufferFactory>(), Is.Null );
	}

	[Test]
	public void AddNumericFactories_WithExistingFactoryAndOperator_PreservesBoth() {
		ServiceCollection services = new();
		INumericBufferFactory factory = DispatchProxy.Create<INumericBufferFactory, SubstituteProxy>();
		INumericBufferOperator operation = DispatchProxy.Create<INumericBufferOperator, SubstituteProxy>();
		services.AddSingleton( factory );
		services.AddSingleton( operation );
		services.AddRaggedNumericBuffers().AddFlatNumericBuffers();

		using ServiceProvider provider = services.BuildServiceProvider();
		Assert.That( provider.GetRequiredService<INumericBufferFactory>(), Is.SameAs( factory ) );
		Assert.That( provider.GetRequiredService<INumericBufferOperator>(), Is.SameAs( operation ) );
	}

}
