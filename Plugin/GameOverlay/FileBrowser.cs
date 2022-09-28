using System.IO;
using UnityEngine;

namespace UniTASPlugin.GameOverlay;

public class FileBrowser
{
    static string currentDir = Application.dataPath;
    static bool dirChanged = true;
    static string[] currentDirPaths = new string[0];
    static string[] displayNames = new string[0];

    static Texture2D BG = new(1, 1);

    static FileBrowser()
    {
        BG.SetPixel(0, 0, new(0, 0, 0, 0.75f));
        BG.Apply();
    }

    public static bool Open(ref Rect windowRect, string title, int id, bool open, out string path)
    {
        path = "";
        if (!open)
            return false;

        if (dirChanged && Directory.Exists(currentDir))
        {
            currentDirPaths = Directory.GetFiles(currentDir);
            displayNames = new string[currentDirPaths.Length];
            for (int i = 0; i < currentDirPaths.Length; i++)
            {
                var currentPath = currentDirPaths[i];
                var name = Path.GetFileName(currentPath);
                displayNames[i] = name;
            }
            dirChanged = false;
        }

        if (BG.width != windowRect.width || BG.height != windowRect.height)
        {
            BG.Resize((int)windowRect.width, (int)windowRect.height);
            BG.Apply();
        }

        windowRect = GUI.Window(id, windowRect, Inner, title, GUI.skin.window);

        return true;
    }

    static void Inner(int id)
    {
        GUI.DragWindow(new Rect(0, 0, 20000, 20));

        GUILayout.BeginVertical();

        for (int i = 0; i < displayNames.Length; i++)
        {
            var name = displayNames[i];
            //var path = currentDirPaths[i];
            if (GUILayout.Button(name))
            {
            }
        }

        GUILayout.EndVertical();
    }
}
