using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.MonoBehaviourPause;

[Singleton]
[ForceInstantiate]
public class AnimationPause
{
    private readonly ILogger _logger;

    public AnimationPause(IMonoBehaviourController monoBehaviourController, ILogger logger)
    {
        _logger = logger;
        monoBehaviourController.OnPauseChange += OnPauseChange;
    }

    private void OnPauseChange(bool pause)
    {
        if (pause)
            PauseAnimation();
        else
            ResumeAnimation();
    }

    private readonly List<AnimatorTracker> _trackedAnimators = [];
    private readonly List<AnimationTracker> _trackedAnimations = [];

    private void PauseAnimation()
    {
        RefreshTrackedAnimators();
        RefreshTrackedAnimations();
        foreach (var animator in _trackedAnimators)
        {
            animator.Pause();
        }

        foreach (var animation in _trackedAnimations)
        {
            animation.Pause();
        }
    }

    private void ResumeAnimation()
    {
        foreach (var animator in _trackedAnimators)
        {
            animator.Resume();
        }

        foreach (var animation in _trackedAnimations)
        {
            animation.Resume();
        }

        _trackedAnimators.Clear();
        _trackedAnimations.Clear();
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

    private void RefreshTrackedAnimations()
    {
        var animations = ObjectUtils.FindObjectsOfType<Animation>();
        _trackedAnimations.Clear();
        _logger.LogDebug(
            $"refreshing tracked animations for frame advancing, found {animations.Length} animations in scene");
        foreach (var animation in animations)
        {
            _trackedAnimations.Add(new(animation));
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
            if (animator == null) return;
            SetSpeed.Invoke(animator, [_speedBeforePause]);
        }
    }

    private class AnimationTracker(Animation animation)
    {
        private readonly Dictionary<AnimationState, float> _speeds = new();

        public void Pause()
        {
            foreach (AnimationState state in animation)
            {
                _speeds.Add(state, state.speed);
                state.speed = 0f;
            }
        }

        public void Resume()
        {
            foreach (var keyValue in _speeds)
            {
                if (keyValue.Key == null) continue;
                keyValue.Key.speed = keyValue.Value;
            }

            _speeds.Clear();
        }
    }
}