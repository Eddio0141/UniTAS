using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Drawing;

public partial class Drawing
{
    private readonly Queue<PendingText> _texts = new();
    private readonly GUIStyle _labelWithFontSize = new();
    private readonly GUIContent _labelContent = new();
    private readonly Action _nextText;

    public void PrintText(AnchoredOffset offset, string text, int fontSize)
    {
        _texts.Enqueue(new(text, offset, fontSize));
        _pendingDraws.Add(_nextText);
    }

    private const int TEXT_SHADOW_OFFSET = 1;

    private static readonly (int, int)[] TextShadowOffsets =
    [
        new(TEXT_SHADOW_OFFSET, TEXT_SHADOW_OFFSET), new(-TEXT_SHADOW_OFFSET * 2, 0),
        new(0, -TEXT_SHADOW_OFFSET * 2), new(TEXT_SHADOW_OFFSET * 2, 0)
    ];

    private void NextText()
    {
        var text = _texts.Dequeue();

        _labelWithFontSize.fontSize = text.FontSize;
        _labelWithFontSize.normal.textColor = Color.black;
        _labelContent.text = text.Text;

        var size = _labelWithFontSize.CalcSize(_labelContent);
        var offset = text.Offset;

        var pos = GetScreenPosition(offset, size);

        var labelRect = new Rect(pos.x, pos.y, size.x, size.y);
        var labelRectOriginal = new Rect(labelRect);

        foreach (var (shadowOffsetX, shadowOffsetY) in TextShadowOffsets)
        {
            labelRect.x += shadowOffsetX;
            labelRect.y += shadowOffsetY;
            UnityEngine.GUI.Label(labelRect, text.Text, _labelWithFontSize);
        }

        _labelWithFontSize.normal.textColor = Color.white;
        UnityEngine.GUI.Label(labelRectOriginal, text.Text, _labelWithFontSize);
    }

    private struct PendingText
    {
        public string Text { get; }
        public AnchoredOffset Offset { get; }
        public int FontSize { get; }

        public PendingText(string text, AnchoredOffset offset, int fontSize)
        {
            Text = text;
            Offset = offset;
            FontSize = fontSize;
        }
    }
}