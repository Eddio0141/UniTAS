using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace Patcher.Tests.VirtualEnv;

public class KeyEqualityCheck
{
    [Fact]
    public void KeyCodeEquals()
    {
        var container = KernelUtils.Init();
        var keyFactory = container.GetInstance<IKeyFactory>();

        var key1 = keyFactory.CreateKey(KeyCode.A);
        var key2 = keyFactory.CreateKey(KeyCode.A);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void KeyCodeAndStringEquals()
    {
        var container = KernelUtils.Init();
        var keyFactory = container.GetInstance<IKeyFactory>();

        var key1 = keyFactory.CreateKey(KeyCode.A);
        var key2 = keyFactory.CreateKey("A");

        Assert.Equal(key1, key2);
    }
}