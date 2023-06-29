using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton(RegisterPriority.ToolBar)]
[ForceInstantiate]
public class ToolBar : IOnGUIUnconditional
{
    private readonly IWindowFactory _windowFactory;

    private readonly GUIStyle _buttonStyle;
    private readonly Texture2D _buttonNormal = new(1, 1);
    private const int TOOLBAR_HEIGHT = 25;

    public ToolBar(IWindowFactory windowFactory)
    {
        _windowFactory = windowFactory;

        _buttonNormal.SetPixel(0, 0, new(0.25f, 0.25f, 0.25f));
        _buttonNormal.Apply();
        var buttonHold = new Texture2D(1, 1);
        buttonHold.SetPixel(0, 0, new(0.5f, 0.5f, 0.5f));
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
    }

    public void OnGUIUnconditional()
    {
        UnityEngine.GUI.DrawTexture(new(0, 0, Screen.width, TOOLBAR_HEIGHT), _buttonNormal);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        if (GUILayout.Button("TAS Movie", _buttonStyle, GUIUtils.EmptyOptions))
        {
            _windowFactory.Create<MoviePlayWindow>().Show();
        }

        if (GUILayout.Button("Settings", _buttonStyle, GUIUtils.EmptyOptions))
        {
        }

        GUILayout.EndHorizontal();
    }
}