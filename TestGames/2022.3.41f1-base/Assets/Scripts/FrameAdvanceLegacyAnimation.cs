using UnityEngine;

public class FrameAdvanceLegacyAnimation : MonoBehaviour
{
    // manually reset those in OnEnable
    private bool _timeTrigger;
    private bool _timeTriggerBlend;
    private int _loadFrameCount;

    [SerializeField] private Animation anim;

    private void OnEnable()
    {
        _timeTrigger = false;
        _timeTriggerBlend = false;
        _loadFrameCount = Time.frameCount;

        foreach (AnimationState state in anim)
        {
            state.wrapMode = WrapMode.Loop;
            if (state.name == "LegacyAnimationBlend")
            {
                state.speed = 0.9f;
            }
        }

        anim.Play("LegacyAnimationMove");
        anim.Blend("LegacyAnimationBlend");
    }

    public void TimeTrigger()
    {
        if (_timeTrigger) return;
        _timeTrigger = true;
        Assert.Equal("frame_advance.legacy_animator.trigger_time", 101, Time.frameCount - _loadFrameCount);
    }

    public void TimeTriggerBlend()
    {
        if (_timeTriggerBlend) return;
        _timeTriggerBlend = true;
        Assert.Equal("frame_advance.legacy_animator_blend.trigger_time", 112, Time.frameCount - _loadFrameCount);
    }
}