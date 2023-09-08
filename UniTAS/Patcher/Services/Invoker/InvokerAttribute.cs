using System;
using UniTAS.Patcher.Models.Invoker;

namespace UniTAS.Patcher.Services.Invoker;

/// <summary>
/// Attributes for invoking methods at specific timing in patcher
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class InvokerAttribute : Attribute
{
    public InvokerPriority Priority = InvokerPriority.Default;
}