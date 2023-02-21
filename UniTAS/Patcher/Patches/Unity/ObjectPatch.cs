using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using UniTAS.Patcher.PatcherUtils;

namespace UniTAS.Patcher.Patches.Unity;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ObjectPatch
{
    private static bool _dontDestroyOnLoadPatch;

    [Patcher]
    public static void DontDestroyOnLoadPatch(AssemblyDefinition assembly)
    {
        if (_dontDestroyOnLoadPatch) return;

        // Find the method, `public static extern void DontDestroyOnLoad(Object);`
        var method = assembly.MainModule.GetType("UnityEngine.Object").FindMethod("DontDestroyOnLoad");

        if (method == null) return;

        _dontDestroyOnLoadPatch = true;

        // this patch inserts the following
        // if (obj is not GameObject gameObject) return;
        // if (gameObject.transform.parent != null) return;
        // UniTAS.Patcher.Tracker.DontDestroyOnLoadObjects.Add(gameObject);

        var processor = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions;

        var returnLabel = Instruction.Create(OpCodes.Ret);
        var returnLabel2 = Instruction.Create(OpCodes.Ret);

        var gameObject = new VariableDefinition(assembly.MainModule.GetType("UnityEngine.GameObject"));
        method.Body.Variables.Add(gameObject);

        var isGameObject = Instruction.Create(OpCodes.Isinst, assembly.MainModule.GetType("UnityEngine.GameObject"));
        var storeGameObject = Instruction.Create(OpCodes.Stloc, gameObject);
        var loadGameObject = Instruction.Create(OpCodes.Ldloc, gameObject);
        var loadDontDestroyOnLoadObjects = Instruction.Create(OpCodes.Ldsfld,
            assembly.MainModule.GetType("UniTAS.Patcher.Tracker").FindField("DontDestroyOnLoadObjects"));
        var loadGameObject2 = Instruction.Create(OpCodes.Ldloc, gameObject);
        var addGameObject = Instruction.Create(OpCodes.Callvirt,
            assembly.MainModule.GetType("System.Collections.Generic.List`1").FindMethod("Add"));
        var loadTransform = Instruction.Create(OpCodes.Callvirt,
            assembly.MainModule.GetType("UnityEngine.GameObject").FindMethod("get_transform"));
        var loadParent = Instruction.Create(OpCodes.Callvirt,
            assembly.MainModule.GetType("UnityEngine.Transform").FindMethod("get_parent"));
        var loadNull = Instruction.Create(OpCodes.Ldnull);
        var compareParent = Instruction.Create(OpCodes.Ceq);
        var branchIfTrue = Instruction.Create(OpCodes.Brtrue, returnLabel2);

        instructions.Insert(0, isGameObject);
        instructions.Insert(1, storeGameObject);
        instructions.Insert(2, loadGameObject);
        instructions.Insert(3, Instruction.Create(OpCodes.Brfalse, returnLabel));
        instructions.Insert(4, loadTransform);
        instructions.Insert(5, loadParent);
        instructions.Insert(6, loadNull);
        instructions.Insert(7, compareParent);
        instructions.Insert(8, branchIfTrue);
        instructions.Insert(9, loadDontDestroyOnLoadObjects);
        instructions.Insert(10, loadGameObject2);
        instructions.Insert(11, addGameObject);
        instructions.Insert(12, returnLabel2);
        instructions.Insert(13, returnLabel);
    }
}