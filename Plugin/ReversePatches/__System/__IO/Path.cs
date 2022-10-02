using UniTASPlugin.FakeGameState.GameFileSystem;

namespace UniTASPlugin.ReversePatches.__System.__IO;

public static class Path
{
    public static char DirectorySeparatorChar { get => FileSystem.ExternalHelpers.DirectorySeparatorChar; }
}
