using System.Diagnostics;

namespace UniTAS.Plugin.Services;

public interface IFfmpegProcessFactory
{
    bool Available { get; }
    Process CreateFfmpegProcess();
}