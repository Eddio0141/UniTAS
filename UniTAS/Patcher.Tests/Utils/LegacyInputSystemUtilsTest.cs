using UniTAS.Patcher.Utils;
using UnityEngine;

namespace Patcher.Tests.Utils;

public class LegacyInputSystemUtilsTest
{
    [Fact]
    public void Alphabet()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("a");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.A, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("z");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Z, keyCode);
    }

    [Fact]
    public void AlphabetFail()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("aa");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Number()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("0");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Alpha0, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("9");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Alpha9, keyCode);
    }

    [Fact]
    public void NumberFail()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("10");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("-1");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Numpad()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("[0]");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Keypad0, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("[9]");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Keypad9, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("[+]");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.KeypadPlus, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("[-]");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.KeypadMinus, keyCode);
    }

    [Fact]
    public void NumpadFail()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("[-1]");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("[10]");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("[+1]");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void FunctionKey()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("f1");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.F1, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("f15");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.F15, keyCode);
    }

    [Fact]
    public void FunctionKeyFail()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("f0");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
        keyCode = InputSystemUtils.KeyCodeParse("f16");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void MostOtherKey()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("backspace");
        Assert.NotNull(keyCode);
        Assert.Equal(KeyCode.Backspace, keyCode);
    }

    [Fact]
    public void OutOfRangeEnum()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("%");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Empty()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Null()
    {
        var keyCode = InputSystemUtils.KeyCodeParse(null);
        Assert.Null(keyCode);
    }

    [Fact]
    public void Invalid()
    {
        var keyCode = InputSystemUtils.KeyCodeParse("foo");
        Assert.Null(keyCode);
        Assert.Equal(default, keyCode);
    }
}