using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Plugin.GameEnvironment.InnerState.FileSystem;

namespace UniTAS.Plugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class MonoIOPatchModule
{
    private static Type monoIOType;
    private static Type MonoIOType => monoIOType ??= AccessTools.TypeByName("System.IO.MonoIO");

    private static Type monoIOErrorType;
    private static Type MonoIOErrorType => monoIOErrorType ??= AccessTools.TypeByName("System.IO.MonoIOError");

    private static Type monoFileType;
    private static Type MonoFileType => monoFileType ??= AccessTools.TypeByName("System.IO.MonoFileType");

    private static Type monoIOStatType;
    private static Type MonoIOStatType => monoIOStatType ??= AccessTools.TypeByName("System.IO.MonoIOStat");

    private static readonly IFileSystemManager FileSystemManager =
        Plugin.Kernel.GetInstance<IFileSystemManager>();
}