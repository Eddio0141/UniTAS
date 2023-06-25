using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.GUI;

[RegisterAll]
public interface IMainMenuTab
{
    void Render();
    string Name { get; }
}