using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.TASRenderer;

[RegisterAll]
public abstract class AudioRenderer : Renderer
{
    public const string OutputPath = "temp.wav";
}