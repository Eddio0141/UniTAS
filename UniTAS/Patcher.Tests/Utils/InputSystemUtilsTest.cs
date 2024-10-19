using System;
using UniTAS.Patcher.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Patcher.Tests.Utils;

public class InputSystemUtilsTest
{
    [Fact]
    public void NumberRowParsing()
    {
        for (var i = 0; i < 10; i++)
        {
            InputSystemUtils.KeyStringToKeys($"Digit{i}", out var keyCode, out var newKey);
            Assert.NotNull(keyCode);
            Assert.Equal(Enum.Parse(typeof(KeyCode), $"Alpha{i}"), keyCode.Value);
            Assert.NotNull(newKey);
            Assert.Equal(Enum.Parse(typeof(Key), $"Digit{i}"), newKey.Value);

            InputSystemUtils.KeyStringToKeys($"Alpha{i}", out keyCode, out newKey);
            Assert.NotNull(keyCode);
            Assert.Equal(Enum.Parse(typeof(KeyCode), $"Alpha{i}"), keyCode.Value);
            Assert.NotNull(newKey);
            Assert.Equal(Enum.Parse(typeof(Key), $"Digit{i}"), newKey.Value);
        }
    }

    [Fact]
    public void AlphabetKeys()
    {
        char[] alphaBet =
        [
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'x', 'y', 'z'
        ];

        foreach (var c in alphaBet)
        {
            var correctKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), char.ToUpper(c).ToString());
            var correctKey = (Key)Enum.Parse(typeof(Key), char.ToUpper(c).ToString());

            InputSystemUtils.KeyStringToKeys(c.ToString(), out var keyCode, out var newKey);
            Assert.Equal(correctKeyCode, keyCode);
            Assert.Equal(correctKey, newKey);

            InputSystemUtils.KeyStringToKeys(char.ToUpper(c).ToString(), out keyCode, out newKey);
            Assert.Equal(correctKeyCode, keyCode);
            Assert.Equal(correctKey, newKey);
        }
    }
}