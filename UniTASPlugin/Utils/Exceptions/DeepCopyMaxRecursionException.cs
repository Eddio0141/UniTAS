using System;

namespace UniTASPlugin.Utils.Exceptions;

public class DeepCopyMaxRecursionException : Exception
{
    public DeepCopyMaxRecursionException() : base("MakeDeepCopy recursion depth limit exceeded")
    {
    }
}