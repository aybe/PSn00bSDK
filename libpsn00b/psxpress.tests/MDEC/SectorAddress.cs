namespace psxpress.tests.MDEC;

public readonly struct SectorAddress
{
	public readonly byte M, S, F;

	public override string ToString()
	{
		return $"{M:X2}:{S:X2}.{F:X2}";
	}
}