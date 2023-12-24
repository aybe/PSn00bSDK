namespace psxpress.tests.MDEC;

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