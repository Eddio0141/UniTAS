using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.FileSystem;

namespace UniTAS.Plugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch(true)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class PathPatchModule
{
    private static readonly IFileSystemManager FileSystemManager =
        Plugin.Kernel.GetInstance<IFileSystemManager>();
}