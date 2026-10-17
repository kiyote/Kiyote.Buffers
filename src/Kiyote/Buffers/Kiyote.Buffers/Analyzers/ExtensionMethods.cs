
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Analyzers;

public static class ExtensionMethods {

	public static IServiceCollection AddBufferAnalyzers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<IBufferOperator, BufferOperator>();
		services.TryAddSingleton<IBufferAnalyzer, BufferAnalyzer>();

		return services;
	}

}
