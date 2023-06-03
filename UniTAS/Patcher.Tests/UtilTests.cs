using UniTAS.Patcher.Utils;

namespace Patcher.Tests;

public class UtilTests
{
    [Fact]
    public void ProjectName()
    {
        Assert.Equal("UniTAS.Patcher", PatcherUtils.ProjectAssembly);
    }
}