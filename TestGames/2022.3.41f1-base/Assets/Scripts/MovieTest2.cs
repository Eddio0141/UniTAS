using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovieTest2 : MonoBehaviour
{
    private IEnumerator Start()
    {
        Assert.Equal("time_since_level_load", 0f, Time.timeSinceLevelLoad, 0.0001f);
        yield return null;
        Assert.Equal("time_since_level_load", 0.01f, Time.timeSinceLevelLoad, 0.0001f);
        yield return new WaitForEndOfFrame();
        Assert.Equal("time_since_level_load", 0.01f, Time.timeSinceLevelLoad, 0.0001f);

        // keyboard
        Assert.False("Input.GetKeyDown.KeyCode", Input.GetKeyDown(KeyCode.A));
        Assert.False("Input.GetKeyDown.String", Input.GetKeyDown("A"));
        Assert.False("Input.GetKeyUp.KeyCode", Input.GetKeyUp(KeyCode.A));
        Assert.False("Input.GetKeyUp.String", Input.GetKeyUp("A"));
        Assert.False("Input.GetKey.KeyCode", Input.GetKey(KeyCode.A));
        Assert.False("Input.GetKey.String", Input.GetKey("a"));
        Assert.False("InputSystem.Keyboard.isPressed", Keyboard.current.aKey.isPressed);
        yield return null;
        Assert.True("Input.GetKeyDown.KeyCode", Input.GetKeyDown(KeyCode.A));
        Assert.True("Input.GetKeyDown.String", Input.GetKeyDown("A"));
        Assert.False("Input.GetKeyUp.KeyCode", Input.GetKeyUp(KeyCode.A));
        Assert.False("Input.GetKeyUp.String", Input.GetKeyUp("A"));
        Assert.True("Input.GetKey.KeyCode", Input.GetKey(KeyCode.A));
        Assert.True("Input.GetKey.String", Input.GetKey("a"));
        Assert.True("InputSystem.Keyboard.isPressed", Keyboard.current.aKey.isPressed);
        yield return null;
        Assert.False("Input.GetKeyDown.KeyCode", Input.GetKeyDown(KeyCode.A));
        Assert.False("Input.GetKeyDown.String", Input.GetKeyDown("A"));
        Assert.False("Input.GetKeyUp.KeyCode", Input.GetKeyUp(KeyCode.A));
        Assert.False("Input.GetKeyUp.String", Input.GetKeyUp("A"));
        Assert.True("Input.GetKey.KeyCode", Input.GetKey(KeyCode.A));
        Assert.True("Input.GetKey.String", Input.GetKey("a"));
        Assert.True("InputSystem.Keyboard.isPressed", Keyboard.current.aKey.isPressed);
        yield return null;
        Assert.False("Input.GetKeyDown.KeyCode", Input.GetKeyDown(KeyCode.A));
        Assert.False("Input.GetKeyDown.String", Input.GetKeyDown("A"));
        Assert.True("Input.GetKeyUp.KeyCode", Input.GetKeyUp(KeyCode.A));
        Assert.True("Input.GetKeyUp.String", Input.GetKeyUp("A"));
        Assert.False("Input.GetKey.KeyCode", Input.GetKey(KeyCode.A));
        Assert.False("Input.GetKey.String", Input.GetKey("a"));
        Assert.False("InputSystem.Keyboard.isPressed", Keyboard.current.aKey.isPressed);

        // mouse
        for (var i = 0; i <= 5; i++)
        {
            yield return null;
            yield return null;
            Assert.Equal("ugui.click_count", i, _clickCount);
        }

        Assert.Equal("Input.mousePosition", Vector3.zero, Input.mousePosition);
        Assert.Equal("Input.GetAxis.Mouse_X", 0f, Input.GetAxis("Mouse X"));
        Assert.Equal("Input.GetAxis.Mouse_Y", 0f, Input.GetAxis("Mouse Y"));
        Assert.Equal("InputSystem.Mouse.position", Vector2.zero, Mouse.current.position.ReadValue());
        yield return null;
        Assert.Equal("Input.mousePosition", new Vector3(123f, 456f), Input.mousePosition);
        Assert.Equal("Input.GetAxis.Mouse_X", 12.3f, Input.GetAxis("Mouse X"), 0.0001f);
        Assert.Equal("Input.GetAxis.Mouse_Y", 45.6f, Input.GetAxis("Mouse Y"), 0.0001f);
        Assert.Equal("InputSystem.Mouse.position", new Vector2(123f, 456f), Mouse.current.position.ReadValue());

        Assert.False("Input.GetMouseButtonDown", Input.GetMouseButtonDown(0));
        Assert.False("Input.GetMouseButtonUp", Input.GetMouseButtonUp(0));
        Assert.False("Input.GetMouseButton", Input.GetMouseButton(0));
        Assert.False("InputSystem.Mouse.leftButton.isPressed", Mouse.current.leftButton.isPressed);
        yield return null;
        Assert.True("Input.GetMouseButtonDown", Input.GetMouseButtonDown(0));
        Assert.False("Input.GetMouseButtonUp", Input.GetMouseButtonUp(0));
        Assert.True("Input.GetMouseButton", Input.GetMouseButton(0));
        Assert.True("InputSystem.Mouse.leftButton.isPressed", Mouse.current.leftButton.isPressed);
        yield return null;
        Assert.False("Input.GetMouseButtonDown", Input.GetMouseButtonDown(0));
        Assert.False("Input.GetMouseButtonUp", Input.GetMouseButtonUp(0));
        Assert.True("Input.GetMouseButton", Input.GetMouseButton(0));
        Assert.True("InputSystem.Mouse.leftButton.isPressed", Mouse.current.leftButton.isPressed);
        yield return null;
        Assert.False("Input.GetMouseButtonDown", Input.GetMouseButtonDown(0));
        Assert.True("Input.GetMouseButtonUp", Input.GetMouseButtonUp(0));
        Assert.False("Input.GetMouseButton", Input.GetMouseButton(0));
        Assert.False("InputSystem.Mouse.leftButton.isPressed", Mouse.current.leftButton.isPressed);
        yield return null;
        Assert.False("Input.GetMouseButtonDown", Input.GetMouseButtonDown(0));
        Assert.False("Input.GetMouseButtonUp", Input.GetMouseButtonUp(0));
        Assert.False("Input.GetMouseButton", Input.GetMouseButton(0));
        Assert.False("InputSystem.Mouse.leftButton.isPressed", Mouse.current.leftButton.isPressed);

        Assert.Finish();
    }

    private int _clickCount;

    public void Click()
    {
        _clickCount++;
    }
}