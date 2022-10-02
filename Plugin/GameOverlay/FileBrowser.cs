using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.GameOverlay;

public class FileBrowser
{
    string currentDir;
    string changingDir;
    string currentDirText;
    bool dirChanged;
    string[] currentDirPaths;
    GUIContent[] displayNames;
    string selectedFileText;
    Vector2 fileScrollPos;
    Rect defaultRect;
    Rect windowRect;
    bool opened;
    readonly int id;
    readonly string title;
    string finalPath;
    bool gotFinalPath;
    readonly FileBrowserType browserType;
    readonly string selectText;
    readonly Extension[] extensions;
    // list of filter list, each filter list is a split of the filter
    readonly string[][][] extensionsProcessed;
    string extensionText;
    int extensionIndex;
    readonly int quickAccessWidth;
    Vector2 quickAccessScrollPos;

    const int CONFIRM_SAVE_WIDTH = 250;
    const int CONFIRM_SAVE_HEIGHT = 100;
    readonly ConfirmBox confirmSave;
    readonly string[] quickAccessPaths;
    readonly string[] quickAccessNames;

    Stack<string> dirPrev;
    Stack<string> dirNext;
    bool movingToPrev;
    bool movingToNext;

    static readonly Texture2D folderTexture;
    static readonly Texture2D fileTexture;

    static FileBrowser()
    {
        var c0 = new Color(0, 0, 0, 0);
        var c1 = new Color(1, 201f / 255f, 14f / 255f);
        var c2 = new Color(239f / 255f, 228f / 255f, 176f / 255f);
        var c3 = new Color(1, 242f / 255f, 0);
        var folderIcon = new[]
        {
            c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,
            c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,
            c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,
            c0,c1,c1,c1,c1,c1,c1,c0,c0,c0,c0,c0,c0,c0,c0,c0,
            c0,c1,c1,c1,c1,c1,c1,c2,c2,c2,c2,c2,c2,c2,c1,c0,
            c0,c1,c1,c1,c1,c1,c1,c2,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c2,c2,c2,c2,c2,c2,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c2,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c3,c1,c0,
            c0,c1,c1,c1,c1,c1,c1,c1,c1,c1,c1,c1,c1,c1,c1,c0,
            c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,c0,
        };
        var size = 16;
        folderTexture = new(size, size);
        for (var i = 0; i < folderIcon.Length; i++)
        {
            var xPos = i % size;
            var yPos = size - i / size;
            folderTexture.SetPixel(xPos, yPos, folderIcon[i]);
        }
        folderTexture.Apply();

        var c4 = new Color(163f / 255f, 164f / 255f, 164f / 255f);
        var c5 = new Color(140f / 255f, 141f / 255f, 142f / 255f);
        var c6 = new Color(214f / 255f, 214f / 255f, 214f / 255f);
        var c7 = new Color(247f / 255f, 248f / 255f, 249f / 255f);
        var fileIcon = new[]
        {
            c0,c0,c4,c4,c4,c4,c4,c4,c4,c4,c4,c4,c0,c0,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c4,c6,c5,c0,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c4,c6,c6,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c4,c5,c5,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c7,c7,c7,c7,c7,c7,c7,c7,c7,c7,c5,c0,c0,
            c0,c0,c4,c5,c5,c5,c5,c5,c5,c5,c5,c5,c5,c5,c0,c0,
        };
        fileTexture = new(size, size);
        for (var i = 0; i < fileIcon.Length; i++)
        {
            var xPos = i % size;
            var yPos = size - i / size;
            fileTexture.SetPixel(xPos, yPos, fileIcon[i]);
        }
        fileTexture.Apply();
    }

