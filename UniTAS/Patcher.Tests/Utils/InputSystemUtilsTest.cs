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
}