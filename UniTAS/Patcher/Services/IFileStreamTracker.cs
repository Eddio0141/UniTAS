using System.IO;

namespace UniTAS.Patcher.Services;

public interface IFileStreamTracker
{
    void NewFileStream(FileStream fileStream);
    void CloseFileStream(FileStream fileStream);
}