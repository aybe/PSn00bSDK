// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace psxpress.tests;

[TestClass]
public sealed class UnitTest4 : UnitTestBase
{
	[TestMethod]
	public void TestMethod1()
	{
		const string path = @"D:\Temp\Wipeout 1 intro movie\WipEout (Europe) (v1.1).AV.2352";

		using var stream = File.OpenRead(path);

		const int sectorLength = 2352;

		var sectorCount = stream.Length / sectorLength;

		var printSectorHeader = false;
		var printSectorSubHeader = false;
		var printSectorMdecHeader = false;
		var printSectorMdecSubHeader = false;

		var frames = new Dictionary<uint, FrameData>();

		for (var i = 0; i < sectorCount; i++)
		{
			stream.Position = i * sectorLength;

			stream.Position += 12; // sync

			var header = stream.Read<SectorHeader>();

			if (printSectorHeader)
			{
				WriteLine($"{i:D5}, {header}");
			}

			var subHeader1 = stream.Read<SectorSubHeader>();
			var subHeader2 = stream.Read<SectorSubHeader>();

			if (printSectorSubHeader)
			{
				WriteLine(subHeader1);
			}

			Assert.AreEqual(subHeader1, subHeader2);

			if (!subHeader1.SubMode.HasFlag(SectorSubMode.Video))
			{
				continue;
			}

			Assert.AreEqual(128, subHeader1.CodingInformation);

			var mdecHeader = stream.Read<MdecHeader>();

			if (printSectorMdecHeader)
			{
				WriteLine(mdecHeader);
			}

			var mdecSubHeader = stream.Read<MdecSubHeader>();

			if (printSectorMdecSubHeader)
			{
				WriteLine(mdecSubHeader);
			}

			if (!frames.TryGetValue(mdecHeader.FrameNumber, out var data))
			{
				var frameData = new FrameData();
				frameData.SectorHeader = header;
				frameData.SectorSubHeader = subHeader1;
				frameData.MdecHeader = mdecHeader;
				frameData.MdecSubHeader = mdecSubHeader;
				frameData.Data = new byte[mdecHeader.FrameSize];
				frames.Add(mdecHeader.FrameNumber, data = frameData);
			}

			var pos = Math.Min(mdecHeader.FrameSize, mdecHeader.SectorOffset * 2016);
			var len = Math.Min(2016, mdecHeader.FrameSize - pos);

			if (len > 0)
			{
				stream.ReadExactly(data.Data, (int)pos, (int)len);
			}
			else
			{
				Assert.AreEqual((int)mdecHeader.FrameSize, data.Data.Length);
			}
		}

		Decode(frames);
	}

	private void Decode(Dictionary<uint, FrameData> frames)
	{
		foreach (var frameData in frames)
		{
			Decode(frameData);
		}
	}

	private void Decode(KeyValuePair<uint, FrameData> frameData)
	{
		
	}
}

public class FrameData
{
	public byte[] Data;
	public MdecHeader MdecHeader;
	public MdecSubHeader MdecSubHeader;
	public SectorHeader SectorHeader;
	public SectorSubHeader SectorSubHeader;
}

public readonly struct MdecHeader
{
	public readonly ushort Status;

	public readonly ushort Type;

	public readonly ushort SectorOffset;

	public readonly ushort SectorSize;

	public readonly uint FrameNumber;

	public readonly uint FrameSize;

	public override string ToString()
	{
		return
			$"{nameof(Status)}: 0x{Status:X4}, " +
			$"{nameof(Type)}: 0x{Type:X4}, " +
			$"{nameof(SectorOffset)}: {SectorOffset}, " +
			$"{nameof(SectorSize)}: {SectorSize}, " +
			$"{nameof(FrameNumber)}: {FrameNumber}, " +
			$"{nameof(FrameSize)}: {FrameSize}";
	}
}

public readonly struct MdecSubHeader
{
	public readonly ushort MovieWidth;

	public readonly ushort MovieHeight;

	public readonly uint HeadM;

	public readonly uint HeadV;

	public readonly uint Unspecified;

	public override string ToString()
	{
		return
			$"{nameof(MovieWidth)}: {MovieWidth}, " +
			$"{nameof(MovieHeight)}: {MovieHeight}, " +
			$"{nameof(HeadM)}: 0x{HeadM:X8}, " +
			$"{nameof(HeadV)}: 0x{HeadV:X8}, " +
			$"{nameof(Unspecified)}: 0x{Unspecified:X8}";
	}
}

public readonly struct SectorHeader
{
	public readonly SectorAddress Address;

	public readonly SectorMode Mode;

	public override string ToString()
	{
		return $"{nameof(Address)}: {Address}, {nameof(Mode)}: {Mode}";
	}
}

[Flags]
public enum SectorSubMode : byte
{
	None = 0,
	EndOfRecord = 1 << 0,
	Video = 1 << 1,
	Audio = 1 << 2,
	Data = 1 << 3,
	Trigger = 1 << 4,
	Form = 1 << 5,
	RealTimeSector = 1 << 6,
	EndOfFile = 1 << 7
}

public readonly struct SectorSubHeader
{
	public readonly byte FileNumber;

	public readonly byte ChannelNumber;

	public readonly SectorSubMode SubMode;

	public readonly byte CodingInformation;

	public override string ToString()
	{
		return $"{nameof(FileNumber)}: {FileNumber}, {nameof(ChannelNumber)}: {ChannelNumber}, {nameof(SubMode)}: {SubMode}, {nameof(CodingInformation)}: {CodingInformation}";
	}
}

public enum SectorMode : byte
{
	Mode0 = 0x0,
	Mode1 = 0x1,
	Mode2 = 0x2
}

public readonly struct SectorAddress
{
	public readonly byte M, S, F;

	public override string ToString()
	{
		return $"{M:X2}:{S:X2}.{F:X2}";
	}
}

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

public enum Endianness
{
	BE,
	LE
}