using System;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Classes that inherit from this will be registered as a dependency related with the interface
/// The interfaces has to be in the same assembly as the class to be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterAttribute : DependencyInjectionAttribute
{
    public bool IncludeDifferentAssembly { get; set; }
    public Type[] IgnoreInterfaces { get; }

    public RegisterAttribute(params Type[] ignoreInterfaces)
    {
        IgnoreInterfaces = ignoreInterfaces;
    }
}