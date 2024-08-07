using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Patch(IHarmony harmony) : TerminalCmd
{
    public override string Name => "patch";

    public override string Description => "patches a c# function via harmony for lua functions.\n " +
                                          "  arg0 (string): type name, arg1 (function): callback, arg2 (auto/method/getter/setter/constructor/staticconstructor) patch type, arg3 (prefix/postfix) patch point\n" +
                                          "  arg4 (string[]) method types, arg5 (string[]) method generics, arg6 (bool) method is directly declared\n\n" +
                                          "callback args: the arguments will be supplied as necessary\n" +
                                          "  arg0 (any)? this value in a non-static call (same as __instance in patches)\n" +
                                          "  arg1 (any[])? arguments supplied to actual function (same as __args)\n" +
                                          "  arg2 (any)? return value (__result) note: modifying this argument won't change the return value (see the return table for callback)\n\n" +
                                          "  return (bool): to execute original method or not in prefix patches |\n" +
                                          "    (table): { result (any): modified return value, ret (bool): to execute original method or not in prefix patches }";

    public override Delegate Callback => Execute;

    private enum PatchType
    {
        Auto,
        Method,
        Getter,
        Setter,
        Constructor,

        // Enumerator, TODO: implement this (https://harmony.pardeike.net/api/HarmonyLib.MethodType.html)
        StaticConstructor,
    }

    private enum PatchPoint
    {
        Prefix,
        Postfix
    }

    private void Execute(Script script, string target, Closure callback, string patchType = "Auto",
        string patchPoint = "Prefix", string[] args = null, string[] generics = null,
        bool declared = false)
    {
        var print = script.Options.DebugPrint;

        PatchType patchTypeEnum;
        try
        {
            patchTypeEnum = (PatchType)Enum.Parse(typeof(PatchType), patchType, true);
        }
        catch (Exception)
        {
            print($"failed to parse {patchType} into PatchType");
            return;
        }

        PatchPoint patchPointEnum;
        try
        {
            patchPointEnum = (PatchPoint)Enum.Parse(typeof(PatchPoint), patchPoint, true);
        }
        catch (Exception)
        {
            print($"failed to parse {patchPoint} into PatchPoint");
            return;
        }

        // parse types
        var targetTypeRawSepIndex = target.LastIndexOf('.');

        var targetTypeRaw = target.Substring(0, targetTypeRawSepIndex);
        var targetTypeMethodRaw = targetTypeRawSepIndex < 0 ? null : target.Substring(targetTypeRawSepIndex + 1);

        var targetType = FindTypeFully(targetTypeRaw);
        if (targetType == null)
        {
            // maybe we got the type separated in a weird wrong way
            // try again!
            targetTypeRaw = target;
            targetTypeMethodRaw = null;
            targetType = FindTypeFully(targetTypeRaw);

            if (targetType == null)
            {
                print($"failed to find target type {targetTypeRaw}");
                return;
            }
        }

        string patchMsg = null;
        Type returnType = null;
        var targetTypeMethod = patchTypeEnum switch
        {
            // literally tries everything
            PatchType.Auto => PatchGetter(targetType, targetTypeMethodRaw, declared, out returnType) ??
                              PatchSetter(targetType, targetTypeMethodRaw, declared) ??
                              PatchMethod(targetType, targetTypeMethodRaw, declared, args, generics, out patchMsg,
                                  out returnType) ??
                              (targetTypeMethodRaw == null
                                  ? null
                                  : PatchConstructor(targetType, args, declared, false, out patchMsg) ??
                                    PatchConstructor(targetType, args, declared, true, out patchMsg)) ??
                              PatchGetter(targetType, targetTypeMethodRaw, !declared, out returnType) ??
                              PatchSetter(targetType, targetTypeMethodRaw, !declared) ??
                              PatchMethod(targetType, targetTypeMethodRaw, !declared, args, generics, out patchMsg,
                                  out returnType) ??
                              (targetTypeMethodRaw == null
                                  ? null
                                  : PatchConstructor(targetType, args, !declared, false, out patchMsg) ??
                                    PatchConstructor(targetType, args, !declared, true, out patchMsg)),
            PatchType.Method => PatchMethod(targetType, targetTypeMethodRaw, declared, args, generics, out patchMsg,
                out returnType),
            PatchType.Getter => PatchGetter(targetType, targetTypeMethodRaw, declared, out returnType),
            PatchType.Setter => PatchSetter(targetType, targetTypeMethodRaw, declared),
            PatchType.Constructor => PatchConstructor(targetType, args, declared, false, out patchMsg),
            PatchType.StaticConstructor => PatchConstructor(targetType, args, declared, true, out patchMsg),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (targetTypeMethod == null)
        {
            if (patchMsg != null)
            {
                print(patchMsg);
                return;
            }

            print($"failed to find method {targetTypeRaw}.{targetTypeMethodRaw}");
            return;
        }

        var canReturn = returnType != null && returnType != typeof(void);
        PatchFactory.CanReturnPending = canReturn;
        PatchFactory.IsPrefixPending = patchPointEnum == PatchPoint.Prefix;

        var patchMethodFactory = AccessTools.Method(typeof(PatchFactory), nameof(PatchFactory.Get));

        print($"patching method {targetType.SaneFullName()}.{targetTypeMethod.Name}");

        var finializer = new HarmonyMethod(AccessTools.Method(typeof(PatchFactory), nameof(PatchFactory.Finalizer)));
        switch (patchPointEnum)
        {
            case PatchPoint.Prefix:
                harmony.Harmony.Patch(targetTypeMethod, prefix: new(patchMethodFactory), finalizer: finializer);
                break;
            case PatchPoint.Postfix:
                harmony.Harmony.Patch(targetTypeMethod, postfix: new(patchMethodFactory), finalizer: finializer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _patchClosureIndex++;
        PatchClosures.Add(callback);
        PatchCanReturn.Add(canReturn);
        PatchIsPrefix.Add(patchPointEnum == PatchPoint.Prefix);

        print("done!");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class PatchFactory
    {
        public static bool CanReturnPending;
        public static bool IsPrefixPending;

        public static MethodInfo Get(MethodBase targetTypeMethod)
        {
            StaticLogger.LogDebug(
                $"get is invoked, target is {targetTypeMethod.DeclaringType.SaneFullName()}.{targetTypeMethod.Name}");

            // https://harmony.pardeike.net/articles/patching-injections.html
            // args in order:
            // object __instance, object[] __args
            // return table:
            // result, return
            var patchMethodArgs = new List<Type>();
            var argNames = new List<string>();
            var hasInstance = !targetTypeMethod.IsStatic;
            if (hasInstance)
            {
                patchMethodArgs.Add(typeof(object));
                argNames.Add("__instance");
            }

            var hasArgs = targetTypeMethod.GetParameters().Length > 0;
            if (hasArgs)
            {
                patchMethodArgs.Add(typeof(object[]));
                argNames.Add("__args");
            }

            if (CanReturnPending)
            {
                patchMethodArgs.Add(typeof(object).MakeByRefType());
                argNames.Add("__result");
            }

            var dmd = new DynamicMethodDefinition("LuaPatchEntry", IsPrefixPending ? typeof(bool) : typeof(void),
                patchMethodArgs.ToArray());
            dmd.Definition.IsStatic = true;
            for (var i = 0; i < dmd.Definition.Parameters.Count; i++)
            {
                var param = dmd.Definition.Parameters[i];
                param.Name = argNames[i];
            }

            var patchBody = AccessTools.Method(typeof(LiveScripting), nameof(PatchBody));

            var il = dmd.GetILGenerator();

            // load args for PatchBody call
            il.Emit(OpCodes.Ldc_I4, _patchClosureIndex);
            var argIndex = 0;
            if (hasInstance)
            {
                il.Emit(OpCodes.Ldarg_0);
                argIndex++;
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }

            if (hasArgs)
            {
                il.Emit(OpCodes.Ldarg, argIndex);
                argIndex++;
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }

            if (CanReturnPending)
            {
                il.Emit(OpCodes.Ldarg, argIndex);
                // argIndex++;
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }

            // call and return, discard return value if not Prefix
            il.Emit(OpCodes.Call, patchBody);
            if (!IsPrefixPending)
                il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            return dmd.Generate();
        }

        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
                StaticLogger.Log.LogWarning($"Exception occured in lua interpreter patch: {__exception}");
            return null;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool PatchBody(int closureIndex, object __instance, object[] __args,
        ref object __result)
    {
        var closure = PatchClosures[closureIndex];
        var args = new List<object>();
        if (__instance != null)
        {
            args.Add(__instance);
        }

        if (__args != null)
        {
            args.Add(__args);
        }

        if (PatchCanReturn[closureIndex])
        {
            args.Add(__result);
        }

        var ret = closure.Call(args.ToArray());

        if (ret.Type == DataType.Table)
        {
            var table = ret.Table;
            if (PatchCanReturn[closureIndex])
            {
                var result = table.RawGet("result");
                if (result != null)
                    __result = result.ToObject(__result.GetType());
            }

            if (PatchIsPrefix[closureIndex])
            {
                var returning = table.RawGet("ret");
                if (returning?.Type is DataType.Boolean)
                    return returning.Boolean;
            }
        }

        if (!PatchIsPrefix[closureIndex]) return true;

        return ret.Type != DataType.Boolean || ret.Boolean;
    }

    private static int _patchClosureIndex;

    private static readonly List<Closure> PatchClosures = [];
    private static readonly List<bool> PatchCanReturn = [];
    private static readonly List<bool> PatchIsPrefix = [];

    private static Type FindTypeFully(string type)
    {
        var targetType = AccessTools.TypeByName(type);
        if (targetType != null) return targetType;

        // could be somewhere else, and TypeByName might not be able to find it
        return AccessTools.AllAssemblies()
            .SelectMany(AccessTools.GetTypesFromAssembly).FirstOrDefault(x => x.SaneFullName() == type);
    }

    private static MethodBase PatchMethod(Type type, string target, bool declared, string[] argsRaw,
        string[] genericsRaw, out string message, out Type returnType)
    {
        var args = RawTypes(argsRaw, out var argsRawMsg);
        if (argsRawMsg != null)
        {
            message = argsRawMsg;
            returnType = null;
            return null;
        }

        var generics = RawTypes(genericsRaw, out var genericsRawMsg);
        if (genericsRawMsg != null)
        {
            message = genericsRawMsg;
            returnType = null;
            return null;
        }

        message = null;
        var method = declared
            ? AccessTools.DeclaredMethod(type, target, args, generics)
            : AccessTools.Method(type, target, args, generics);
        returnType = method?.ReturnType;
        return method;
    }

    private static Type[] RawTypes(string[] typesRaw, out string message)
    {
        if (typesRaw == null)
        {
            message = null;
            return null;
        }

        var types = new List<Type>(typesRaw.Length);
        foreach (var typeRaw in typesRaw)
        {
            var t = FindTypeFully(typeRaw);
            if (t == null)
            {
                message = $"failed to find argument type {typeRaw}";
                return null;
            }

            types.Add(t);
        }

        message = null;
        return types.ToArray();
    }

    private static MethodBase PatchGetter(Type type, string target, bool declared, out Type returnType)
    {
        var method = declared
            ? AccessTools.DeclaredPropertyGetter(type, target)
            : AccessTools.PropertyGetter(type, target);
        returnType = method?.ReturnType;
        return method;
    }

    private static MethodBase PatchSetter(Type type, string target, bool declared)
    {
        return declared ? AccessTools.DeclaredPropertySetter(type, target) : AccessTools.PropertySetter(type, target);
    }

    private static MethodBase PatchConstructor(Type type, string[] argsRaw, bool declared, bool staticConstructor,
        out string message)
    {
        var args = RawTypes(argsRaw, out var argsMsg);
        if (argsMsg != null)
        {
            message = argsMsg;
            return null;
        }

        message = null;
        return declared
            ? AccessTools.DeclaredConstructor(type, args, staticConstructor)
            : AccessTools.Constructor(type, args, staticConstructor);
    }
}