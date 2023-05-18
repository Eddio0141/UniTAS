using System.IO;

namespace UniTAS.Plugin.Services;

public interface IFileStreamTracker
{
    void NewFileStream(FileStream fileStream);
    void CloseFileStream(FileStream fileStream);
}