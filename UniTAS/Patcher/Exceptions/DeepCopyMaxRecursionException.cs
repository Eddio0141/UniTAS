using System;

namespace UniTAS.Patcher.Exceptions;

public class DeepCopyMaxRecursionException : Exception
{
    public DeepCopyMaxRecursionException() : base("MakeDeepCopy recursion depth limit exceeded")
    {
    }
}