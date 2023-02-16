using UnityEngine;

namespace UniTASPlugin.GUI.FileBrowser;

public class FileBrowser : Window
{
    protected override Rect DefaultWindowRect { get; }
    protected override string WindowName { get; }

    public FileBrowser(string windowName)
    {
        WindowName = windowName;

        // for now the width and height are hardcoded
        const int width = 1000;
        const int height = 500;

        // get the screen size
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        // calculate the position of the window
        var x = (screenWidth - width) / 2;
        var y = (screenHeight - height) / 2;

        // set the window position
        DefaultWindowRect = new(x, y, width, height);
    }
}