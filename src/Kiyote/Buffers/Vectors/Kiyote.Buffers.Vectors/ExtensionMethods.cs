using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Buffers.Vectors;

public static class ExtensionMethods {

	public static IServiceCollection AddVectorBuffers(
		this IServiceCollection services
	) {
		services.AddBuffers();
		//services.TryAddSingleton<IFloatBufferOperators, FloatBufferOperators>();
		return services;
	}

}
