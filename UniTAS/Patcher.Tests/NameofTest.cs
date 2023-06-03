namespace Patcher.Tests;

public class NameofTest
{
    [Fact]
    public void UniTASPlugin()
    {
        Assert.Equal("UniTAS.Plugin", typeof(UniTAS.Patcher.Plugin).Namespace);
    }
}