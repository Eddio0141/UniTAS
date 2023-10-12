using Mono.Cecil;
using MonoMod.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Extensions;

public static class TypeDefinitionExtensions
{
    /// <summary>
    /// Checks if the type is a subclass of <see cref="MonoBehaviour"/>
    /// </summary>
    public static bool IsMonoBehaviour(this TypeDefinition type)
    {
        var baseType = type.BaseType?.SafeResolve();
        while (baseType != null)
        {
            if (baseType.FullName == "UnityEngine.MonoBehaviour")
            {
                return true;
            }

            baseType = baseType.BaseType?.SafeResolve();
        }

        return false;
    }
}