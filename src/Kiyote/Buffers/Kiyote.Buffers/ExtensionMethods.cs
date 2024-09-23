using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers;

public static class ExtensionMethods {

	public static IServiceCollection AddBuffers(
		this IServiceCollection services
	) {
		services.TryAddSingleton<IBufferOperator, BufferOperator>();
		services.TryAddSingleton<IBufferFactory, ArrayBufferFactory>();

		return services;
	}
}
