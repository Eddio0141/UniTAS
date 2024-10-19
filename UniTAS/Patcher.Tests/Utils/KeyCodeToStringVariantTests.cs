using UniTAS.Patcher.Utils;
using UnityEngine;

namespace Patcher.Tests.Utils;

public class KeyCodeToStringVariantTests
{
    [Fact]
    public void JoystickButtons()
    {
        Assert.Equal("joystick button 0", InputSystemUtils.KeyCodeToStringVariant(KeyCode.JoystickButton0));
        Assert.Equal("joystick button 5", InputSystemUtils.KeyCodeToStringVariant(KeyCode.JoystickButton5));
        Assert.Equal("joystick 3 button 6", InputSystemUtils.KeyCodeToStringVariant(KeyCode.Joystick3Button6));
    }

    [Fact]
    public void CamelCase()
    {
        Assert.Equal("page up", InputSystemUtils.KeyCodeToStringVariant(KeyCode.PageUp));
    }
}