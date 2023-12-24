using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace psxpress.tests.MDEC;

public static class StreamExtensions
{
	public static Endianness Endianness { get; } = BitConverter.IsLittleEndian ? Endianness.LE : Endianness.BE;

	public static T Read<T>(this Stream stream, Endianness? endianness = null) where T : unmanaged
	{
		Span<byte> span = stackalloc byte[Unsafe.SizeOf<T>()];

		stream.ReadExactly(span);

		if ((endianness ?? Endianness) != Endianness)
		{
			span.Reverse();
		}

		var read = MemoryMarshal.Read<T>(span);

		return read;
	}
}