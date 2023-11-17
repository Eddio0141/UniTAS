using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface IDropdownMenuFactory
{
    /// <summary>
    /// Creates a drop down window
    /// </summary>
    /// <param name="entries">A pair of name and the function it will execute when clicked on</param>
    /// <returns>The window</returns>
    T Create<T>(DropdownEntry[] entries) where T : Window;
}