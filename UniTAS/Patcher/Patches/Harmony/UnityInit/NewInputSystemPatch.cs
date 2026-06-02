using System;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine.InputSystem.LowLevel;
#if TRACE
using System.Text;
using UnityEngine.InputSystem;
using System.Linq;
#endif

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
public class NewInputSystemPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController = ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly IInputSystemState InputSystemState = ContainerStarter.Kernel.GetInstance<IInputSystemState>();

    private static readonly IPatchReverseInvoker ReverseInvoker = ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();
    private static readonly IVirtualEnvController VEnv = ContainerStarter.Kernel.GetInstance<IVirtualEnvController>();

    private static readonly ITimeEnv TimeEnv = ContainerStarter.Kernel.GetInstance<ITimeEnv>();
    private static readonly IMonoBehEventInvoker MonoBehEventInvoker = ContainerStarter.Kernel.GetInstance<IMonoBehEventInvoker>();

    private static readonly Type NativeInputSystem = AccessTools.TypeByName("UnityEngineInternal.Input.NativeInputSystem");

    private static readonly Func<IntPtr, InputEventBuffer> ToInputEventBuffer;

    private static readonly Action<InputEventPtr, double> inputEventPtrInternalTime;

    static NewInputSystemPatch()
    {
        if (!ContainerStarter.Kernel.GetInstance<IInputSystemState>().HasNewInputSystem)
        {
            return;
        }

        inputEventPtrInternalTime = AccessTools.MethodDelegate<Action<InputEventPtr, double>>(AccessTools.PropertySetter(typeof(InputEventPtr), "internalTime"));

        var dmd = new DynamicMethodDefinition("ToInputEventBuffer", typeof(InputEventBuffer), [typeof(IntPtr)]);
        var def = dmd.Definition;
        def.IsStatic = true;

        var body = def.Body;
        var il = dmd.GetILProcessor();

        var result = new VariableDefinition(il.Import(typeof(InputEventBuffer)));
        var eventBufferPtr = new VariableDefinition(il.Import(typeof(NativeInputEventBuffer).MakePointerType()));
        body.Variables.AddRange([result, eventBufferPtr]);

        // var eventBufferPtr = (NativeInputEventBuffer*)eventBuffer.ToPointer();
        il.Emit(OpCodes.Ldarga, def.Parameters[0]);
        il.Emit(OpCodes.Call, AccessTools.Method(typeof(IntPtr), nameof(IntPtr.ToPointer)));
        il.Emit(OpCodes.Stloc, eventBufferPtr);
        // InputEventBuffer eventBuffer = new InputEventBuffer((InputEvent*)eventBufferPtr->eventBuffer, eventBufferPtr->eventCount, eventBufferPtr->sizeInBytes, eventBufferPtr->capacityInBytes);
        il.Emit(OpCodes.Ldloca, result);
        il.Emit(OpCodes.Ldloc, eventBufferPtr);
        il.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(NativeInputEventBuffer), nameof(NativeInputEventBuffer.eventBuffer)));
        il.Emit(OpCodes.Ldloc, eventBufferPtr);
        il.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(NativeInputEventBuffer), nameof(NativeInputEventBuffer.eventCount)));
        il.Emit(OpCodes.Ldloc, eventBufferPtr);
        il.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(NativeInputEventBuffer), nameof(NativeInputEventBuffer.sizeInBytes)));
        il.Emit(OpCodes.Ldloc, eventBufferPtr);
        il.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(NativeInputEventBuffer), nameof(NativeInputEventBuffer.capacityInBytes)));
        il.Emit(OpCodes.Call, AccessTools.Constructor(typeof(InputEventBuffer), [typeof(InputEvent).MakePointerType(), typeof(int), typeof(int), typeof(int)]));
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);

        body.Optimize();

        ToInputEventBuffer = dmd.Generate().CreateDelegate<Func<IntPtr, InputEventBuffer>>();
    }

    [HarmonyPatch]
    private class get_currentTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.PropertyGetter(NativeInputSystem, "currentTime");

        private static bool Prefix(ref double __result)
        {
            if (ReverseInvoker.Invoking)
            {
                return true;
            }

            __result = TimeEnv.UnscaledTime;
            return false;
        }
    }

    [HarmonyPatch]
    private class get_currentTimeOffsetToRealtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.PropertyGetter(NativeInputSystem, "currentTimeOffsetToRealtimeSinceStartup");

        private static bool Prefix(ref double __result)
        {
            if (ReverseInvoker.Invoking)
            {
                return true;
            }

            __result = 0.0;
            return false;
        }
    }

    [HarmonyPatch]
    private class NotifyBeforeUpdate
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.Method(NativeInputSystem, "NotifyBeforeUpdate");

        private static bool Prefix(int updateType)
        {
            /*
                Dynamic = 1,
                Fixed = 2,
                BeforeRender = 4,
                IgnoreFocus = -2147483648, // 0x80000000
             */
            if ((updateType & 1) != 0)
            {
                MonoBehEventInvoker.InvokeUpdate(false);
            }
            else if ((updateType & 2) != 0)
            {
                MonoBehEventInvoker.InvokeFixedUpdate(false);
            }

            return !MonoBehaviourController.PausedExecution;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
    internal struct NativeInputEventBuffer
    {
        [FieldOffset(0)]
        public unsafe void* eventBuffer;

        [FieldOffset(8)]
        public int eventCount;

        [FieldOffset(12)]
        public int sizeInBytes;

        [FieldOffset(16)]
        public int capacityInBytes;
    }


    [HarmonyPatch]
    private class NotifyUpdate
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.Method(NativeInputSystem, "NotifyUpdate");

        private static void Prefix(IntPtr eventBuffer)
        {
            var iter = ToInputEventBuffer(eventBuffer).GetEnumerator();
            while (iter.MoveNext())
            {
                var inputEvent = iter.Current;
                inputEvent.id = InputSystemState.NewInputSystemEventId;
                inputEventPtrInternalTime(inputEvent, TimeEnv.UnscaledTime);

#if TRACE
                var info = new StringBuilder($"input event, event: {inputEvent}");
                var device = InputSystem.GetDeviceById(inputEvent.deviceId);
                if (device != null)
                {
                    info.Append($", device: {device.displayName}, buttons: {inputEvent.GetAllButtonPresses().Select(x => x.name).Join()}");
                }
                StaticLogger.Trace(info.ToString());
#endif
            }
        }
    }
}
