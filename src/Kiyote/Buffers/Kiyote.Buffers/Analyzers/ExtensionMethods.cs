
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Analyzers;

public static class ExtensionMethods {

	/// <summary>Registers the default analyzer and its convenient default buffer operator,
	/// preserving any application-provided registrations for either service.</summary>
	public static IServiceCollection AddBufferAnalyzers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<IBufferOperator, BufferOperator>();
		services.TryAddSingleton<IBufferAnalyzer, BufferAnalyzer>();

		return services;
	}

}
