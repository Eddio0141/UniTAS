using System;
using UniTAS.Plugin.Models.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Use this to register a class as a singleton
/// The interfaces has to be in the same assembly as the class to be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : RegisterAttribute
{
    public SingletonAttribute(RegisterPriority priority = RegisterPriority.Default) : base(priority)
    {
    }
}