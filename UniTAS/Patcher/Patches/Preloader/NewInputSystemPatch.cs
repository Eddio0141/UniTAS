using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class NewInputSystemPatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        if (assembly.Name.Name != "UnityEngine.InputModule") return;

        var nativeInputSystem = assembly.MainModule.Types.First(x => x.FullName == "UnityEngineInternal.Input.NativeInputSystem");
        var notifyDeviceDiscovered = nativeInputSystem.Methods.First(x => x.Name == "NotifyDeviceDiscovered");
        ILCodeUtils.HookHarmony(notifyDeviceDiscovered, prefix: AccessTools.Method(typeof(NewInputSystemPatch), nameof(NotifyDeviceDiscoveredPatch)));
    }

    private static IPatchReverseInvoker ReverseInvoker;

    public readonly struct NotifyDevice(int deviceId, string deviceDescriptor)
    {
        public readonly int DeviceId = deviceId;
        public readonly string DeviceDescriptor = deviceDescriptor;
    }

    public static List<NotifyDevice> NotifyDeviceDiscovered = [];

    private static void NotifyDeviceDiscoveredPatch(int deviceId, string deviceDescriptor)
    {
        ReverseInvoker ??= ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();
        if (ReverseInvoker.Invoking) return;
        StaticLogger.Trace($"NotifyDeviceDiscovered, deviceId: {deviceId}, deviceDescriptor: {deviceDescriptor}");
        NotifyDeviceDiscovered.Add(new NotifyDevice(deviceId, deviceDescriptor));
    }
}
