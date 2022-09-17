using UnityEngine;
using UnityEngine.UI;

namespace Core.TAS.Input;

public static class VirtualCursor
{
    static Vector2 hotspot;
    public static bool Visible { get; set; }
    static readonly CanvasScaler canvasScaler;
    static readonly GameObject cursorRawImageObj;
    static readonly RawImage cursorRawImage;
    static readonly RectTransform cursorRectTransform;

    static VirtualCursor()
    {
        hotspot = Vector2.zero;
        Visible = true;

        var canvasObj = new GameObject("VirtualCursorCanvas");
        Object.DontDestroyOnLoad(canvasObj);
        cursorRawImageObj = new GameObject("VirtualCursorRawImage");
        Object.DontDestroyOnLoad(cursorRawImageObj);

        cursorRawImageObj.transform.SetParent(canvasObj.transform);

        canvasObj.AddComponent<Canvas>();

        canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        cursorRawImage = cursorRawImageObj.AddComponent<RawImage>();
        cursorRectTransform = cursorRawImageObj.GetComponent<RectTransform>();
        cursorRawImage.texture = new Texture2D(100, 100);
    }

    public static void SetCursor(Texture2D texture, Vector2 hotspot)
    {
        cursorRawImage.texture = texture;
        VirtualCursor.hotspot = hotspot;
    }

    /// <summary>
    /// Shows the cursor.
    /// </summary>
    public static void Update()
    {
        if (!Visible)
            return;

        cursorRectTransform.anchoredPosition = (Vector2)Mouse.Position.ConvertTo() + hotspot;
    }
}

