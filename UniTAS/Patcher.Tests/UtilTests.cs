using UniTAS.Patcher;

namespace Patcher.Tests;

public class UtilTests
{
    [Fact]
    public void ProjectName()
    {
        Assert.Equal("UniTAS.Patcher", Utils.ProjectName);
    }
}