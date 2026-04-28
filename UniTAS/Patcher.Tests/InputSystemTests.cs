using UniTAS.Patcher.Models.VirtualEnvironment;

namespace Patcher.Tests;

public class InputSystemTests
{
    [Fact]
    public void ButtonDownTest()
    {
        var device = new BufferedFullKeyState<string>();
        device.Hold("A");

        device.Update();

        Assert.True(device.IsDown("A"));
        Assert.False(device.IsDown("B"));

        device.Update();

        Assert.False(device.IsDown("A"));
        Assert.False(device.IsDown("B"));

        device.Update();

        Assert.False(device.IsDown("A"));
        Assert.False(device.IsDown("B"));
    }

    [Fact]
    public void ButtonHeldTest()
    {
        var device = new BufferedFullKeyState<string>();
        device.Hold("A");

        device.Update();

        Assert.True(device.IsHeld("A"));
        Assert.False(device.IsHeld("B"));

        device.Update();
        device.Update();

        Assert.True(device.IsHeld("A"));
        Assert.False(device.IsHeld("B"));

        device.Update();
        device.Release("A");
        device.Update();

        Assert.False(device.IsHeld("A"));
        Assert.False(device.IsHeld("B"));
    }

    [Fact]
    public void ButtonUpTest()
    {
        var device = new BufferedFullKeyState<string>();
        device.Hold("A");

        device.Update();

        Assert.False(device.IsUp("A"));
        Assert.False(device.IsUp("B"));

        device.Update();

        Assert.False(device.IsUp("A"));
        Assert.False(device.IsUp("B"));

        device.Release("A");
        device.Update();

        Assert.True(device.IsUp("A"));
        Assert.False(device.IsUp("B"));

        device.Update();

        Assert.False(device.IsUp("A"));
        Assert.False(device.IsUp("B"));
    }
}
