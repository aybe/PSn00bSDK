namespace psxpress.tests.MDEC;

public readonly struct StreamPositionScope(Stream stream) : IDisposable
{
	private readonly long Position = stream.Position;

	public void Dispose()
	{
		stream.Position = Position;
	}
}