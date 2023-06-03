using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.TASRenderer;

[RegisterAll]
public abstract class AudioRenderer : Renderer
{
    public const string OutputPath = "temp.wav";
}