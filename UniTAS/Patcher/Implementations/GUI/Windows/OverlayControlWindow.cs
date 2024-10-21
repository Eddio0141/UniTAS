using System;
using System.Globalization;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class OverlayControlWindow : Window
{
    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        for (var i = 0; i < _overlays.Length; i++)
        {
            var overlay = _overlays[i];

            var horizontal = i % 2 == 0;
            if (horizontal)
            {
                if (i > 0)
                    GUILayout.EndHorizontal();
                if (i < _overlays.Length - 1)
                {
                    GUILayout.BeginHorizontal();
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }

            overlay.Show = GUILayout.Toggle(overlay.Show, overlay.WindowConfigId);
        }

        if (_overlays.Length % 2 == 0)
            GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("General settings");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        var selectedOverlay = _overlays[_selectedIndex];
        if (GUILayout.Button(selectedOverlay.WindowConfigId))
        {
            _dropDownOpen = true;
        }

        if (_dropDownOpen && Event.current.type == EventType.Repaint)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            _dropdownRect = new Rect(lastRect.x, lastRect.y + lastRect.height, 150, 0);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();

        // settings for that overlay
        var windowRect = selectedOverlay.WindowRect;
        _selectedXString = GUILayout.TextField(_selectedXString, GUILayout.Width(200));
        if (UnityEngine.GUI.changed && int.TryParse(_selectedXString.Trim(), out var x))
        {
            windowRect.x = Mathf.Clamp(x, 0, Screen.width - selectedOverlay.WindowRect.width);
        }

        _selectedYString = GUILayout.TextField(_selectedYString, GUILayout.Width(200));
        if (UnityEngine.GUI.changed && int.TryParse(_selectedYString.Trim(), out var y))
        {
            windowRect.y = Mathf.Clamp(y, 0, Screen.height - selectedOverlay.WindowRect.height);
        }

        selectedOverlay.WindowRect = windowRect;

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        if (!_dropDownOpen) return;
        if (_dropdownList.DropdownButtons(_dropdownRect, _dropDowns))
        {
            _dropDownOpen = false;
        }
    }

    private bool _dropDownOpen;
    private int _selectedIndex;
    private readonly (string, Action)[] _dropDowns;
    private readonly BuiltInOverlay[] _overlays;
    private readonly IDropdownList _dropdownList;
    private Rect _dropdownRect;
    private string _selectedXString;
    private string _selectedYString;

    public OverlayControlWindow(WindowDependencies windowDependencies, BuiltInOverlay[] overlays,
        IGUIComponentFactory guiComponentFactory) : base(windowDependencies,
        new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(400, 400), windowName: "Overlay control"),
        "OverlayControl")
    {
        _overlays = overlays;
        _dropDowns = overlays
            .Select<BuiltInOverlay, (string, Action)>(
                (x, i) => { return (x.WindowConfigId, () => OnOverlaySelect(i)); })
            .ToArray();
        _dropdownList = guiComponentFactory.CreateComponent<IDropdownList>();

        // initial selection
        OnOverlaySelect(0);
        return;

        void GetSelectedXYFromOverlay(BuiltInOverlay x)
        {
            var overlayRect = x.WindowRect;
            _selectedXString = overlayRect.x.ToString(CultureInfo.InvariantCulture);
            _selectedYString = overlayRect.y.ToString(CultureInfo.InvariantCulture);
        }

        void OnOverlaySelect(int index)
        {
            var overlay = _overlays[index];
            GetSelectedXYFromOverlay(overlay);
            _selectedIndex = index;
            overlay.OnDragEnd += () =>
            {
                if (_selectedIndex != index) return;
                // refresh
                GetSelectedXYFromOverlay(overlay);
            };
        }
    }
}