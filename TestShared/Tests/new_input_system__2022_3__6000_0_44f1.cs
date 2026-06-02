using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[MovieTest(MovieTestTiming.Awake)]
public class new_input_system__2022_3__6000_0_44f1 : MonoBehaviour
{
    [Test]
    public IEnumerator<TestYield> MousePosLock()
    {
        var mouse = Mouse.current;
        Assert.Equal(mouse.position.ReadValue(), new Vector2(155, 205), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), Vector2.zero, 0.000001f);
        Screen.SetResolution(100, 100, false);
        Cursor.lockState = CursorLockMode.Locked;
        yield return new UnityYield(null);
        Assert.Equal(Screen.width, 100);
        Assert.Equal(Screen.height, 100);

        Assert.Equal(mouse.position.ReadValue(), new Vector2(100, 100), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(100, 100), 0.000001f);
        yield return new UnityYield(null);

        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(40, 40), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(500, 500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(500, 500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(25, -25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(-500, -500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(-500, -500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(0, 0), 0.000001f);

        yield return new UnityYield(null);

        Cursor.lockState = CursorLockMode.Confined;
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(50, 50), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(75, 75), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(25, 25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(100, 100), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(25, 25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(100, 100), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), Vector2.zero, 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(0, 0), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), new Vector2(-100, -100), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(mouse.position.ReadValue(), new Vector2(0, 0), 0.000001f);
        Assert.Equal(mouse.delta.ReadValue(), Vector2.zero, 0.000001f);

        Cursor.lockState = CursorLockMode.None;
        Screen.SetResolution(1920, 1080, false);

        yield return new UnityYield(null);
    }

    [Test]
    public IEnumerator<TestYield> KeyboardTwoKeys()
    {
        var keyboard = Keyboard.current;
        // press A and B, but B is 1f delayed in action
        Assert.False(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.True(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.True(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.True(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.True(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.True(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.True(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.True(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.True(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        // tap A and B twice, but B is delayed 1f in action
        Assert.True(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.True(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.True(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.True(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.True(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.True(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.True(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.True(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.True(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.True(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.True(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.True(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);

        Assert.False(keyboard.aKey.isPressed);
        Assert.False(keyboard.aKey.wasReleasedThisFrame);
        Assert.False(keyboard.aKey.wasPressedThisFrame);
        Assert.False(keyboard.bKey.isPressed);
        Assert.False(keyboard.bKey.wasReleasedThisFrame);
        Assert.False(keyboard.bKey.wasPressedThisFrame);
        yield return new UnityYield(null);
    }
}
