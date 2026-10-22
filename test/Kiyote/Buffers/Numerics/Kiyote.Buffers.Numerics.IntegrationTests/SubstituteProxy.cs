using System.Reflection;

namespace Kiyote.Buffers.Numerics.IntegrationTests;

public class SubstituteProxy : DispatchProxy {
	protected override object? Invoke( MethodInfo? targetMethod, object?[]? args ) {
		throw new NotSupportedException();
	}
}
