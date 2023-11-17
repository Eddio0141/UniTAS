using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton(RegisterPriority.ToolBar)]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class ToolBar : IOnGUIUnconditional
{
    private readonly IWindowFactory _windowFactory;
    private readonly IDropdownMenuFactory _dropdownMenuFactory;

    private readonly GUIStyle _buttonStyle;
    private readonly Texture2D _buttonNormal = new(1, 1);
    private const int TOOLBAR_HEIGHT = 25;
    private bool _visible;

    private readonly Bind _toolbarVisibleBind;

    private readonly DropdownEntry[] _dropdownEntries;

    public ToolBar(IWindowFactory windowFactory, IBinds binds, IDropdownMenuFactory dropdownMenuFactory)
    {
        _windowFactory = windowFactory;
        _dropdownMenuFactory = dropdownMenuFactory;

        _buttonNormal.SetPixel(0, 0, GUIUtils.StandardBgColour);
        _buttonNormal.Apply();
        var buttonHold = new Texture2D(1, 1);
        buttonHold.SetPixel(0, 0, GUIUtils.HoverColour);
        buttonHold.Apply();

        _buttonStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = TOOLBAR_HEIGHT,
            padding = new(5, 5, 5, 5),
            normal = { background = _buttonNormal, textColor = Color.white },
            hover = { background = _buttonNormal, textColor = Color.white },
            active = { background = buttonHold, textColor = Color.white }
        };

        _toolbarVisibleBind = binds.Create(new("ToolbarVisible", KeyCode.F1));

        _dropdownEntries = new[]
        {
            new DropdownEntry("Show hidden", () => _windowFactory.Create<ShowHiddenWindow>().Show())
        };
    }

    private bool _toolsClickable;
    private Rect _toolsLastRect;

    public void OnGUIUnconditional()
    {
        if (Event.current.type == EventType.KeyDown && _toolbarVisibleBind.IsPressed())
        {
            _visible = !_visible;
            Event.current.Use();
        }

        // left click
        if (_toolsClickable && Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var pos = new Vector2(_toolsLastRect.xMax, _toolsLastRect.yMax);
            _dropdownMenuFactory.Create<DropdownWindow>(_dropdownEntries, pos).Show();
            Event.current.Use();
        }

        if (!_visible) return;

        UnityEngine.GUI.DrawTexture(new(0, 0, Screen.width, TOOLBAR_HEIGHT), _buttonNormal);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        if (GUILayout.Button("TAS Movie", _buttonStyle, GUIUtils.EmptyOptions))
        {
            _windowFactory.Create<MoviePlayWindow>().Show();
        }

        GUILayout.Button("Tools", _buttonStyle, GUIUtils.EmptyOptions);
        if (Event.current.type == EventType.Repaint)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            _toolsClickable = lastRect.Contains(Event.current.mousePosition);
            _toolsLastRect = lastRect;
        }

        if (GUILayout.Button("Terminal", _buttonStyle, GUIUtils.EmptyOptions))
        {
            _windowFactory.Create<TerminalWindow>().Show();
        }

        GUILayout.EndHorizontal();
    }
}