    public FileBrowser(string currentDir, Rect windowRect, string title, int id, FileBrowserType browserType, Extension[] extensions)
    {
        this.currentDir = currentDir;
        changingDir = currentDir;
        currentDirText = currentDir;
        defaultRect = windowRect;
        this.windowRect = windowRect;
        this.title = title;
        this.id = id;
        dirChanged = true;
        fileScrollPos = new();
        opened = false;
        finalPath = "";
        gotFinalPath = true;
        this.browserType = browserType;
        if (extensions.Length < 1)
            extensions = new[] { new Extension() };
        this.extensions = extensions;
        confirmSave = new ConfirmBox(
            "Confirm Save",
            "Are you sure you want to save?",
            new Rect(
                Screen.width / 2 - CONFIRM_SAVE_WIDTH / 2,
                Screen.height / 2 - CONFIRM_SAVE_HEIGHT / 2,
                CONFIRM_SAVE_WIDTH,
                CONFIRM_SAVE_HEIGHT),
            id + 100,
            ConfirmBox.ConfirmBoxType.YesNo);
        extensionIndex = 0;
        extensionText = extensions[0].ToString();

        extensionsProcessed = new string[extensions.Length][][];
        for (var i = 0; i < extensions.Length; i++)
        {
            var ext = extensions[i];
            extensionsProcessed[i] = new string[ext.Filters.Length][];
            for (var j = 0; j < ext.Filters.Length; j++)
            {
                var filter = ext.Filters[j];
                var splitBuilder = new List<string>();
                if (filter == "*")
                    splitBuilder.Add("*");
                else
                {
                    var extSplit = filter.Split('*');
                    for (var k = 0; k < extSplit.Length; k++)
                    {
                        var split = extSplit[k];
                        if (split == "")
                            splitBuilder.Add("*");
                        else
                            splitBuilder.Add(split);
                    }
                }
                extensionsProcessed[i][j] = splitBuilder.ToArray();
            }
        }

        switch (browserType)
        {
            case FileBrowserType.Open:
                selectText = "Open";
                break;
            case FileBrowserType.Save:
                {
                    selectText = "Save"; ;
                    break;
                }
        }

        quickAccessWidth = (int)(windowRect.width / 9);

        var homePath = Environment.GetEnvironmentVariable("HOMEPATH");

        var quickAccessPathsBuilder = new List<string>() {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            homePath,
            Helper.GameRootDir(),
        };
        var quickAccessNamesBuilder = new List<string>() {
            "Desktop",
            "Documents",
            "Home",
            "Game root",
        };

        try
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                quickAccessPathsBuilder.Add(drive.Name);
                quickAccessNamesBuilder.Add(drive.Name);
            }
        }
        catch (Exception) { }

        quickAccessPaths = quickAccessPathsBuilder.ToArray();
        quickAccessNames = quickAccessNamesBuilder.ToArray();
    }

    public void Open()
    {
        changingDir = currentDir;
        windowRect = defaultRect;
        dirChanged = true;
        fileScrollPos = new();
        opened = true;
        finalPath = "";
        gotFinalPath = true;
        quickAccessScrollPos = new();
        dirPrev = new();
        dirNext = new();
        movingToPrev = false;
        movingToNext = false;
        selectedFileText = "";
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

        confirmSave.Update();
        if (confirmSave.FinalResult(out var overwriteSave) && overwriteSave)
        {
            finalPath = Path.Combine(currentDir, selectedFileText);
            gotFinalPath = false;
            opened = false;
        }

        if (dirChanged)
        {
            if (Directory.Exists(changingDir))
            {
                try
                {
                    var displayNamesBuilder = new List<GUIContent>();
                    var currentDirPathsBuilder = new List<string>();
                    var entries = Directory.GetFileSystemEntries(changingDir, "*");

                    foreach (var entry in entries)
                    {
                        if (Directory.Exists(entry))
                        {
                            var dirName = Path.GetFileName(entry);
                            displayNamesBuilder.Add(new GUIContent(dirName, folderTexture));
                            currentDirPathsBuilder.Add(entry);
                            continue;
                        }

                        var name = Path.GetFileName(entry);

                        // filter
                        var filterOut = true;
                        var extFilters = extensionsProcessed[extensionIndex];
                        var nameProcessing = name;

                        foreach (var extFilter in extFilters)
                        {
                            var i = 0;
                            while (i < extFilter.Length)
                            {
                                var filter = extFilter[i];
                                if (filter == "*")
                                {
                                    if (i + 1 == extFilter.Length)
                                    {
                                        filterOut = false;
                                        break;
                                    }
                                    else
                                    {
                                        var nextFilter = extFilter[i + 1];
                                        var nextFilterIndex = nameProcessing.IndexOf(nextFilter);
                                        if (nextFilterIndex < 0)
                                            break;
                                        nameProcessing = nameProcessing.Substring(nextFilterIndex + nextFilter.Length);
                                        i += 2;
                                        if (nameProcessing.Length == 0 && i >= extFilter.Length)
                                        {
                                            filterOut = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    var filterIndex = nameProcessing.IndexOf(filter);
                                    if (filterIndex < 0)
                                        break;
                                    if (filterIndex == 0)
                                    {
                                        nameProcessing = nameProcessing.Substring(filter.Length);
                                    }
                                    if (i + 1 == extFilter.Length)
                                    {
                                        if (nameProcessing.Length == 0)
                                            filterOut = false;
                                        break;
                                    }
                                    i++;
                                }
                            }
                            if (!filterOut)
                                break;
                        }

                        if (filterOut)
                            continue;

                        displayNamesBuilder.Add(new GUIContent(name, fileTexture));
                        currentDirPathsBuilder.Add(entry);
                    }

                    currentDirPaths = currentDirPathsBuilder.ToArray();
                    displayNames = displayNamesBuilder.ToArray();

                    if (movingToPrev)
                    {
                        movingToPrev = false;
                        dirNext.Push(currentDir);
                    }
                    else
                    if (movingToNext)
                    {
                        movingToNext = false;
                        dirPrev.Push(currentDir);
                    }
                    else
                    {
                        dirPrev.Push(currentDir);
                        dirNext.Clear();
                    }
                }
                catch (Exception)
                {
                    currentDirPaths = new string[0];
                    displayNames = new GUIContent[0];
                }
                currentDir = changingDir;
            }
            currentDirText = currentDir;
            dirChanged = false;
        }

        windowRect = GUILayout.Window(id, windowRect, Window, title, GUI.skin.window);
    }

    void Window(int id)
    {
        GUI.DragWindow(new Rect(0, 0, 20000, 20));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(20)) && !confirmSave.Opened && dirPrev.Count > 0)
        {
            changingDir = dirPrev.Pop();
            movingToPrev = true;
            dirChanged = true;
        }
        if (GUILayout.Button(">", GUILayout.Width(20)) && !confirmSave.Opened && dirNext.Count > 0)
        {
            changingDir = dirNext.Pop();
            movingToNext = true;
            dirChanged = true;
        }
        if (GUILayout.Button("^", GUILayout.Width(20)) && !confirmSave.Opened)
        {
            changingDir = Path.GetDirectoryName(currentDir);
            dirChanged = true;
        }
        currentDirText = GUILayout.TextField(currentDirText);
        if (GUILayout.Button("Go", GUILayout.Width(50)) && !confirmSave.Opened)
        {
            changingDir = currentDirText;
            dirChanged = true;
        }
        if (GUILayout.Button("x", GUILayout.Width(20)) && !confirmSave.Opened)
        {
            opened = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        // quick access
        var nextScrollPos = GUILayout.BeginScrollView(quickAccessScrollPos, GUILayout.Width(quickAccessWidth));
        GUILayout.BeginVertical();
        GUILayout.Label("Quick access", GUILayout.Width(quickAccessWidth - 30));
        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        if (!confirmSave.Opened)
            quickAccessScrollPos = nextScrollPos;
        for (var i = 0; i < quickAccessPaths.Length; i++)
        {
            var path = quickAccessPaths[i];
            var name = quickAccessNames[i];
            if (GUILayout.Button(name) && !confirmSave.Opened)
            {
                changingDir = path;
                dirChanged = true;
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        nextScrollPos = GUILayout.BeginScrollView(fileScrollPos, false, true);
        // files in dir
        if (!confirmSave.Opened)
            fileScrollPos = nextScrollPos;
        for (var i = 0; i < currentDirPaths.Length; i++)
        {
            var path = currentDirPaths[i];
            var name = displayNames[i];
            if (GUILayout.Button(name) && !confirmSave.Opened)
            {
                if (Directory.Exists(path))
                {
                    changingDir = path;
                    dirChanged = true;
                }
                else
                {
                    selectedFileText = name.text;
                }
            }
        }
        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("file name:", GUILayout.Width(70));
        selectedFileText = GUILayout.TextField(selectedFileText);
        if (GUILayout.Button(selectText, GUILayout.Width(50)) && !confirmSave.Opened)
        {
            var combinedPath = Path.Combine(currentDir, selectedFileText);
            switch (browserType)
            {
                case FileBrowserType.Open:
                    {
                        if (File.Exists(combinedPath))
                        {
                            opened = false;
                            gotFinalPath = false;
                            finalPath = combinedPath;
                        }
                        break;
                    }
                case FileBrowserType.Save:
                    {
                        if (File.Exists(combinedPath))
                        {
                            confirmSave.Open();
                        }
                        else
                        {
                            opened = false;
                            gotFinalPath = false;
                            finalPath = combinedPath;
                        }
                        break;
                    }
            }
        }
        if (GUILayout.Button(extensionText, GUILayout.Width(200)) && extensions.Length > 1)
        {
            extensionIndex++;
            if (extensionIndex >= extensions.Length)
                extensionIndex = 0;
            var nextExt = extensions[extensionIndex];
            extensionText = nextExt.ToString();
            changingDir = currentDir;
            dirChanged = true;
        }
        GUILayout.EndHorizontal();
    }

    public enum FileBrowserType
    {
        Open,
        Save
    }

    public struct Extension
    {
        public string Name;
        public string[] Filters;
        readonly string stringified;

        public Extension(string name, string[] filters)
        {
            Name = name;
            Filters = filters;
            stringified = $"{Name} ({string.Join(";", Filters)})";
        }

        public Extension() : this("All Files", new[] { "*" }) { }

        public override string ToString()
        {
            return stringified;
        }
    }
}
