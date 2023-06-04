using System;
using UniTAS.Patcher.Services.Invoker;

namespace UniTAS.Patcher.Interfaces.Invoker;

/// <summary>
/// Invokes static constructor <see cref="Entry.Finish"/> invoke
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class InvokeOnPatcherFinishAttribute : InvokerAttribute
{
}