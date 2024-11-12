using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class SteamAPIPatch : PreloadPatcher
{
    private static readonly HashSet<string> SkipSteamMethods =
    [
        "SteamAPI_Init",
        "SteamAPI_GetHSteamUser",
        "SteamAPI_GetHSteamPipe",
        "SteamInternal_CreateInterface",
        // "ISteamClient_GetISteamUser",
        // "ISteamClient_GetISteamFriends"
    ];

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();
        foreach (var type in types)
        {
            foreach (var method in type.Methods)
            {
                if ((method.Attributes & MethodAttributes.PInvokeImpl) == 0 || method.PInvokeInfo == null) continue;
                if (method.PInvokeInfo.Module.Name is not ("steam_api" or "steam_api64")) continue;

                StaticLogger.LogDebug($"Found steam API use in method `{method.FullName}`");

                if (SkipSteamMethods.Contains(method.Name))
                {
                    StaticLogger.LogDebug("Method required, skipping");
                    continue;
                }

                method.PInvokeInfo = null;
                method.Attributes &= ~MethodAttributes.PInvokeImpl;

                method.Body = new MethodBody(method);
                var il = method.Body.GetILProcessor();
                ILCodeUtils.ReturnDefaultValueOnMethod(method, il);
            }
        }
    }
}