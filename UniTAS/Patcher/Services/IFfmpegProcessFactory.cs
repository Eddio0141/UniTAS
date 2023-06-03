using System.Diagnostics;

namespace UniTAS.Patcher.Services;

public interface IFfmpegProcessFactory
{
    bool Available { get; }
    Process CreateFfmpegProcess();
}