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

	/// <summary>Registers the numeric operator and ragged factory unless already registered.
	/// If both numeric factories are added, the first registered factory wins.</summary>
	public static IServiceCollection AddRaggedNumericBuffers(
		this IServiceCollection services
	) {
		services
			.AddNumericBuffers()
			.TryAddSingleton<INumericBufferFactory, RaggedNumericBufferFactory>();

		return services;
	}

	/// <summary>Registers the numeric operator and flat factory unless already registered.
	/// If both numeric factories are added, the first registered factory wins.</summary>
	public static IServiceCollection AddFlatNumericBuffers(
		this IServiceCollection services
	) {
		services
			.AddNumericBuffers()
			.TryAddSingleton<INumericBufferFactory, FlatNumericBufferFactory>();

		return services;
	}
}
