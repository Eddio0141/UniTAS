using System;

namespace UniTAS.Patcher.Services.Invoker;

/// <summary>
/// Attributes for invoking methods at specific timing in patcher
/// </summary>
public abstract class InvokerAttribute : Attribute
{
    public abstract void HandleType(Type type);
}