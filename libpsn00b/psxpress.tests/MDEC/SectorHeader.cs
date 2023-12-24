namespace psxpress.tests.MDEC;

public readonly struct SectorHeader
{
	public readonly SectorAddress Address;

	public readonly SectorMode Mode;

	public override string ToString()
	{
		return $"{nameof(Address)}: {Address}, {nameof(Mode)}: {Mode}";
	}
}