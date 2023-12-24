namespace psxpress.tests.MDEC;

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