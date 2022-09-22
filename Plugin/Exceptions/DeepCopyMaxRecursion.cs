using System;

namespace UniTASPlugin.Exceptions;

internal class DeepCopyMaxRecursion : Exception
{
    internal DeepCopyMaxRecursion() : base("MakeDeepCopy recursion depth limit exceeded") { }
}