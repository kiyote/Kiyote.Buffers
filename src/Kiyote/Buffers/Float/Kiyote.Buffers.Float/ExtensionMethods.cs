using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Float;

public static class ExtensionMethods {

	public static IServiceCollection AddFloatBuffers(
		this IServiceCollection services
	) {
		services.AddBuffers();
		services.TryAddSingleton<IFloatBufferOperators, FloatBufferOperators>();
		return services;
	}

}
