using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;

namespace UniTAS.Patcher.Extensions;

public static class MethodBaseExtensions
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public static bool IsExtern(this MethodBase methodBase)
    {
        return (methodBase.GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) != 0;
    }

    // https://github.com/pardeike/Harmony/blob/6af255873a00640e847f7531e0dd8a74e95b5a43/Harmony/Tools/AccessTools.cs#L1539
    public static TDelegateType MethodDelegate<TDelegateType>(
        this MethodInfo method,
        object instance = null,
        bool virtualCall = true,
        Type[] delegateArgs = null)
        where TDelegateType : Delegate
    {
        var delegateType = typeof(TDelegateType);

        // Static method delegate
        if (method.IsStatic)
        {
            return (TDelegateType)Delegate.CreateDelegate(delegateType, method);
        }

        var declaringType = method.DeclaringType;
        if (declaringType is { IsInterface: true } && !virtualCall)
        {
            throw new ArgumentException("Interface methods must be called virtually");
        }

        // Open instance method delegate ...
        if (instance is null)
        {
            var delegateParameters = delegateType.GetMethod("Invoke")!.GetParameters();
            if (delegateParameters.Length == 0)
            {
                // Following should throw an ArgumentException with the proper message string.
                _ = Delegate.CreateDelegate(typeof(TDelegateType), method);
                // But in case it doesn't...
                throw new ArgumentException("Invalid delegate type");
            }

            var delegateInstanceType = delegateParameters[0].ParameterType;
            // Exceptional case: delegate struct instance type cannot be created from an interface method.
            // This case is handled in the "non-virtual call" case, using the struct method and the matching delegate instance type.
            if (declaringType is { IsInterface: true } && delegateInstanceType.IsValueType)
            {
                var interfaceMapping = delegateInstanceType.GetInterfaceMap(declaringType);
                method = interfaceMapping.TargetMethods[Array.IndexOf(interfaceMapping.InterfaceMethods, method)];
                declaringType = delegateInstanceType;
            }

            // ... that virtually calls ...
            if (declaringType != null && virtualCall)
            {
                // ... an interface method
                // If method is already an interface method, just create a delegate from it directly.
                if (declaringType.IsInterface)
                {
                    return (TDelegateType)Delegate.CreateDelegate(delegateType, method);
                }

                // delegate interface instance type requires interface method.
                if (delegateInstanceType.IsInterface)
                {
                    var interfaceMapping = declaringType.GetInterfaceMap(delegateInstanceType);
                    var interfaceMethod =
                        interfaceMapping.InterfaceMethods[Array.IndexOf(interfaceMapping.TargetMethods, method)];
                    return (TDelegateType)Delegate.CreateDelegate(delegateType, interfaceMethod);
                }

                // ... a class instance method
                // Exceptional case: struct instance methods actually have their internal instance parameter passed by ref,
                // and thus are incompatible with typical non-ref-instance delegates
                // (delegate type must be: delegate <return type> MyStructDelegate(ref MyStruct instance, ...)),
                // so for struct instance methods, instead always use DynamicMethodDefinition approach,
                // so that typical non-ref-instance delegates work.
                if (!declaringType.IsValueType)
                {
                    return (TDelegateType)Delegate.CreateDelegate(delegateType, method.GetBaseDefinition());
                }
            }

            // ... that non-virtually calls
            var parameters = method.GetParameters();
            var numParameters = parameters.Length;
            var parameterTypes = new Type[numParameters + 1];
            parameterTypes[0] = declaringType;
            for (var i = 0; i < numParameters; i++)
                parameterTypes[i + 1] = parameters[i].ParameterType;
            // special handling for Func
            var delegateArgsResolved = delegateArgs;
            if (delegateArgsResolved == null && delegateType.IsGenericType && delegateType.GetGenericTypeDefinition()
                    .SaneFullName().StartsWith("System.Func`"))
            {
                var args = delegateType.GetGenericArguments().ToList();
                args.RemoveAt(args.Count - 1); // return
                delegateArgsResolved = args.ToArray();
            }
            else if (delegateArgsResolved == null)
            {
                delegateArgsResolved = delegateType.GetGenericArguments();
            }

            var dynMethodReturn = delegateArgsResolved.Length < parameterTypes.Length
                ? parameterTypes
                : delegateArgsResolved;
            var dmd = new DynamicMethodDefinition(
                "OpenInstanceDelegate_" + method.Name,
                method.ReturnType,
                dynMethodReturn)
            {
                // OwnerType = declaringType
            };
            var ilGen = dmd.GetILGenerator();
            LocalBuilder valueTypeHolder = null;
            if (declaringType is { IsValueType: true } && !delegateArgsResolved[0].IsByRef &&
                (declaringType == delegateArgsResolved[0] || delegateArgsResolved[0].IsSubclassOf(declaringType)))
                ilGen.Emit(OpCodes.Ldarga_S, 0);
            // if `ref object` for instance, you would want a return
            else if (declaringType is { IsValueType: true } &&
                     !(delegateArgsResolved[0].GetElementType() ?? delegateArgsResolved[0]).IsValueType)
            {
                if (delegateArgsResolved[0].IsByRef)
                {
                    valueTypeHolder = ilGen.DeclareLocal(declaringType);

                    // unbox and store to temp
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldind_Ref);
                    ilGen.Emit(OpCodes.Unbox_Any, declaringType);
                    ilGen.Emit(OpCodes.Stloc, valueTypeHolder);
                    ilGen.Emit(OpCodes.Ldloca, valueTypeHolder);
                }
                else
                {
                    var tempHolder = ilGen.DeclareLocal(declaringType);
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Unbox_Any, declaringType);
                    ilGen.Emit(OpCodes.Stloc, tempHolder);
                    ilGen.Emit(OpCodes.Ldloca, tempHolder);
                }
            }
            else
                ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = 1; i < parameterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i);
                // unbox to make il code valid
                if (parameterTypes[i].IsValueType && i < delegateArgsResolved.Length &&
                    !delegateArgsResolved[i].IsValueType)
                {
                    ilGen.Emit(OpCodes.Unbox_Any, parameterTypes[i]);
                }
            }

            ilGen.Emit(OpCodes.Call, method);

            if (valueTypeHolder != null)
            {
                // replace ref
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldloc, valueTypeHolder);
                ilGen.Emit(OpCodes.Box, declaringType);
                ilGen.Emit(OpCodes.Stind_Ref);
            }

            ilGen.Emit(OpCodes.Ret);
            return dmd.Generate().CreateDelegate<TDelegateType>();
        }

        // Closed instance method delegate that virtually calls
        if (virtualCall)
        {
            return (TDelegateType)Delegate.CreateDelegate(delegateType, instance, method.GetBaseDefinition());
        }

        // Closed instance method delegate that non-virtually calls
        // It's possible to create a delegate to a derived class method bound to a base class object,
        // but this has undefined behavior, so disallow it.
        if (declaringType != null && !declaringType.IsInstanceOfType(instance))
        {
            // Following should throw an ArgumentException with the proper message string.
            _ = Delegate.CreateDelegate(typeof(TDelegateType), instance, method);
            // But in case it doesn't...
            throw new ArgumentException("Invalid delegate type");
        }

        // Mono had a bug where it internally uses the equivalent of ldvirtftn when calling delegate constructor on a method pointer,
        // so as a workaround, manually create a dynamic method to create the delegate using ldftn rather than ldvirtftn.
        // See https://github.com/mono/mono/issues/19964
        if (AccessTools.IsMonoRuntime)
        {
            var dmd = new DynamicMethodDefinition(
                "LdftnDelegate_" + method.Name,
                delegateType,
                [typeof(object)])
            {
                // OwnerType = delegateType
            };
            var ilGen = dmd.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldftn, method);
            ilGen.Emit(OpCodes.Newobj, delegateType.GetConstructor([typeof(object), typeof(IntPtr)])!);
            ilGen.Emit(OpCodes.Ret);
            return (TDelegateType)dmd.Generate().Invoke(null, [instance]);
        }

        return (TDelegateType)Activator.CreateInstance(delegateType, instance,
            method.MethodHandle.GetFunctionPointer());
    }
}