using System;
using HarmonyLib;
using Mono.Cecil;

namespace UniTAS.Patcher.Extensions;

public static class TypeDefinitionExt
{
    public static void AddAllContents(this TypeDefinition type, Type originalType)
    {
        type.AddAllFields(originalType);
        type.AddAllProperties(originalType);
        type.AddAllMethods(originalType);

        // TODO add all nested types
    }

    public static void AddAllFields(this TypeDefinition type, Type originalType)
    {
        var fields = originalType.GetFields(AccessTools.all);

        foreach (var field in fields)
        {
            var fieldDef = new FieldDefinition(field.Name, (FieldAttributes)field.Attributes,
                type.Module.ImportReference(field.FieldType));
            type.Fields.Add(fieldDef);
        }
    }

    public static void AddAllProperties(this TypeDefinition type, Type originalType)
    {
        var properties = originalType.GetProperties(AccessTools.all);

        foreach (var property in properties)
        {
            var propertyDef = new PropertyDefinition(property.Name, (PropertyAttributes)property.Attributes,
                type.Module.ImportReference(property.PropertyType));
            type.Properties.Add(propertyDef);
        }
    }

    public static void AddAllMethods(this TypeDefinition type, Type originalType)
    {
        var methods = originalType.GetMethods(AccessTools.all);

        foreach (var method in methods)
        {
            var methodDef = new MethodDefinition(method.Name, (MethodAttributes)method.Attributes,
                type.Module.ImportReference(method.ReturnType));
            type.Methods.Add(methodDef);
        }
    }
}