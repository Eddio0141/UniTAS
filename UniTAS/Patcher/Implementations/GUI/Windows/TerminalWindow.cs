using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx;
using UniTAS.Patcher.Exceptions.GUI;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class TerminalWindow : Window, ITerminalWindow
{
    public TerminalEntry[] TerminalEntries { get; }

    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly ITerminalLogger _logger;

    private string _terminalOutput = string.Empty;
    private string _terminalInput = string.Empty;
    private string _terminalInputFull = string.Empty;
    private Vector2 _terminalOutputScroll;

    private TerminalEntry _hijackingEntry;

    private bool _initialFocus = true;

    private readonly Bind _terminalBind;
    private readonly Bind _terminalSubmit;

    public TerminalWindow(WindowDependencies windowDependencies, TerminalEntry[] terminalEntries, IBinds binds,
        IGlobalHotkey
            globalHotkey, ITerminalLogger logger) : base(
        windowDependencies,
        new(defaultWindowRect: GUIUtils.WindowRect(Screen.width - 100, Screen.height - 100), windowName: "Terminal"))
    {
        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        windowDependencies.UpdateEvents.OnUpdateUnconditional += OnUpdateUnconditional;

        _terminalBind = binds.Create(new("NewTerminal", KeyCode.BackQuote));
        _terminalSubmit = binds.Create(new("TerminalSubmit", KeyCode.Return), true);
        globalHotkey.AddGlobalHotkey(new(_terminalBind, Show));

        // check dupes
        var dupes = terminalEntries.GroupBy(x => x.Command).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        if (dupes.Any()) throw new DuplicateTerminalEntryException(dupes);
        TerminalEntries = terminalEntries;
        _logger = logger;
    }

    private readonly GUILayoutOption[] _textAreaOptions = [GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)];
    private readonly GUILayoutOption[] _terminalInputOptions = [GUILayout.ExpandWidth(true)];
    private readonly GUILayoutOption[] _submitOptions = [GUILayout.ExpandWidth(false)];

    private void OnUpdateUnconditional()
    {
        if (!_waitForTerminalBindRelease || _terminalBind.IsPressed()) return;

        // if waiting for release and bind is not pressed
        _initialFocus = true;
        _waitForTerminalBindRelease = false;
        Close();
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        _terminalOutputScroll = GUILayout.BeginScrollView(_terminalOutputScroll, GUIUtils.EmptyOptions);

        GUILayout.TextArea(_terminalOutput, _textAreaOptions);

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        CheckTerminalInputBinds();

        UnityEngine.GUI.SetNextControlName("TerminalInput");

        _terminalInput = GUILayout.TextField(_terminalInput, _terminalInputOptions);

        if (_initialFocus)
        {
            _initialFocus = false;
            UnityEngine.GUI.FocusControl("TerminalInput");
        }

        if (GUILayout.Button("Submit", _submitOptions))
        {
            Submit(false);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private bool GetKey(KeyCode keyCode)
    {
        return _patchReverseInvoker.Invoke(key => UnityInput.Current.GetKey(key), keyCode);
    }

    private bool _waitForTerminalBindRelease;

    private void CheckTerminalInputBinds()
    {
        if (_waitForTerminalBindRelease)
        {
            Event.current.Use();
            return;
        }

        if (Event.current.type != EventType.KeyDown || UnityEngine.GUI.GetNameOfFocusedControl() != "TerminalInput")
        {
            return;
        }

        if (Event.current.keyCode == _terminalSubmit.Key)
        {
            // hold shift to split input
            Submit(GetKey(KeyCode.LeftShift) | GetKey(KeyCode.RightShift));
            Event.current.Use();
            return;
        }

        if (Event.current.keyCode == _terminalBind.Key && _hijackingEntry == null)
        {
            _waitForTerminalBindRelease = true;
            Event.current.Use();
            return;
        }
    }

    private void Submit(bool split)
    {
        _terminalInputFull += $"{_terminalInput}\n";

        if (_hijackingEntry != null)
        {
            _hijackingEntry.OnInput(_terminalInput, true);
            if (!split)
            {
                _hijackingEntry.OnInput(_terminalInputFull, false);
            }

            _terminalInput = string.Empty;
            return;
        }

        TerminalPrintLine($"] {_terminalInput}");

        _terminalInput = string.Empty;

        if (split) return;

        var command = _terminalInputFull.Split(' ').FirstOrDefault()?.Trim();
        var args = _terminalInputFull.Split(' ').Skip(1).Select(x => x.Trim()).ToArray();

        _terminalInputFull = string.Empty;

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
        _logger.LogMessage(output);

        // scroll to bottom
        _terminalOutputScroll.y = Mathf.Infinity;
    }

    public void ReleaseTerminal()
    {
        _hijackingEntry = null;
    }

    protected override void Close()
    {
        base.Close();
    }
}
