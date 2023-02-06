using System.Diagnostics.CodeAnalysis;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem;

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch(true)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class PathPatchModule
{
    private static readonly IFileSystemManager FileSystemManager =
        Plugin.Kernel.GetInstance<IFileSystemManager>();
}