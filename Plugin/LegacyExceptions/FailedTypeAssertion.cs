using System;

namespace UniTASPlugin.Exceptions;

internal class MissingSomethingStatic : Exception
{
    public MissingSomethingStatic(Type type_, string name, string missingItem) : base(
        $"Failed type get assertion, missing static {type_.FullName}.{name} {missingItem}")
    {
    }
}

internal class MissingSomethingInstance : Exception
{
    public MissingSomethingInstance(Type type_, string name, string missingItem) : base(
        $"Failed type get assertion, missing instance {type_.FullName}.{name} {missingItem}")
    {
    }
}

internal class MissingStaticField : MissingSomethingStatic
{
    public MissingStaticField(Type type_, string fieldName) : base(type_, fieldName, "field")
    {
    }
}

internal class MissingInstanceField : MissingSomethingInstance
{
    public MissingInstanceField(Type type_, string fieldName) : base(type_, fieldName, "field")
    {
    }
}

internal class MissingStaticGetter : MissingSomethingStatic
{
    public MissingStaticGetter(Type type_, string getterName) : base(type_, getterName, "getter")
    {
    }
}

internal class MissingInstanceGetter : MissingSomethingInstance
{
    public MissingInstanceGetter(Type type_, string getterName) : base(type_, getterName, "getter")
    {
    }
}

internal class MissingStaticSetter : MissingSomethingStatic
{
    public MissingStaticSetter(Type type_, string setterName) : base(type_, setterName, "setter")
    {
    }
}

internal class MissingInstanceSetter : MissingSomethingInstance
{
    public MissingInstanceSetter(Type type_, string setterName) : base(type_, setterName, "setter")
    {
    }
}

internal class MissingStaticMethod : MissingSomethingStatic
{
    public MissingStaticMethod(Type type_, string methodName) : base(type_, methodName, "method")
    {
    }
}

internal class MissingInstanceMethod : MissingSomethingInstance
{
    public MissingInstanceMethod(Type type_, string methodName) : base(type_, methodName, "method")
    {
    }
}