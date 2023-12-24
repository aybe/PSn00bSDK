using psxpress.tests.MDEC;

namespace psxpress.tests;

[TestClass]
public class UnitTest2 : UnitTestBase
{
	private BitStreamReader GetReader(BitStreamFormat format)
	{
		var buffer = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

		foreach (var b in buffer)
		{
			WriteLine($"{b:X2} {b:B8}");
		}

		return new BitStreamReader(new MemoryStream(buffer), format);
	}

	[TestMethod]
	public void TestByteLeastSignificant()
	{
		NewMethod(BitStreamFormat.ByteLeastSignificant);
	}

	[TestMethod]
	public void TestByteMostSignificant()
	{
		NewMethod(BitStreamFormat.ByteMostSignificant);
	}

	[TestMethod]
	public void TestShortBigEndianLeastSignificant()
	{
		NewMethod(BitStreamFormat.ShortBigEndianLeastSignificant);
	}

	[TestMethod]
	public void TestShortBigEndianMostSignificant()
	{
		NewMethod(BitStreamFormat.ShortBigEndianMostSignificant);
	}

	[TestMethod]
	public void TestShortLittleEndianLeastSignificant()
	{
		NewMethod(BitStreamFormat.ShortLittleEndianLeastSignificant);
	}

	[TestMethod]
	public void TestShortLittleEndianMostSignificant()
	{
		NewMethod(BitStreamFormat.ShortLittleEndianMostSignificant);
	}

	private void NewMethod(BitStreamFormat format)
	{
		using var reader = GetReader(format);

		WriteLine(format.GetType().Name);

		WriteLine($"\t{nameof(format.ByteSize)}: {format.ByteSize}");
		WriteLine($"\t{nameof(format.ByteSwap)}: {format.ByteSwap}");
		WriteLine($"\t{nameof(format.Reversed)}: {format.Reversed}");

		for (var i = 0; i < 8; i++)
		{
			WriteLine(reader.Read<byte>(8).ToString("B8"));
		}
	}

	[TestMethod]
	public void TestMethod1()
	{
		using var reader = GetReader(BitStreamFormat.ShortLittleEndianMostSignificant);

		WriteLine(reader.Read<ushort>(8).ToString("B16"));
		WriteLine(reader.Read<ushort>(8).ToString("B16"));
	}
}