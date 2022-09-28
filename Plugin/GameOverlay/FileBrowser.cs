using System.IO;
using UnityEngine;

namespace UniTASPlugin.GameOverlay;

public class FileBrowser
{
    string currentDir;
    string currentDirText;
    bool dirChanged;
    string[] currentDirPaths;
    string[] displayNames;
    string selectedPath;
    string selectedName;
    bool selected;
    Vector2 scrollPos;
    Rect windowRect;
    bool opened;
    int id;
    string title;

    public FileBrowser(string currentDir, Rect windowRect, string title, int id)
    {
        this.currentDir = currentDir;
        currentDirText = currentDir;
        this.windowRect = windowRect;
        this.title = title;
        this.id = id;
        dirChanged = true;
        selected = false;
        scrollPos = new();
        opened = true;
    }

    public FileBrowser()
    {
        opened = false;
    }

    public bool Update(out string path)
    {
        path = "";
        if (!opened)
            return false;

        if (dirChanged && Directory.Exists(currentDir))
        {
            currentDirPaths = Directory.GetFileSystemEntries(currentDir, "*");
            displayNames = new string[currentDirPaths.Length];
            for (int i = 0; i < currentDirPaths.Length; i++)
            {
                var currentPath = currentDirPaths[i];
                var name = Path.GetFileName(currentPath);
                displayNames[i] = name;
            }
            currentDirText = currentDir;
        }
        dirChanged = false;

        windowRect = GUILayout.Window(id, windowRect, Window, title, GUI.skin.window);
        return true;
    }

    void Window(int id)
    {
        GUI.DragWindow(new Rect(0, 0, 20000, 20));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("^", GUILayout.Width(20)))
        {
            currentDir = Path.GetDirectoryName(currentDir);
            dirChanged = true;
        }
        currentDirText = GUILayout.TextField(currentDirText);
        if (GUILayout.Button("Go", GUILayout.Width(50)))
        {
            currentDir = currentDirText;
            dirChanged = true;
        }
        if (GUILayout.Button("x", GUILayout.Width(20)))
            opened = false;
        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);
        for (int i = 0; i < currentDirPaths.Length; i++)
        {
            var path = currentDirPaths[i];
            var name = displayNames[i];
            if (GUILayout.Button(name))
            {
                if (Directory.Exists(path))
                {
                    currentDir = path;
                    dirChanged = true;
                }
                else
                {
                    selectedPath = path;
                    selectedName = name;
                    selected = true;
                }
            }
        }
        GUILayout.EndScrollView();
    }
}
