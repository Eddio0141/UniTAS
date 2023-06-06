using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace UniTAS.Patcher.Utils;

public static class ILCodeUtils
{
    public static void MethodInvokeHookOnCctor(AssemblyDefinition assembly, TypeDefinition type, MethodBase method)
    {
        if (type == null) return;

        AddCctorIfMissing(assembly, type);
        var staticCtor = type.Methods.First(m => m.IsConstructor && m.IsStatic);

        var invoke = assembly.MainModule.ImportReference(method);

        var firstInstruction = staticCtor.Body.Instructions.First();
        var ilProcessor = staticCtor.Body.GetILProcessor();

        // insert call before first instruction
        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, invoke));
        StaticLogger.Log.LogDebug(
            $"Added invoke hook to cctor of {type.FullName} invoking {method.DeclaringType?.FullName ?? "unknown"}.{method.Name}");
    }

    public static void AddCctorIfMissing(AssemblyDefinition assembly, TypeDefinition type)
    {
        var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
        if (staticCtor != null) return;

        StaticLogger.Log.LogDebug($"Adding cctor to {type.FullName}");
        staticCtor = new(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
            | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            assembly.MainModule.ImportReference(typeof(void)));

        type.Methods.Add(staticCtor);
        var il = staticCtor.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ret));
    }
}