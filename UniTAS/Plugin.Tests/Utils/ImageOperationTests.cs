using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Tests.Utils;

public class ImageOperationTests
{
    [Fact]
    public void ResizeNoChange()
    {
        var original = new byte[3];
        var result = ImageOperation.Resize(original, 1, 1, 1, 1);
        Assert.Equal(original, result);
    }

    [Fact]
    public void Resize_1x1To2x2()
    {
        var original = Enumerable.Repeat((byte)1, 6).ToArray();
        var resized = Enumerable.Repeat((byte)1, 12).ToArray();

        var result = ImageOperation.Resize(original, 1, 1, 2, 2);
        Assert.Equal(resized, result);
    }

    [Fact]
    public void Resize_1x1To3x1()
    {
        // 1x1 -> 3x1
        var original = Enumerable.Repeat((byte)1, 6).ToArray();
        var resized = new byte[] { 0, 0, 0, 1, 1, 1, 0, 0, 0 };

        var result = ImageOperation.Resize(original, 1, 1, 3, 1);
        Assert.Equal(resized, result);
    }

    [Fact]
    public void Resize_2x2To3x3()
    {
        var original = new byte[]
        {
            1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 11, 12
        };
        var resized = new byte[]
        {
            1, 2, 3, 1, 2, 3, 4, 5, 6,
            1, 2, 3, 1, 2, 3, 4, 5, 6,
            7, 8, 9, 7, 8, 9, 10, 11, 12
        };

        var result = ImageOperation.Resize(original, 2, 2, 3, 3);
        Assert.Equal(resized, result);
    }

    [Fact]
    public void Resize_900x600To1080p()
    {
        var original = Enumerable.Repeat((byte)0, 900 * 600 * 3).ToArray();
        var resized = Enumerable.Repeat((byte)0, 1920 * 1080 * 3).ToArray();

        var result = ImageOperation.Resize(original, 900, 600, 1920, 1080);
        Assert.Equal(resized, result);
    }

    [Fact]
    public void Resize_900x600To15x10()
    {
        var original = Enumerable.Repeat((byte)0, 900 * 600 * 3).ToArray();
        var resized = Enumerable.Repeat((byte)0, 15 * 10 * 3).ToArray();

        var result = ImageOperation.Resize(original, 900, 600, 15, 10);
        Assert.Equal(resized, result);
    }
}