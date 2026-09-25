using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers;

public static class ExtensionMethods {

	public static IServiceCollection AddBuffers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<IBufferOperator, BufferOperator>();
		services.TryAddSingleton<IBufferAnalyzer, BufferAnalyzer>();

		return services;
	}

	public static IServiceCollection AddRaggedBuffers(
		this IServiceCollection services
	) {
		services
			.AddBuffers()
			.TryAddSingleton<IBufferFactory, RaggedArrayBufferFactory>();

		return services;
	}

	public static IServiceCollection AddFlatBuffers(
		this IServiceCollection services
	) {
		services
			.AddBuffers()
			.TryAddSingleton<IBufferFactory, FlatArrayBufferFactory>();

		return services;
	}

}
