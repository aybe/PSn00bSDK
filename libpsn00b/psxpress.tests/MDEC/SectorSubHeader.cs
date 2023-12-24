namespace psxpress.tests.MDEC;

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