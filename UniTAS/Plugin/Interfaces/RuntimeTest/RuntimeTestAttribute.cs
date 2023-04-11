using System;

namespace UniTAS.Plugin.Interfaces.RuntimeTest;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RuntimeTestAttribute : Attribute
{
}