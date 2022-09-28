using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniTASPlugin.GameOverlay;

public class FileBrowser
{
    string currentDir;
    string changingDir;
    string currentDirText;
    bool dirChanged;
    string[] currentDirPaths;
    string[] displayNames;
    string selectedFileText;
    Vector2 scrollPos;
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

    const int CONFIRM_SAVE_WIDTH = 250;
    const int CONFIRM_SAVE_HEIGHT = 100;
    readonly ConfirmBox confirmSave;

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
        scrollPos = new();
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
            id + 1,
            ConfirmBox.ConfirmBoxType.YesNo);
        extensionIndex = 0;
        extensionText = extensions[0].ToString();

        extensionsProcessed = new string[extensions.Length][][];
        for (int i = 0; i < extensions.Length; i++)
        {
            var ext = extensions[i];
            extensionsProcessed[i] = new string[ext.Filters.Length][];
            for (int j = 0; j < ext.Filters.Length; j++)
            {
                var filter = ext.Filters[j];
                var splitBuilder = new List<string>();
                if (filter == "*")
                    splitBuilder.Add("*");
                else
                {
                    var extSplit = filter.Split('*');
                    Plugin.Log.LogDebug($"extSplit.Length: {extSplit.Length}");
                    for (int k = 0; k < extSplit.Length; k++)
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
    }

    public void Open()
    {
        changingDir = currentDir;
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
                    var displayNamesBuilder = new List<string>();
                    var currentDirPathsBuilder = new List<string>();
                    var entries = Directory.GetFileSystemEntries(changingDir, "*");

                    foreach (var entry in entries)
                    {
                        if (Directory.Exists(entry))
                        {
                            var dirName = Path.GetFileName(entry);
                            displayNamesBuilder.Add(dirName);
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
                                        Plugin.Log.LogDebug($"nextFilter: {nextFilter}");
                                        var nextFilterIndex = nameProcessing.IndexOf(nextFilter);
                                        Plugin.Log.LogDebug($"nextFilterIndex: {nextFilterIndex}");
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

                        displayNamesBuilder.Add(name);
                        currentDirPathsBuilder.Add(entry);
                    }

                    currentDirPaths = currentDirPathsBuilder.ToArray();
                    displayNames = displayNamesBuilder.ToArray();
                }
                catch (Exception)
                {
                    currentDirPaths = new string[0];
                    displayNames = new string[0];
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

        var nextScrollPos = GUILayout.BeginScrollView(scrollPos, false, true);
        if (!confirmSave.Opened)
            scrollPos = nextScrollPos;
        for (int i = 0; i < currentDirPaths.Length; i++)
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
                    selectedFileText = name;
                }
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
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
        string stringified;

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
