using System.Diagnostics.CodeAnalysis;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch(true)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class PathPatchModule
{
    private static readonly IReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<IReverseInvokerFactory>();

    private static readonly IFileSystemManager FileSystemManager =
        Plugin.Kernel.GetInstance<IFileSystemManager>();
}