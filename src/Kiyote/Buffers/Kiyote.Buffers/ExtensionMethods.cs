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

	/// <summary>Registers default buffer operators and the ragged factory unless already registered.
	/// If both buffer factories are added, the first registered factory wins.</summary>
	public static IServiceCollection AddRaggedBuffers(
		this IServiceCollection services
	) {
		services
			.AddBufferOperators()
			.TryAddSingleton<IBufferFactory, RaggedArrayBufferFactory>();

		return services;
	}

	/// <summary>Registers default buffer operators and the flat factory unless already registered.
	/// If both buffer factories are added, the first registered factory wins.</summary>
	public static IServiceCollection AddFlatBuffers(
		this IServiceCollection services
	) {
		services
			.AddBufferOperators()
			.TryAddSingleton<IBufferFactory, FlatArrayBufferFactory>();

		return services;
	}

}
