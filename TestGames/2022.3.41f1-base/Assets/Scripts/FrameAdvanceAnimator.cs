using UnityEngine;

public class FrameAdvanceAnimator : MonoBehaviour
{
    // manually reset those in OnEnable
    private bool _timeTrigger;
    private int _loadFrameCount;

    private void OnEnable()
    {
        _timeTrigger = false;
        _loadFrameCount = Time.frameCount;
    }

    public void TimeTrigger()
    {
        if (_timeTrigger) return;
        _timeTrigger = true;
        Assert.Equal("frame_advance.animator.trigger_time", 100, Time.frameCount - _loadFrameCount);
    }
}