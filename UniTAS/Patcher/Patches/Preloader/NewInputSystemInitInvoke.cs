using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Services.Invoker;
using UniTAS.Patcher.Utils;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Preloader;

public class NewInputSystemInitInvoke : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => new[] { "Unity.InputSystem.dll" };

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes());

        foreach (var type in types)
        {
            ILCodeUtils.MethodInvokeHookOnCctor(assembly, type,
                typeof(NewInputSystemInitInvoke).GetMethod(nameof(OnNewInputSystemInit)));
        }
    }

    private static bool _invoked;
    private static bool _checkingInit;

    public static void OnNewInputSystemInit()
    {
        if (_invoked) return;
        if (!IsNewInputSystemActuallyInit()) return;
        _invoked = true;

        StaticLogger.Log.LogDebug("New Input System has been initialized");

        InvokeEventAttributes.Invoke<InvokeOnNewInputSystemInitAttribute>();
    }

    private static bool IsNewInputSystemActuallyInit()
    {
        if (_checkingInit) return false;
        _checkingInit = true;
        try
        {
            var check = InputSystem.settings != null;
            return check;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _checkingInit = false;
        }
    }
}