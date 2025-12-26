using System.Collections;
using UnityEngine;

public class MovieTest3: MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(AwakeCoroutine());
    }

    private IEnumerator AwakeCoroutine()
    {
        Assert.Equal("start_coroutine.frame_count", 0, Time.frameCount);
        yield return new WaitForSeconds(1f);
        Assert.Equal("yield.wait_for_seconds.elapsed_frames", 101, Time.frameCount);
    }

    private IEnumerator Start()
    {
        Assert.Equal("start_coroutine.frame_count", 1, Time.frameCount);
        yield return new WaitForSeconds(1f);
        Assert.Equal("yield.wait_for_seconds.elapsed_frames", 102, Time.frameCount);
    }
}