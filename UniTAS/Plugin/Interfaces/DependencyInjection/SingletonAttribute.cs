using System;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Use this to register a class as a singleton
/// The interfaces has to be in the same assembly as the class to be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : RegisterAttribute
{
    public SingletonAttribute(params Type[] ignoreInterfaces) : base(ignoreInterfaces)
    {
    }
}