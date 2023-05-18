using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.InputSystemOverride;
using UnityEngine.InputSystem;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
public class NewInputSystemExists : INewInputSystemExists
{
    public bool HasInputSystem { get; }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public NewInputSystemExists()
    {
        try
        {
            if (Mouse.current != null)
            {
                // check dummy
            }

            HasInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}