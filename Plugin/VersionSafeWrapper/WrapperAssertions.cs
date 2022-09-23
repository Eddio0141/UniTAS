using HarmonyLib;
using System;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class WrapperAssertions
{
    public static void AssertStaticField(Type type, string fieldName)
    {
        var field = AccessTools.Field(type, fieldName);
        if (field == null || !field.IsStatic)
            throw new Exceptions.MissingStaticField(type, fieldName);
    }

    public static void AssertInstanceField(Type type, string fieldName)
    {
        var field = AccessTools.Field(type, fieldName);
        if (field == null || field.IsStatic)
            throw new Exceptions.MissingInstanceField(type, fieldName);
    }

    public static void AssertStaticGetter(Type type, string propertyName)
    {
        var getter = AccessTools.PropertyGetter(type, propertyName);
        if (getter == null || !getter.IsStatic)
            throw new Exceptions.MissingStaticGetter(type, propertyName);
    }

    public static void AssertInstanceGetter(Type type, string propertyName)
    {
        var getter = AccessTools.PropertyGetter(type, propertyName);
        if (getter == null || getter.IsStatic)
            throw new Exceptions.MissingInstanceGetter(type, propertyName);
    }

    public static void AssertStaticSetter(Type type, string propertyName)
    {
        var setter = AccessTools.PropertySetter(type, propertyName);
        if (setter == null || !setter.IsStatic)
            throw new Exceptions.MissingStaticSetter(type, propertyName);
    }

    public static void AssertInstanceSetter(Type type, string propertyName)
    {
        var setter = AccessTools.PropertySetter(type, propertyName);
        if (setter == null || setter.IsStatic)
            throw new Exceptions.MissingInstanceSetter(type, propertyName);
    }

    // TODO
    /*
    public static void AssertStaticMethod(Type type, string methodName)
    {
        var methodTraverse = Traverse.Create(type).Method(methodName);
        if (!methodTraverse.MethodExists())
            throw new Exceptions.MissingStaticMethod(type, methodName);
    }

    public static void AssertInstanceMethod(Type type, string methodName)
    {
        var methodTraverse = Traverse.Create(instance).Method(methodName);
        if (!methodTraverse.MethodExists())
            throw new Exceptions.MissingInstanceMethod(instance, methodName);
    }
    */
}
