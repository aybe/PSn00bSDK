// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

using System.Buffers.Binary;
using System.Collections;
using System.Runtime.InteropServices;

namespace psxpress.tests;

[TestClass]
public class UnitTest1
{
	private byte[] PsxMem = new byte[2 * 1024 * 1024];

	[TestMethod]
	public void TestMethod1()
	{
		return;
		try
		{
			NativeMethods.DecDCTReset(0);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	[TestMethod]
	public void TestMethod2()
	{
		var len = PsxMemory.Max - PsxMemory.Min;

		Console.WriteLine(len.ToString("N0"));

		var map = new PsxMemory();

		foreach (var range in map)
		{
			Console.WriteLine(range);
		}

		var hops = 100_000;

		for (var i = 0; i < hops; i++)
		{
			var address = (PsxMemory.Max - PsxMemory.Min) / hops * i;

			var range = map.FirstOrDefault(s => s.Contains(address));

			if (range == null)
			{
				continue;
			}

			Console.WriteLine($"0x{address:X8}, {address,16:N0}, {range.Name}");
		}

		// https://stackoverflow.com/questions/7747265/why-in-mips-architecture-program-space-divided-into-4-areas

		// Everything You Have Always Wanted to Know about the Playstation
		// https://fabiensanglard.net/doom_psx/psx.pdf
	}
}

public sealed partial class PsxMemory
{
	public const long Min = 0x00000000;

	public const long Max = 0xBFC7FFFF;

	public static PsxMemoryBlock Kernel { get; } =
		new(0x_0000_0000, 0x_0000_FFFF, nameof(Kernel));

	public static PsxMemoryBlock User { get; } =
		new(0x_0001_0000, 0x_001F_FFFF, nameof(User));

	public static PsxMemoryBlock ParallelPort { get; } =
		new(0x_1F00_0000, 0x1F00_FFFF, nameof(ParallelPort));

	public static PsxMemoryBlock ScratchPad { get; } =
		new(0x_1F80_0000, 0x_1F80_03FF, nameof(ScratchPad));

	public static PsxMemoryBlock HardwareRegisters { get; } =
		new(0x_1F80_1000, 0x_1F80_2FFF, nameof(HardwareRegisters));

	public static PsxMemoryBlock KernelUserMirrorCached { get; } =
		new(0x_8000_0000, 0x_801F_FFFF, nameof(KernelUserMirrorCached));

	public static PsxMemoryBlock KernelUserMirrorUncached { get; } =
		new(0x_A000_0000, 0x_A01F_FFFF, nameof(KernelUserMirrorUncached));

	public static PsxMemoryBlock Bios { get; } =
		new(0x_BFC0_0000, 0x_BFC7_FFFF, nameof(Bios));
}

public sealed partial class PsxMemory : IReadOnlyList<PsxMemoryBlock>
{
	private readonly IReadOnlyList<PsxMemoryBlock> Blocks;

	public PsxMemory() : this(new[]
	{
		Kernel,
		User,
		ParallelPort,
		ScratchPad,
		HardwareRegisters,
		KernelUserMirrorCached,
		KernelUserMirrorUncached,
		Bios
	})
	{
	}

	private PsxMemory(IEnumerable<PsxMemoryBlock> blocks)
	{
		Blocks = blocks.ToList();
	}

	public IEnumerator<PsxMemoryBlock> GetEnumerator()
	{
		return Blocks.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Blocks).GetEnumerator();
	}

	public int Count => Blocks.Count;

	public PsxMemoryBlock this[int index] => Blocks[index];
}

public sealed partial class PsxMemory : IPsxMemory
{
	public byte GetByte(int index)
	{
		var block = GetBlock(index);

		return block.GetByte(index);
	}

	public ushort GetWord(int index)
	{
		var block = GetBlock(index);

		return block.GetWord(index);
	}

	public void SetByte(int index, byte value)
	{
		var block = GetBlock(index);

		block.SetByte(index, value);
	}

	public void SetWord(int index, ushort value)
	{
		var block = GetBlock(index);

		block.SetWord(index, value);
	}

	private PsxMemoryBlock GetBlock(int index)
	{
		var block = this.Single(s => s.Contains(index));

		return block;
	}
}

public sealed class PsxMemoryBlock : IPsxMemory
{
	public PsxMemoryBlock(long start, long end, string name)
	{
		Start = start;

		End = end;

		Name = name;

		Length = end - start + 1;

		Bytes = new byte[Length];
	}

	public long Start { get; }

	public long End { get; }

	public string Name { get; }

	public long Length { get; }

	public byte[] Bytes { get; }

	public byte GetByte(int index)
	{
		return Bytes[index];
	}

	public ushort GetWord(int index)
	{
		return BinaryPrimitives.ReadUInt16LittleEndian(Bytes.AsSpan(index));
	}

	public void SetByte(int index, byte value)
	{
		Bytes[index] = value;
	}

	public void SetWord(int index, ushort value)
	{
		BinaryPrimitives.WriteUInt16LittleEndian(Bytes.AsSpan(index), value);
	}

	public override string ToString()
	{
		return $"0x{Start:X8} | 0x{End:X8}, {Length}, {Name}";
	}

	public bool Contains(long address)
	{
		return address >= Start && address <= End;
	}
}

public interface IPsxMemory
{
	byte GetByte(int index);

	ushort GetWord(int index);

	void SetByte(int index, byte value);

	void SetWord(int index, ushort value);
}

internal static class NativeMethods
{
	private const string DllName = @"C:\GitHub\PSn00bSDK\libpsn00b\psxpress\Debug\psxpress.exe";

	private const CallingConvention DllConv = CallingConvention.Cdecl;

	[DllImport(DllName, CallingConvention = DllConv)]
	public static extern void DecDCTReset(int mode);
}

