using System;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Register all classes that inherit from this
/// If the registering class has an attribute such as singleton, it will be registered as a singleton
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterAllAttribute : DependencyInjectionAttribute
{
}