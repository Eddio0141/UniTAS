using System;

namespace UniTAS.Patcher.Exceptions;

public class DeepCopyMaxRecursionException : Exception
{
    public DeepCopyMaxRecursionException(object source, string path) : base(
        $"MakeDeepCopy recursion depth limit exceeded, object type: {source.GetType()}, path: {path}")
    {
    }
}