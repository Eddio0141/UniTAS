using FluentAssertions;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;

namespace UniTASPlugin.Tests.FileSystem;

public class FileSystemTests
{
    [Fact]
    public void CreateDir()
    {
        var fileSystem = (GameEnvironment.InnerState.FileSystem.OsFileSystems.FileSystem)new WindowsFileSystem();

        fileSystem.CreateDir("C:\\Path/To\\Dir");
        fileSystem.DirectoryExists("C:/Path/To/Dir").Should().BeTrue();

        fileSystem.CreateDir("D:\\Path/To\\Dir");
        fileSystem.DirectoryExists("D:/Path/To/Dir").Should().BeTrue();
    }
}