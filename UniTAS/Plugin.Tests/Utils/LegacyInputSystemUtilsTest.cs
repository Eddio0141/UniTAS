using UniTAS.Plugin.Utils;
using UnityEngine;

namespace UniTAS.Plugin.Tests.Utils;

public class LegacyInputSystemUtilsTest
{
    [Fact]
    public void Alphabet()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("a", out var keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.A, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("z", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Z, keyCode);
    }

    [Fact]
    public void AlphabetFail()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("aa", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Number()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("0", out var keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Alpha0, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("9", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Alpha9, keyCode);
    }

    [Fact]
    public void NumberFail()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("10", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("-1", out keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Numpad()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("[0]", out var keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Keypad0, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("[9]", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Keypad9, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("[+]", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.KeypadPlus, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("[-]", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.KeypadMinus, keyCode);
    }

    [Fact]
    public void NumpadFail()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("[-1]", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("[10]", out keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("[+1]", out keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void FunctionKey()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("f1", out var keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.F1, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("f15", out keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.F15, keyCode);
    }

    [Fact]
    public void FunctionKeyFail()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("f0", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
        ret = LegacyInputSystemUtils.KeyStringToKeyCode("f16", out keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void MostOtherKey()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("backspace", out var keyCode);
        Assert.True(ret);
        Assert.Equal(KeyCode.Backspace, keyCode);
    }

    [Fact]
    public void OutOfRangeEnum()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("%", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Empty()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Null()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode(null, out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }

    [Fact]
    public void Invalid()
    {
        var ret = LegacyInputSystemUtils.KeyStringToKeyCode("foo", out var keyCode);
        Assert.False(ret);
        Assert.Equal(default, keyCode);
    }
}