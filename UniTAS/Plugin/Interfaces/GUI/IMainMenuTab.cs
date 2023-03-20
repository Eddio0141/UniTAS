namespace UniTAS.Plugin.Interfaces.GUI;

public interface IMainMenuTab
{
    void Render(int windowID);
    string Name { get; }
}