using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MoonSharp.Interpreter;

namespace UniTAS.Patcher.Implementations.Proxies;

[method: MoonSharpHidden]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class TraverseProxy(Traverse traverse)
{
    private static readonly AccessTools.FieldRef<object, MethodBase> TraverseMethodField =
        AccessTools.FieldRefAccess<MethodBase>(typeof(Traverse), "_method");

    private static readonly AccessTools.FieldRef<object, MemberInfo> TraverseInfoField =
        AccessTools.FieldRefAccess<MemberInfo>(typeof(Traverse), "_info");

    public object GetValue(params object[] args)
    {
        return args.Length == 0 ? traverse.GetValue() : traverse.GetValue(args);
    }

    public Traverse SetValue(DynValue value)
    {
        var memberInfo = TraverseInfoField(traverse);

        switch (memberInfo)
        {
            case FieldInfo f:
                return traverse.SetValue(value.ToObject(f.FieldType));
            case PropertyInfo p:
                return traverse.SetValue(value.ToObject(p.PropertyType));
        }

        var args = TraverseMethodField(traverse).GetParameters();
        var argsLength = args.Length;
        var argTypes = args.Select(x => x.ParameterType);

        object valueConverted;
        if (argsLength == 1)
        {
            valueConverted = value.ToObject(argTypes.First());
        }
        else if (value.Type is not (DataType.Table or DataType.UserData))
        {
            throw new Exception("Traverse has more than 1 arguments required, but didn't receive a table or UserData");
        }
        else if (value.Type is DataType.Table && value.Table.Length != argsLength)
        {
            throw new Exception($"Mismatch in traverse method argument count, expected {argsLength} arguments");
        }
        else
        {
            var convertedArray = new object[argsLength];
            var table = value.Table;
            var values = table.Values.GetEnumerator();
            var argTypesEnumerator = argTypes.GetEnumerator();
            for (var i = 0; i < table.Length; i++)
            {
                // wont fail
                values.MoveNext();
                argTypesEnumerator.MoveNext();

                var item = values.Current;
                var argType = argTypesEnumerator.Current;

                convertedArray[i] = item!.ToObject(argType);
            }

            values.Dispose();
            argTypesEnumerator.Dispose();

            valueConverted = convertedArray;
        }

        return traverse.SetValue(valueConverted);
    }

    public Traverse Field(string name) => traverse.Field(name);

    public Traverse Property(string name, object[] index = null) => traverse.Property(name, index);

    public Traverse Method(string name, params object[] arguments) => traverse.Method(name, arguments);

    public Traverse Method(string name, Type[] paramTypes, object[] arguments = null) =>
        traverse.Method(name, paramTypes, arguments);

    public bool PropertyExists() => traverse.PropertyExists();

    public bool MethodExists() => traverse.MethodExists();

    public bool TypeExists() => traverse.TypeExists();

    public Table Methods(Script script)
    {
        var list = traverse.Methods();
        return new Table(script, list.Select(x => DynValue.FromObject(script, x)).ToArray());
    }
}