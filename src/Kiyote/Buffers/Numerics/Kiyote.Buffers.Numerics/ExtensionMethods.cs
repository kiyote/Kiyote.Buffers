using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Numerics;

public static class ExtensionMethods {

	public static IServiceCollection AddNumericBuffers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<INumericBufferFactory, NumericBufferFactory>();
		services.TryAddSingleton<INumericBufferOperator, NumericBufferOperator>();

		return services;
	}
}
