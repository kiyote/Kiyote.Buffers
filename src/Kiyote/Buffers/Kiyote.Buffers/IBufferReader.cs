namespace Kiyote.Buffers;

public interface IBufferReader<T> {

	Task<IBuffer<T>> ReadAsync(
		Stream stream
	);
}
