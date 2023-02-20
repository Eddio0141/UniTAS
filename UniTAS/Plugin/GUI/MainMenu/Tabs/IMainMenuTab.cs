namespace UniTAS.Plugin.GUI.MainMenu.Tabs;

public interface IMainMenuTab
{
    void Render(int windowID);
    string Name { get; }
}