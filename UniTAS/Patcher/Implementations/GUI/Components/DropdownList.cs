using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Components;

[Register]
public class DropdownList : IDropdownList
{
    private static GUIStyle _buttonStyle;
    private static float? _buttonHeight;

    public bool DropdownButtons(Rect position, (string, Action)[] buttons)
    {
        _buttonStyle ??= new(UnityEngine.GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = new()
            {
                background = TextureUtils.MakeSolidColourTexture(2, 2, new Color(0.09f, 0.12f, 0.22f)),
                textColor = Color.white
            },
            hover = new()
            {
                background = TextureUtils.MakeSolidColourTexture(2, 2, new Color(0.12f, 0.16f, 0.29f)),
                textColor = Color.white
            },
            active = new()
            {
                background = TextureUtils.MakeSolidColourTexture(2, 2, new Color(0.15f, 0.27f, 0.37f)),
                textColor = Color.white
            },
            fixedHeight = 25,
            padding = new(5, 5, 5, 5),
            margin = new(),
        };
        _buttonStyle.fixedWidth = position.width;
        _buttonHeight ??= _buttonStyle.CalcHeight(GUIContent.none, position.width);
        position.height = _buttonHeight.Value * buttons.Length;

        var clicked = false;

        // has mouse click happened anywhere else
        var currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 &&
            !position.Contains(currentEvent.mousePosition))
        {
            currentEvent.Use();
            clicked = true;
        }

        UnityEngine.GUI.BeginGroup(position);

        for (var i = 0; i < buttons.Length; i++)
        {
            var (label, onClick) = buttons[i];
            var pos = new Rect(0, _buttonHeight.Value * i, position.width, position.height);
            if (UnityEngine.GUI.Button(pos, label, _buttonStyle))
            {
                onClick();
                clicked = true;
            }
        }

        UnityEngine.GUI.EndGroup();

        return clicked;
    }
}