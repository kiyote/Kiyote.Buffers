using System.Numerics;

namespace Kiyote.Buffers.Numerics;

public interface INumericBuffer<T> : IBuffer<T> where T : struct, INumber<T> {
}
