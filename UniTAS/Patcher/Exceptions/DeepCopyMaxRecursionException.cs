using System;

namespace UniTAS.Patcher.Exceptions;

public class DeepCopyMaxRecursionException(object source)
    : Exception($"MakeDeepCopy recursion depth limit exceeded, object type: {source.GetType()}");