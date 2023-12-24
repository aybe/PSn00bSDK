// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

using System.Globalization;
using System.Numerics;

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

	private void Decode(KeyValuePair<uint, FrameData> frameData)
	{
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

			break;
		}
	}

	private static string ToBinaryString<T>(T value) where T : IBinaryInteger<T>
	{
		var s = value.ToString($"B{value.GetByteCount() * 8}", CultureInfo.InvariantCulture);
		var chunk = s.Chunk(8);
		s = string.Join("_", chunk.Select(chars => new string(chars)));
		return s;
		return s;
	}
}