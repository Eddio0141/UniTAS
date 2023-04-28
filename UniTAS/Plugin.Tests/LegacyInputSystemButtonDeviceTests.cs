using UniTAS.Plugin.Interfaces.VirtualEnvironment;

namespace UniTAS.Plugin.Tests;

public class LegacyInputSystemButtonDeviceTests
{
    private class SomeDeviceWithButtons : LegacyInputSystemButtonBasedDevice<string>
    {
        public new void Hold(string button)
        {
            base.Hold(button);
        }

        public new void Release(string button)
        {
            base.Release(button);
        }

        public new bool IsButtonDown(string button)
        {
            return base.IsButtonDown(button);
        }

        public new bool IsButtonHeld(string button)
        {
            return base.IsButtonHeld(button);
        }

        public new bool IsButtonUp(string button)
        {
            return base.IsButtonUp(button);
        }
    }

    [Fact]
    public void ButtonDownTest()
    {
        var device = new SomeDeviceWithButtons();
        device.Hold("A");

        device.PreUpdateActual();

        Assert.True(device.IsButtonDown("A"));
        Assert.False(device.IsButtonDown("B"));

        device.OnLastUpdateActual();

        Assert.False(device.IsButtonDown("A"));
        Assert.False(device.IsButtonDown("B"));
    }

    [Fact]
    public void ButtonHeldTest()
    {
        var device = new SomeDeviceWithButtons();
        device.Hold("A");

        device.PreUpdateActual();

        Assert.True(device.IsButtonHeld("A"));
        Assert.False(device.IsButtonHeld("B"));

        device.OnLastUpdateActual();

        Assert.True(device.IsButtonHeld("A"));
        Assert.False(device.IsButtonHeld("B"));

        device.Release("A");

        device.PreUpdateActual();

        Assert.False(device.IsButtonHeld("A"));
        Assert.False(device.IsButtonHeld("B"));
    }

    [Fact]
    public void ButtonUpTest()
    {
        var device = new SomeDeviceWithButtons();
        device.Hold("A");

        device.PreUpdateActual();

        Assert.False(device.IsButtonUp("A"));
        Assert.False(device.IsButtonUp("B"));

        device.OnLastUpdateActual();

        Assert.False(device.IsButtonUp("A"));
        Assert.False(device.IsButtonUp("B"));

        device.Release("A");

        device.PreUpdateActual();
        device.OnLastUpdateActual();

        Assert.True(device.IsButtonUp("A"));
        Assert.False(device.IsButtonUp("B"));

        device.OnLastUpdateActual();

        Assert.False(device.IsButtonUp("A"));
        Assert.False(device.IsButtonUp("B"));
    }
}