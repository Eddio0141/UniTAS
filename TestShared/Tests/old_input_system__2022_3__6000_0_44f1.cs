using System.Collections.Generic;
using UnityEngine;

[MovieTest(MovieTestTiming.Awake)]
public class old_input_system__2022_3__6000_0_44f1 : MonoBehaviour
{
    [Test]
    public IEnumerator<TestYield> MousePosLock()
    {
        Assert.Equal(Input.mousePosition, new Vector3(155, 205), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.000001f);
        Screen.SetResolution(100, 100, false);
        Cursor.lockState = CursorLockMode.Locked;
        yield return new UnityYield(null);
        Assert.Equal(Screen.width, 100);
        Assert.Equal(Screen.height, 100);

        Assert.Equal(Input.mousePosition, new Vector3(100, 100), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(100, 100), 0.000001f);
        yield return new UnityYield(null);

        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(40, 40), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(500, 500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(500, 500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(25, -25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(-600, -600), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(-600, -600), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(0, 0), 0.000001f);

        yield return new UnityYield(null);

        Cursor.lockState = CursorLockMode.Confined;
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(0, 0), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(75, 75), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(25, 25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(100, 100), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(25, 25), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(100, 100), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(0, 0), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(-100, -100), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(0, 0), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.000001f);

        Cursor.lockState = CursorLockMode.None;
        Screen.SetResolution(1920, 1080, false);

        yield return new UnityYield(null);
    }

    [Test]
    public IEnumerator<TestYield> MouseAxis()
    {
        Assert.Equal(Input.mousePositionDelta, new Vector3(150f, 142f), 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), 15f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), 14.2f, 0.0000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePositionDelta, new Vector3(-100f, -110f), 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), -10f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), -11f, 0.0000001f);
        yield return new UnityYield(null);
        Debug.Log("3");
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), 0f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), 0f, 0.0000001f);
        yield return new UnityYield(null);
        Debug.Log("4");
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), 0f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), 0f, 0.0000001f);
        yield return new UnityYield(null);
        // limit within resolution
        Debug.Log("5");
        Assert.Equal(Input.mousePositionDelta, new Vector3(1920f, 1080f), 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), 192f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), 108f, 0.0000001f);
        yield return new UnityYield(null);
        Debug.Log("6");
        Assert.Equal(Input.mousePositionDelta, new Vector3(-1920f, -1080f), 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse X"), -192f, 0.0000001f);
        Assert.Equal(Input.GetAxis("Mouse Y"), -108f, 0.0000001f);
        yield return new UnityYield(null);
    }

    [Test]
    public IEnumerator<TestYield> KeyboardTwoKeys()
    {
        // press A and B, but B is 1f delayed in action
        Assert.False(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.True(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.True(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.True(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.True(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.True(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.True(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.True(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.True(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        // tap A and B twice, but B is delayed 1f in action
        Assert.True(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.True(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.True(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.True(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.True(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.True(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.True(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.True(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.True(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.True(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.True(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.True(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);

        Assert.False(Input.GetKey(KeyCode.A));
        Assert.False(Input.GetKeyUp(KeyCode.A));
        Assert.False(Input.GetKeyDown(KeyCode.A));
        Assert.False(Input.GetKey(KeyCode.B));
        Assert.False(Input.GetKeyUp(KeyCode.B));
        Assert.False(Input.GetKeyDown(KeyCode.B));
        yield return new UnityYield(null);
    }
}
