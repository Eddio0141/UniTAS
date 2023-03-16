using System.Diagnostics;

namespace UniTAS.Plugin.GameVideoRender;

public interface IFfmpegRunner
{
    bool Available { get; }
    Process FfmpegProcess { get; }
}