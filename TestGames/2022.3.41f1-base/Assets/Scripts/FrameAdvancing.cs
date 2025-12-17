using System.Collections;
using UnityEngine;

public class FrameAdvancing : MonoBehaviour
{
    // ReSharper disable NotAccessedField.Local
    private static int _yieldNull;
    // ReSharper restore NotAccessedField.Local

    private IEnumerator Start()
    {
        var startFrame = Time.frameCount;
        for (var i = 0; i < 5; i++)
        {
            _yieldNull = i;
            yield return null;
        }

        Assert.Equal("frame_advancing.yield.null", 5, Time.frameCount - startFrame);

        startFrame = Time.frameCount;
        yield return new WaitForSeconds(1f);
        Assert.Equal("frame_advancing.yield.WaitForSeconds", 101, Time.frameCount - startFrame);

        Assert.Finish();
    }
}