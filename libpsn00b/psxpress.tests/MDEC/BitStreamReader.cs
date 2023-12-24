using System.Numerics;
using System.Runtime.CompilerServices;

namespace psxpress.tests.MDEC;

public sealed class BitStreamReader(Stream stream, BitStreamFormat format, bool leaveOpen = false) : IDisposable
{
	private readonly Queue<bool> Queue = new();

	private bool DebugQueue { get; } = false;

	private bool DebugRead { get; } = false;

	private bool FixCrap { get; } = true;

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

		var reversed = format.Reversed;

		if (FixCrap)
		{
			reversed = true; // BUG we shouldn't need this?
			buffer.Reverse(); // BUG we shouldn't need this? 
		}

		foreach (var b in buffer)
		{
			for (var i = 0; i < 8; i++)
			{
				Queue.Enqueue((b & (1 << (reversed ? 7 - i : i))) != 0);
			}
		}

		if (!DebugRead)
		{
			return;
		}

		foreach (var b in Queue)
		{
			Console.Write(Convert.ToInt16(b));
		}

		Console.WriteLine();
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
			var read = Read();
			if (DebugQueue)
			{
				Console.WriteLine(read ? 1 : 0);
			}

			data |= (read ? T.One : T.Zero) << (format.Reversed ? bits - i - 1 : i);
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