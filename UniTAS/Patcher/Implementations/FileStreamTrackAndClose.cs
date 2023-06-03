using System.Collections.Generic;
using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class FileStreamTrackAndClose : IFileStreamTracker, IOnPreGameRestart
{
    private readonly List<FileStream> _openedFileStreams = new();
    private readonly ILogger _logger;

    private bool _disposing;

    public FileStreamTrackAndClose(ILogger logger)
    {
        _logger = logger;
    }

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
        _logger.LogDebug("Closing all open file streams");
        foreach (var fileStream in _openedFileStreams)
        {
            _logger.LogDebug($"Closing file stream: {fileStream.Name}");
            fileStream.Dispose();
        }

        _disposing = false;

        _openedFileStreams.Clear();
    }
}