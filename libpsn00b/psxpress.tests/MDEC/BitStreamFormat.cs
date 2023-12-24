namespace psxpress.tests.MDEC;

public readonly struct BitStreamFormat
{
	public BitStreamFormat(int byteSize, bool byteSwap, bool reversed)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteSize);
		ByteSize = byteSize;
		ByteSwap = byteSwap;
		Reversed = reversed;
	}

	public static BitStreamFormat ByteLeastSignificant { get; } =
		new(1, false, false);

	public static BitStreamFormat ByteMostSignificant { get; } =
		new(1, false, true);

	public static BitStreamFormat ShortBigEndianLeastSignificant { get; } =
		new(2, BitConverter.IsLittleEndian is false, false);

	public static BitStreamFormat ShortBigEndianMostSignificant { get; } =
		new(2, BitConverter.IsLittleEndian is false, true);

	public static BitStreamFormat ShortLittleEndianLeastSignificant { get; } =
		new(2, BitConverter.IsLittleEndian, false);

	public static BitStreamFormat ShortLittleEndianMostSignificant { get; } =
		new(2, BitConverter.IsLittleEndian, true);

	public int ByteSize { get; }

	public bool ByteSwap { get; }

	public bool Reversed { get; }

	public override string ToString()
	{
		return $"{nameof(ByteSize)}: {ByteSize}, {nameof(ByteSwap)}: {ByteSwap}, {nameof(Reversed)}: {Reversed}";
	}
}