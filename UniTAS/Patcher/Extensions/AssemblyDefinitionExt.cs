using System;
using Mono.Cecil;

namespace UniTAS.Patcher.Extensions;

public static class AssemblyDefinitionExt
{
    public static void AddType(this AssemblyDefinition assembly, Type type)
    {
        var typeDef = new TypeDefinition(type.Namespace, type.Name, (TypeAttributes)type.Attributes,
            assembly.MainModule.ImportReference(type.BaseType));

        typeDef.AddAllContents(type);

        assembly.MainModule.Types.Add(typeDef);
    }
}