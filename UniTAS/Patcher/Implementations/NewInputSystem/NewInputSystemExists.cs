using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.InputSystemOverride;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class NewInputSystemExists : INewInputSystemExists
{
    public bool HasInputSystem { get; }

    public NewInputSystemExists()
    {
        try
        {
            var mouseCurrent = AccessTools.TypeByName("UnityEngine.InputSystem.Mouse");
            var current = AccessTools.Property(mouseCurrent, "current");
            current.GetValue(null, null);

            HasInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}