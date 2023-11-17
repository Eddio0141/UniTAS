using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
public class ShowHiddenWindow : Window
{
    public ShowHiddenWindow(WindowDependencies windowDependencies) : base(windowDependencies,
        new(windowName: "Show hidden"))
    {
    }

    protected override void OnGUI()
    {
    }
}