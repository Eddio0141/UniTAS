using System.Diagnostics;

namespace UniTAS.Plugin.FFMpeg;

public interface IFfmpegProcessFactory
{
    bool Available { get; }
    Process CreateFfmpegProcess();
}