using UniTAS.Patcher.Implementations.GUI;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations;

[Register]
public class InitGUI
{
    public InitGUI(IWindowFactory windowFactory)
    {
        windowFactory.Create<MainMenu>().Show();
    }
}