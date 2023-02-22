using System;
using System.Collections.Generic;
using Mono.Cecil;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;

namespace Patcher.Tests;

public class TypeDefinitionExtTests
{
    private class TestEmptyClass
    {
    }

#pragma warning disable CS0649
#pragma warning disable CS8618
    private class TestFieldsClass
    {
        public int TestInt;
        public string TestString;
    }
#pragma warning restore CS8618
#pragma warning restore CS0649

#pragma warning disable CS8618
    private class TestPropertiesClass
    {
        public int TestInt { get; set; }
        public string TestString { get; set; }
        public List<object> TestList { get; set; }
    }
#pragma warning restore CS8618

    /// <summary>
    /// Sets up a module and type definition for testing
    /// </summary>
    /// <returns>Type definition called Test, in a module called Test</returns>
    private static TypeDefinition Setup()
    {
        var module = ModuleDefinition.CreateModule("Test", new ModuleParameters
        {
            Kind = ModuleKind.Dll
        });
        var typeDef = new TypeDefinition("Test", "Test", TypeAttributes.Public);
        module.Types.Add(typeDef);

        return typeDef;
    }

    [Fact]
    public void Empty()
    {
        var type = Setup();

        type.AddAllFields(typeof(TestEmptyClass));

        Assert.False(type.HasFields);
        Assert.False(type.HasProperties);
        Assert.False(type.HasMethods);
    }

    [Fact]
    public void AllFields()
    {
        var type = Setup();

        type.AddAllFields(typeof(TestFieldsClass));

        Assert.True(type.HasFields);
        Assert.Equal(2, type.Fields.Count);

        Assert.Equal(nameof(TestFieldsClass.TestInt), type.Fields[0].Name);
        Assert.Equal(nameof(TestFieldsClass.TestString), type.Fields[1].Name);

        Assert.Equal(typeof(int), type.Fields[0].FieldType.ResolveReflection());
        Assert.Equal(typeof(string), type.Fields[1].FieldType.ResolveReflection());
    }

    [Fact]
    public void AllProperties()
    {
        var type = Setup();

        type.AddAllProperties(typeof(TestPropertiesClass));

        Assert.True(type.HasProperties);
        Assert.Equal(3, type.Properties.Count);

        Assert.Equal(nameof(TestPropertiesClass.TestInt), type.Properties[0].Name);
        Assert.Equal(nameof(TestPropertiesClass.TestString), type.Properties[1].Name);
        Assert.Equal(nameof(TestPropertiesClass.TestList), type.Properties[2].Name);

        Assert.Equal(typeof(int), type.Properties[0].PropertyType.ResolveReflection());
        Assert.Equal(typeof(string), type.Properties[1].PropertyType.ResolveReflection());
        Assert.Equal(typeof(List<object>), type.Properties[2].PropertyType.ResolveReflection());
    }
}