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
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.000001f);
        yield return new UnityYield(null);

        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, Vector3.zero, 0.000001f);
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
        Assert.Equal(Input.mousePositionDelta, new Vector3(-500, -500), 0.000001f);
        yield return new UnityYield(null);
        Assert.Equal(Input.mousePosition, new Vector3(50, 50), 0.000001f);
        Assert.Equal(Input.mousePositionDelta, new Vector3(-500, -500), 0.000001f);
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
    }

    // TODO: test axis
}
