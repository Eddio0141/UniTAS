using UniTAS.Plugin.Extensions;

namespace UniTAS.Plugin.Tests.Extensions;

public class StringExtensionsTest
{
    [Fact]
    public void EqualsExact()
    {
        const string str = "test";
        const string pattern = "test";

        Assert.True(str.Like(pattern));
    }

    [Fact]
    public void EqualsDotAny()
    {
        const string str = "System.Xml";
        const string pattern = "System.*";

        Assert.True(str.Like(pattern));
    }
}