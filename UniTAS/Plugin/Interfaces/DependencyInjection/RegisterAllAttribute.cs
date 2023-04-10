using System;
using UniTAS.Plugin.Models.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Register all classes that inherit from this
/// If the registering class has an attribute such as singleton, it will be registered as a singleton
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class RegisterAllAttribute : RegisterAttribute
{
    public RegisterAllAttribute(RegisterPriority priority = RegisterPriority.Default) : base(priority)
    {
    }
}