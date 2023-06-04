using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.GUI;

[RegisterAll]
public interface IMainMenuTab
{
    void Render(int windowID);
    string Name { get; }
}