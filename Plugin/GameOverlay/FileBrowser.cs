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
    string selectedFileText;
    Vector2 scrollPos;
    Rect defaultRect;
    Rect windowRect;
    bool opened;
    int id;
    string title;
    string finalPath;
    bool gotFinalPath;

    public FileBrowser(string currentDir, Rect windowRect, string title, int id)
    {
        this.currentDir = currentDir;
        currentDirText = currentDir;
        defaultRect = windowRect;
        this.windowRect = windowRect;
        this.title = title;
        this.id = id;
        dirChanged = true;
        scrollPos = new();
        opened = false;
        finalPath = "";
        gotFinalPath = true;
    }

    public void Open()
    {
        currentDirText = currentDir;
        windowRect = defaultRect;
        dirChanged = true;
        scrollPos = new();
        opened = true;
        finalPath = "";
        gotFinalPath = true;
    }

    public void GetFinalPath(ref string finalPath)
    {
        if (gotFinalPath)
            return;
        finalPath = this.finalPath;
        gotFinalPath = true;
    }

    public void Update()
    {
        if (!opened)
            return;

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
        {
            opened = false;
        }
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
                    selectedFileText = name;
                }
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        selectedFileText = GUILayout.TextField(selectedFileText);
        if (GUILayout.Button("Open", GUILayout.Width(50)))
        {
            var combinedPath = Path.Combine(currentDir, selectedFileText);
            if (File.Exists(combinedPath))
            {
                opened = false;
                gotFinalPath = false;
                finalPath = combinedPath;
            }
        }
        GUILayout.EndHorizontal();
    }
}
