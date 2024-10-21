using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton(RegisterPriority.ToolBar)]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class ToolBar : IOnGUIUnconditional, IToolBar
{
    private readonly IWindowFactory _windowFactory;

    private readonly GUIStyle _buttonStyle;
    private readonly Texture2D _buttonNormal = new(1, 1);
    private const int ToolbarHeight = 35;

    public bool Show
    {
        get => _show;
        private set
        {
            if (_show == value) return;
            _show = value;
            OnShowChange?.Invoke(value);
        }
    }

    public event Action<bool> OnShowChange;

    private readonly Bind _toolbarVisibleBind;
    private bool _show;
    private readonly IDropdownList _dropdownList;

    public ToolBar(IWindowFactory windowFactory, IBinds binds, IGUIComponentFactory guiComponentFactory)
    {
        _windowFactory = windowFactory;
        _dropdownList = guiComponentFactory.CreateComponent<IDropdownList>();

        _buttonNormal.SetPixel(0, 0, new(0.25f, 0.25f, 0.25f));
        _buttonNormal.Apply();
        var buttonHold = new Texture2D(1, 1);
        buttonHold.SetPixel(0, 0, new(0.5f, 0.5f, 0.5f));
        buttonHold.Apply();

        _buttonStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = ToolbarHeight - 4,
            padding = new(15, 15, 5, 5),
            margin = new(5, 5, 5, 5),
            normal = { background = _buttonNormal, textColor = Color.white },
            hover = { background = _buttonNormal, textColor = Color.white },
            active = { background = buttonHold, textColor = Color.white }
        };

        _toolbarVisibleBind = binds.Create(new("ToolbarVisible", KeyCode.F1));

        // each index corresponds to DropDownSection entry
        _dropdownButtons =
        [
            [
                ("Overlays", () => { _windowFactory.Create<OverlayControlWindow>().Show = true; }),
                ("Terminal", () => { _windowFactory.Create<TerminalWindow>().Show = true; })
            ]
        ];
    }

    private enum DropDownSection
    {
        Windows
    }

    private DropDownSection? _currentDropDown;
    private Rect _currentDropDownRect;
    private bool _gotDropDownRect;
    private Rect _barRect;

    private const float BarWidthPercentage = 0.25f;

    public void OnGUIUnconditional()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == _toolbarVisibleBind.Key)
        {
            Show = !Show;
            Event.current.Use();
        }

        if (!Show) return;

        var width = Screen.width;
        var widthModified = width * BarWidthPercentage;

        _barRect = new Rect((width - widthModified) / 2, 20, widthModified, ToolbarHeight);

        UnityEngine.GUI.DrawTexture(_barRect, _buttonNormal);

        GUILayout.BeginArea(_barRect);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Movie", _buttonStyle, GUIUtils.EmptyOptions))
        {
            _windowFactory.Create<MoviePlayWindow>().Show = true;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Windows", _buttonStyle, GUIUtils.EmptyOptions))
        {
            _gotDropDownRect = false;
            _currentDropDown = DropDownSection.Windows;
        }

        SetDropDownRect();

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        if (!_currentDropDown.HasValue) return;

        if (_dropdownList.DropdownButtons(_currentDropDownRect, _dropdownButtons[(int)_currentDropDown.Value]))
        {
            _currentDropDown = null;
        }
    }

    // place this in front of all dropdown buttons
    private void SetDropDownRect()
    {
        if (_gotDropDownRect || Event.current.type != EventType.Repaint) return;
        _gotDropDownRect = true;

        var lastRect = GUILayoutUtility.GetLastRect();
        _currentDropDownRect = new(lastRect.x + _barRect.x, lastRect.y + _barRect.y + lastRect.height, 150, 0);
    }

    private readonly (string, Action)[][] _dropdownButtons;
}