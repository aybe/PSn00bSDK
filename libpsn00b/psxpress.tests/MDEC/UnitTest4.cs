// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace psxpress.tests.MDEC;

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

		var decodeFirstFrameOnly = true;

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
				break;
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

			if (decodeFirstFrameOnly && mdecHeader.FrameNumber > 1)
			{
				break;
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

	private void Test(byte[] data)
	{
		using var stream = new MemoryStream(data);
		using var reader = new BitStreamReader(stream, BitStreamFormat.ShortLittleEndianMostSignificant);

		var dc = reader.Read<short>(10);

		WriteLine($"{nameof(dc)}: {dc}");

		var codes = new[]
		{
			new Code(02, 0b0000000000_10, 00, 0, 0, 1),
			new Code(02, 0b0000000000_11, 00, 1, 0, 0),
			new Code(03, 0b000000000_011, 00, 1, 0, 0),
			new Code(03, 0b000000000_010, 01, 1, 0, 0),
			new Code(04, 0b00000000_0011, 01, 1, 0, 0),
			new Code(05, 0b0000000_00101, 00, 1, 0, 0),
			new Code(05, 0b0000000_00100, 03, 1, 0, 0),
			new Code(04, 0b00000000_0001, 02, 1, 0, 0),
			new Code(05, 0b0000000_00001, 02, 1, 0, 0),
			new Code(06, 0b000000_000001, 16, 0, 1, 0),
			new Code(07, 0b00000_0000001, 03, 1, 0, 0),
			new Code(08, 0b0000_00000001, 04, 1, 0, 0),
			new Code(09, 0b000_000000001, 04, 1, 0, 0),
			new Code(10, 0b00_0000000001, 04, 1, 0, 0),
			new Code(11, 0b0_00000000001, 04, 1, 0, 0),
			new Code(12, 0b_000000000001, 04, 1, 0, 0)
		};

		var i = 0;

		var n = 0;

		while (stream.Position < stream.Length)
		{
			foreach (var code in codes)
			{
				if (reader.Peek<ushort>(code.Len) != code.Sig)
				{
					continue;
				}

				reader.Read<ushort>(code.Len);

				WriteLine($"{i:D6} | {code}");

				i++;

				//WriteLine($"end {code.End}, esc {code.Esc}, sig {code.Sig.ToString($"B{code.Len}")} ");

				if (code.End != 0)
				{
					break;
				}

				if (code.Esc != 0)
				{
					break;
				}

				var val = reader.Read<ushort>(code.Val);
				var sgn = reader.Read<ushort>(code.Sgn);
				//WriteLine($"\t{val}, {sgn}");
				break;
			}

			n++;

			if (n != i)
			{
				break;
			}
		}
	}


	private void Decode(KeyValuePair<uint, FrameData> frameData)
	{
		Test(frameData.Value.Data.AsSpan(8).ToArray());
		return;

		WriteLine($"0x{0xFE20:X4}, {ToBinaryString((ushort)0xFE20)}");
		WriteLine($"0x{0x2291:X4}, {ToBinaryString((ushort)0x2291)}");
		WriteLine($"0x{0xF1F4:X4}, {ToBinaryString((ushort)0xF1F4)}");
		WriteLine($"0x{0x8948:X4}, {ToBinaryString((ushort)0x8948)}");
		WriteLine($"0x{0xBB35:X4}, {ToBinaryString((ushort)0xBB35)}");
		WriteLine($"0x{0x210C:X4}, {ToBinaryString((ushort)0x210C)}");

		var bytes = new byte[] { 0xFE, 0x20, 0x22, 0x91, 0xF1, 0xF4, 0x89, 0x48, 0xBB, 0x35, 0x21, 0x0C };
		foreach (var b in bytes)
		{
			WriteLine($"0x{b:X2} {ToBinaryString(b)}");
		}

		using var stream = new MemoryStream(frameData.Value.Data);
		using var reader = new BitStreamReader(stream, BitStreamFormat.ShortLittleEndianMostSignificant);

		var mdecSizeBy4 = stream.Read<ushort>();
		var mdecFileId = stream.Read<ushort>();
		var quantStep = stream.Read<ushort>();
		var version = stream.Read<ushort>();

		WriteLine($"{nameof(mdecSizeBy4)}: {mdecSizeBy4}");
		WriteLine($"{nameof(mdecFileId)}: 0x{mdecFileId:X4}");
		WriteLine($"{nameof(quantStep)}: {quantStep}");
		WriteLine($"{nameof(version)}: {version}");

		Assert.AreEqual(0x08C0, mdecSizeBy4);
		Assert.AreEqual(0x3800, mdecFileId);
		Assert.AreEqual(0x0001, quantStep);
		Assert.AreEqual(0x0001, version);

		WriteLine();

		WriteLine($"0x{(ushort)0x20FE:X4} = {ToBinaryString((ushort)0x20FE)}");
		WriteLine($"0x{(ushort)0xFE20:X4} = {ToBinaryString((ushort)0xFE20)}");

		while (stream.Position < stream.Length)
		{
			var dc = reader.Read<short>(10);
			var eb = reader.Read<ushort>(2);
			Assert.AreEqual(2, eb);

			if (reader.Peek<ushort>(2) == 0b_10)
			{
				reader.Read<ushort>(2);
				break; // EOB
			}

			if (reader.Peek<ushort>(2) == 0b_11)
			{
				reader.Read<ushort>(2);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(3) == 0b_011)
			{
				reader.Read<ushort>(3);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(3) == 0b_010)
			{
				reader.Read<ushort>(3);
				var x = reader.Read<ushort>(1);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(4) == 0b_0011)
			{
				reader.Read<ushort>(4);
				var x = reader.Read<ushort>(1);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(5) == 0b_0_0101)
			{
				reader.Read<ushort>(5);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(5) == 0b_0_0100)
			{
				reader.Read<ushort>(5);
				var x = reader.Read<ushort>(3);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(4) == 0b_0001)
			{
				reader.Read<ushort>(4);
				var x = reader.Read<ushort>(2);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(5) == 0b_0_0001)
			{
				reader.Read<ushort>(5);
				var x = reader.Read<ushort>(2);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(6) == 0b_00_0001)
			{
				reader.Read<ushort>(6);
				var x = reader.Read<ushort>(10);
				break; // escape
			}

			if (reader.Peek<ushort>(7) == 0b_000_0001)
			{
				reader.Read<ushort>(7);
				var x = reader.Read<ushort>(3);
				var s = reader.Read<ushort>(1);
				WriteLine(x);
				WriteLine(s);
				break;
			}


			if (reader.Peek<ushort>(8) == 0b_0000_0001)
			{
				reader.Read<ushort>(8);
				var x = reader.Read<ushort>(4);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(9) == 0b_0_0000_0001)
			{
				reader.Read<ushort>(9);
				var x = reader.Read<ushort>(4);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(10) == 0b_00_0000_0001)
			{
				reader.Read<ushort>(10);
				var x = reader.Read<ushort>(4);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(11) == 0b_000_0000_0001)
			{
				reader.Read<ushort>(11);
				var x = reader.Read<ushort>(4);
				var s = reader.Read<ushort>(1);
				break;
			}

			if (reader.Peek<ushort>(12) == 0b_0000_0000_0001)
			{
				reader.Read<ushort>(12);
				var x = reader.Read<ushort>(4);
				var s = reader.Read<ushort>(1);
			}

			break;
		}
	}

	private static string ToBinaryString<T>(T value) where T : IBinaryInteger<T>
	{
		var s = value.ToString($"B{value.GetByteCount() * 8}", CultureInfo.InvariantCulture);
		var chunk = s.Chunk(4);
		s = string.Join("_", chunk.Select(chars => new string(chars)));
		return s;
		return s;
	}

	private readonly record struct Code(int Len, int Sig, int Val, int Sgn, int Esc, int End)
	{
		public override string ToString()
		{
			return $"{nameof(Val)}: {Val}, " +
			       $"{nameof(Sgn)}: {Sgn}, " +
			       $"{nameof(Esc)}: {Esc}, " +
			       $"{nameof(End)}: {End}, " +
			       $"{nameof(Len)}: {Len}, " +
			       $"{nameof(Sig)}: {Sig.ToString($"B{Len}")}";
		}
	}
}