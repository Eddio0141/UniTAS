using UnityEngine;

namespace UniTASPlugin;

public static class Overlay
{
    /*
    static Vector2 hotspot = Vector2.zero;
    public static bool Visible { get; set; } = true;
    static readonly CanvasScaler canvasScaler;
    static readonly GameObject cursorRawImageObj;
    static readonly RawImage cursorRawImage;
    static readonly RectTransform cursorRectTransform;

    public static void Init()
    {
        var canvasObj = new GameObject("VirtualCursorCanvas");
        Object.DontDestroyOnLoad(canvasObj);
        cursorRawImageObj = new GameObject("VirtualCursorRawImage");
        Object.DontDestroyOnLoad(cursorRawImageObj);

        cursorRawImageObj.transform.SetParent(canvasObj.transform);

        canvasObj.AddComponent<Canvas>();

        canvasScaler = canvasObj.AddComponent<CanvasScaler>();

        cursorRawImage = cursorRawImageObj.AddComponent<RawImage>();
        cursorRectTransform = cursorRawImageObj.GetComponent<RectTransform>();
        cursorRawImage.texture = new Texture2D(100, 100);
    }

    public static void SetCursor(Texture2D texture, Vector2 hotspot)
    {
        cursorRawImage.texture = texture;
        VirtualCursor.hotspot = hotspot;
    }

    public static void Update()
    {
        if (!Visible)
            return;

        cursorRectTransform.anchoredPosition = (Vector2)Mouse.Position.ConvertTo() + hotspot;
    }
    */
}

