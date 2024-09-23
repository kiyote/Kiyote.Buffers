namespace Kiyote.Buffers;

public interface IBufferWriter<T> {

	Task WriteAsync(
		IBuffer<T> buffer
	);

}

