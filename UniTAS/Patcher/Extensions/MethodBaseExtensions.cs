using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UniTAS.Patcher.Extensions;

public static class MethodBaseExtensions
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public static bool IsExtern(this MethodBase methodBase)
    {
        return (methodBase.GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) != 0;
    }
}