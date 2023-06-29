using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Exceptions.GUI;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class TerminalWindow : Window, ITerminalWindow
{
    public TerminalEntry[] TerminalEntries { get; }

    private string _terminalOutput = string.Empty;
    private string _terminalInput = string.Empty;
    private Vector2 _terminalOutputScroll;

    private TerminalEntry _hijackingEntry;

    private bool _initialFocus = true;

    public TerminalWindow(WindowDependencies windowDependencies, TerminalEntry[] terminalEntries) : base(
        windowDependencies,
        new(defaultWindowRect: GUIUtils.WindowRect(Screen.width - 100, Screen.height - 100), windowName: "Terminal"))
    {
        // check dupes
        var dupes = terminalEntries.GroupBy(x => x.Command).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        if (dupes.Any()) throw new DuplicateTerminalEntryException(dupes);
        TerminalEntries = terminalEntries;
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        _terminalOutputScroll = GUILayout.BeginScrollView(_terminalOutputScroll, GUIUtils.EmptyOptions);

        GUILayout.TextArea(_terminalOutput, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        if (_initialFocus)
        {
            UnityEngine.GUI.SetNextControlName("TerminalInput");
        }

        _terminalInput = GUILayout.TextField(_terminalInput, GUILayout.ExpandWidth(true));

        // check enter
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
            UnityEngine.GUI.GetNameOfFocusedControl() == "TerminalInput")
        {
            Submit();
            Event.current.Use();
        }

        if (_initialFocus)
        {
            _initialFocus = false;
            UnityEngine.GUI.FocusControl("TerminalInput");
        }

        if (GUILayout.Button("Submit", GUILayout.ExpandWidth(false)))
        {
            Submit();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void Submit()
    {
        if (_hijackingEntry != null)
        {
            _hijackingEntry.OnInput(_terminalInput);
            _terminalInput = string.Empty;
            return;
        }

        TerminalPrintLine($"] {_terminalInput}");

        var command = _terminalInput.Split(' ').FirstOrDefault();
        var args = _terminalInput.Split(' ').Skip(1).ToArray();

        _terminalInput = string.Empty;

        if (string.IsNullOrEmpty(command)) return;

        var entry = TerminalEntries.FirstOrDefault(x => x.Command == command);
        if (entry == null)
        {
            TerminalPrintLine("command not found");
            return;
        }

        var hijack = entry.Execute(args, this);
        if (hijack)
        {
            _hijackingEntry = entry;
        }
    }

    public void TerminalPrintLine(string output)
    {
        _terminalOutput += $"{output}\n";
    }

    public void ReleaseTerminal()
    {
        _hijackingEntry = null;
    }
}