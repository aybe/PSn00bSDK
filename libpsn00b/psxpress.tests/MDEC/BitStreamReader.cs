using System.Numerics;
using System.Runtime.CompilerServices;

namespace psxpress.tests.MDEC;

public sealed class BitStreamReader(Stream stream, BitStreamFormat format, bool leaveOpen = false) : IDisposable
{
	private readonly Queue<bool> Queue = new();

	public void Dispose()
	{
		if (leaveOpen)
		{
			return;
		}

		stream.Dispose();
	}

	private void Update()
	{
		if (Queue.Count != 0)
		{
			return;
		}

		if (stream.Length - stream.Position < format.ByteSize)
		{
			throw new EndOfStreamException();
		}

		Span<byte> buffer = stackalloc byte[format.ByteSize];

		stream.ReadExactly(buffer);

		if (format.ByteSwap)
		{
			buffer.Reverse();
		}

		foreach (var b in buffer)
		{
			for (var i = 0; i < 8; i++)
			{
				Queue.Enqueue((b & (1 << (format.Reversed ? 7 - i : i))) != 0);
			}
		}
	}

	public bool Read()
	{
		Update();

		var bit = Queue.Dequeue();

		return bit;
	}

	public T Read<T>(int bits) where T : IBinaryInteger<T>, IMinMaxValue<T>
	{
		var size = Unsafe.SizeOf<T>() * 8;

		ArgumentOutOfRangeException.ThrowIfLessThan(bits, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, size);

		var data = T.Zero;

		for (var i = 0; i < bits; i++)
		{
			data |= (Read() ? T.One : T.Zero) << i;
		}

		if (T.MinValue >= T.Zero)
		{
			return data;
		}

		for (var i = bits; i < size; i++)
		{
			data |= T.One << i;
		}

		return data;
	}
}