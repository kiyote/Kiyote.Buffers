using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers;

public static class ExtensionMethods {

	public static IServiceCollection AddBufferOperators(
		this IServiceCollection services
	) {
		services.TryAddSingleton<IBufferOperator, BufferOperator>();

		return services;
	}

	public static IServiceCollection AddRaggedBuffers(
		this IServiceCollection services
	) {
		services
			.AddBufferOperators()
			.TryAddSingleton<IBufferFactory, RaggedArrayBufferFactory>();

		return services;
	}

	public static IServiceCollection AddFlatBuffers(
		this IServiceCollection services
	) {
		services
			.AddBufferOperators()
			.TryAddSingleton<IBufferFactory, FlatArrayBufferFactory>();

		return services;
	}

}
