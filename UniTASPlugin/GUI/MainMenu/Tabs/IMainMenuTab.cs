namespace UniTASPlugin.GUI.MainMenu.Tabs;

public interface IMainMenuTab
{
    void Render();
    string Name { get; }
}