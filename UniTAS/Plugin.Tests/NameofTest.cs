namespace UniTAS.Plugin.Tests;

public class NameofTest
{
    [Fact]
    public void UniTASPlugin()
    {
        Assert.Equal("UniTAS.Plugin", typeof(Plugin).Namespace);
    }
}