using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

// handles all animators for frame advancing
public partial class FrameAdvancing
{
    private readonly List<AnimatorTracker> _trackedAnimators = new();
    // private readonly List<Animation> _trackedAnimations = new();

    private void PauseAnimation()
    {
        RefreshTrackedAnimators();
        foreach (var animator in _trackedAnimators)
        {
            animator.Pause();
        }
    }

    private void ResumeAnimation()
    {
        foreach (var animator in _trackedAnimators)
        {
            animator.Resume();
        }

        _trackedAnimators.Clear();
    }

    private void RefreshTrackedAnimators()
    {
        var animators = Object.FindObjectsOfType(AnimatorTracker.Animator);
        _trackedAnimators.Clear();
        _logger.LogDebug(
            $"refreshing tracked animators for frame advancing, found {animators.Length} animators in scene");
        foreach (var animator in animators)
        {
            _trackedAnimators.Add(new(animator));
        }
    }

    private class AnimatorTracker(Object animator)
    {
        private float _speedBeforePause;

        private static MethodInfo GetSpeed { get; }
        private static MethodInfo SetSpeed { get; }
        public static Type Animator { get; }

        static AnimatorTracker()
        {
            Animator = AccessTools.TypeByName("UnityEngine.Animator");
            if (Animator == null) return;
            GetSpeed = AccessTools.PropertyGetter(Animator, "speed");
            SetSpeed = AccessTools.PropertySetter(Animator, "speed");
        }

        public void Pause()
        {
            _speedBeforePause = (float)GetSpeed.Invoke(animator, []);
            SetSpeed.Invoke(animator, [0]);
        }

        public void Resume()
        {
            SetSpeed.Invoke(animator, [_speedBeforePause]);
        }
    }
}