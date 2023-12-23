// ReSharper disable IdentifierTypo

namespace psxpress.tests;

public abstract class UnitTestBase
{
	public TestContext TestContext { get; set; } = null !;

	protected void Write(object? value = null)
	{
		TestContext.Write(value?.ToString());
	}

	protected void WriteLine(object? value = null)
	{
		TestContext.WriteLine(value?.ToString());
	}
}