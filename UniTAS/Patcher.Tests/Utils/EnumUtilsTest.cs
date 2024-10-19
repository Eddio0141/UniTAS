using UniTAS.Patcher.Utils;
using UnityEngine;

namespace Patcher.Tests.Utils;

public class EnumUtilsTest
{
    [Fact]
    public void TryParseStrict()
    {
        var res = EnumUtils.TryParse("None", out KeyCode resEnum);
        Assert.True(res);
        Assert.Equal(KeyCode.None, resEnum);

        res = EnumUtils.TryParse("1", out resEnum);
        Assert.False(res);
    }
    
    [Fact]
    public void TryParseCaseInsensitive()
    {
        var res = EnumUtils.TryParse("none", out KeyCode resEnum, true);
        Assert.True(res);
        Assert.Equal(KeyCode.None, resEnum);

        // also tests caching
        res = EnumUtils.TryParse("None", out resEnum, true);
        Assert.True(res);
        Assert.Equal(KeyCode.None, resEnum);
        
        res = EnumUtils.TryParse("1", out resEnum, true);
        Assert.False(res);
    }
}