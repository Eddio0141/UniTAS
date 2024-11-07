using System.Collections.Generic;
using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class FileStreamTrackAndClose(ILogger logger) : IFileStreamTracker, IOnPreGameRestart
{
    private readonly HashSet<FileStream> _openedFileStreams = new();

    private bool _disposing;

    public void NewFileStream(FileStream fileStream)
    {
        _openedFileStreams.Add(fileStream);
    }

    public void CloseFileStream(FileStream fileStream)
    {
        if (_disposing) return;
        _openedFileStreams.Remove(fileStream);
    }

    public void OnPreGameRestart()
    {
        _disposing = true;
        logger.LogDebug("Closing all open file streams");
        foreach (var fileStream in _openedFileStreams)
        {
            logger.LogDebug($"Closing file stream: {fileStream.Name}");
            fileStream.Dispose();
        }

        _disposing = false;

        _openedFileStreams.Clear();
    }
}