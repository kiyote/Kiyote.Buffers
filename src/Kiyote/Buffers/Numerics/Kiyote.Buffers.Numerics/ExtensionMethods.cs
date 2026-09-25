using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Numerics;

public static class ExtensionMethods {

	public static IServiceCollection AddNumericBuffers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<INumericBufferOperator, NumericBufferOperator>();

		return services;
	}

	public static IServiceCollection AddRaggedNumericBuffes(
		this IServiceCollection services
	) {
		services
			.AddNumericBuffers()
			.TryAddSingleton<INumericBufferFactory, RaggedNumericBufferFactory>();

		return services;
	}

	public static IServiceCollection AddFlatNumericBuffes(
		this IServiceCollection services
	) {
		services
			.AddNumericBuffers()
			.TryAddSingleton<INumericBufferFactory, FlatNumericBufferFactory>();

		return services;
	}
}
