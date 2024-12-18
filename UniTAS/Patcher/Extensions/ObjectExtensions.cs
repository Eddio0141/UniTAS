using System;
using System.Collections;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.Converters;

namespace UniTAS.Patcher.Extensions;

public static class ObjectExtensions
{
    public static DynValue ToDynValue(this object obj, Script script)
    {
        if (obj == null)
            return DynValue.Nil;

        DynValue v = null;

        if (obj is Type type)
            v = UserData.CreateStatic(type);

        // unregistered enums go as integers
        if (obj is Enum)
            // ReSharper disable once PossibleInvalidCastException
            return DynValue.NewNumber((int)obj);

        if (v != null) return v;

        if (obj is Delegate @delegate)
            return DynValue.NewCallback(CallbackFunction.FromDelegate(script, @delegate));

        if (obj is MethodInfo { IsStatic: true } mi)
        {
            return DynValue.NewCallback(CallbackFunction.FromMethodInfo(script, mi));
        }

        if (obj is IList list)
        {
            var converted = new DynValue[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var o = list[i];
                converted[i] = o.ToDynValue(script);
            }

            return DynValue.NewTable(script, converted);
        }

        var enumerator = ClrToScriptConversions.EnumerationToDynValue(script, obj);
        if (enumerator != null) return enumerator;

        return DynValue.FromObject(script, obj);
    }

    public static IntPtr Addr(this object obj)
    {
        var tr = __makeref(obj);
        unsafe
        {
#pragma warning disable CS8500
            var ptr = *(IntPtr*)(&tr);
#pragma warning restore CS8500
            return ptr;
        }
    }
}