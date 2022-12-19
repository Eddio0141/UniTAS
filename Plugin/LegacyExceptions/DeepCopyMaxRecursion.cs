using System;

namespace UniTASPlugin.LegacyExceptions;

internal class DeepCopyMaxRecursion : Exception
{
    internal DeepCopyMaxRecursion() : base("MakeDeepCopy recursion depth limit exceeded")
    {
    }
}