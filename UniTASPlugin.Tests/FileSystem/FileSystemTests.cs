using System.Text;
using FluentAssertions;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;

namespace UniTASPlugin.Tests.FileSystem;

public class FileSystemTests
{
    [Fact]
    public void CreateDir()
    {
        var fileSystem = (GameEnvironment.InnerState.FileSystem.OsFileSystems.FileSystem)new WindowsFileSystem();

        fileSystem.DirectoryExists("C:/Path/To/Dir").Should().BeFalse();
        fileSystem.DirectoryExists("D:/Path/To/Dir").Should().BeFalse();

        fileSystem.CreateDir("C:\\Path/To\\Dir");
        fileSystem.DirectoryExists("C:/Path/To/Dir").Should().BeTrue();

        fileSystem.CreateDir("D:\\Path/To\\Dir");
        fileSystem.DirectoryExists("D:/Path/To/Dir").Should().BeTrue();
    }

    [Fact]
    public void Open()
    {
        var fileSystem = (GameEnvironment.InnerState.FileSystem.OsFileSystems.FileSystem)new WindowsFileSystem();
        const string fileContent = "Hello World!";
        var fileContentBytes = Encoding.UTF8.GetBytes(fileContent);

        fileSystem.FileExists("C:/Path/To/File.txt").Should().BeFalse();

        var openPtr = fileSystem.Open("C:\\Path/To\\File.txt", FileMode.Create, FileAccess.Write, FileShare.None,
            FileOptions.None);

        fileSystem.FileExists("C:/Path/To/File.txt").Should().BeTrue();

        var writtenBytes = fileSystem.Write(openPtr, fileContentBytes, 0, fileContentBytes.Length);
        writtenBytes.Should().Be(fileContentBytes.Length);

        fileSystem.Close(openPtr);

        openPtr = fileSystem.Open("C:\\Path/To\\File.txt", FileMode.Open, FileAccess.Read, FileShare.None,
            FileOptions.None);

        var readBytes = new byte[fileContentBytes.Length];
        var readBytesCount = fileSystem.Read(openPtr, readBytes, 0, readBytes.Length);
        readBytesCount.Should().Be(fileContentBytes.Length);
        readBytes.Should().BeEquivalentTo(fileContentBytes);

        fileSystem.Close(openPtr);
    }

    [Fact]
    public void GetFileFail()
    {
        // ReSharper disable StringLiteralTypo
        const string path = "\\home\\yuu0141\\UnityTestClients\\TASTest\\build\\build_Data\\TextFile.txt";
        const string path2 = "C:\\TextFile.txt";
        // ReSharper restore StringLiteralTypo

        var fileSystem = (GameEnvironment.InnerState.FileSystem.OsFileSystems.FileSystem)new WindowsFileSystem();

        fileSystem.GetFile(path).Should().BeNull();
        fileSystem.GetFile(path2).Should().BeNull();
    }

    [Fact]
    public void GetDirFail()
    {
        // ReSharper disable StringLiteralTypo
        const string path = "\\home\\yuu0141\\UnityTestClients\\TASTest\\build\\build_Data\\";
        const string path2 = "\\home\\yuu0141\\UnityTestClients\\TASTest\\build\\build_Data";
        const string path3 = "C:\\TextFile.txt";
        // ReSharper restore StringLiteralTypo

        var fileSystem = (GameEnvironment.InnerState.FileSystem.OsFileSystems.FileSystem)new WindowsFileSystem();

        fileSystem.GetDir(path).Should().BeNull();
        fileSystem.GetDir(path2).Should().BeNull();
        fileSystem.GetDir(path3).Should().BeNull();
    }
}