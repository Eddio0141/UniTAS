using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.GUI;

[RegisterAll]
public interface IMainMenuTab
{
    void Render(int windowID);
    string Name { get; }
}