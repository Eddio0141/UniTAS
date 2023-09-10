using System;

namespace UniTAS.Patcher.Services.Invoker;

/// <summary>
/// Attributes for invoking methods at specific timing in patcher
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class InvokerAttribute : Attribute
{
}