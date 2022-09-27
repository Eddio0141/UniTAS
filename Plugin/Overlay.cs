using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin;

internal static class Overlay
{
    public static bool Enabled { get; set; } = true;
    public static bool ShowCursor { get; set; } = true;
    private static bool unityCursorVisible = false;
    public static bool UnityCursorVisible
    {
        get => unityCursorVisible;
        set
        {
            CursorWrap.visible = !value;
            unityCursorVisible = value;
        }
    }
    static Texture2D cursorDefaultTexture = new Texture2D(2, 2);
    static Texture2D currentTexture = new Texture2D(2, 2);

    public static void Init()
    {
        var alpha = new Color(0, 0, 0, 0);
        var black = Color.black;
        var white = Color.white;
        var cursorRaw = new Color[]
        {
            black,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,
            black,black,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,
            black,white,black,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,
            black,white,white,black,alpha,alpha,alpha,alpha,alpha,alpha,alpha,alpha,
            black,white,white,white,black,alpha,alpha,alpha,alpha,alpha,alpha,alpha,
            black,white,white,white,white,black,alpha,alpha,alpha,alpha,alpha,alpha,
            black,white,white,white,white,white,black,alpha,alpha,alpha,alpha,alpha,
            black,white,white,white,white,white,white,black,alpha,alpha,alpha,alpha,
            black,white,white,white,white,white,white,white,black,alpha,alpha,alpha,
            black,white,white,white,white,white,white,white,white,black,alpha,alpha,
            black,white,white,white,white,white,white,white,white,white,black,alpha,
            black,white,white,white,white,white,white,white,white,white,white,black,
            black,white,white,white,white,white,white,black,black,black,black,black,
            black,white,white,white,black,white,white,black,alpha,alpha,alpha,alpha,
            black,white,white,black,alpha,black,white,white,black,alpha,alpha,alpha,
            black,white,black,alpha,alpha,black,white,white,black,alpha,alpha,alpha,
            black,black,alpha,alpha,alpha,alpha,black,white,white,black,alpha,alpha,
            alpha,alpha,alpha,alpha,alpha,alpha,black,white,white,black,alpha,alpha,
            alpha,alpha,alpha,alpha,alpha,alpha,alpha,black,black,alpha,alpha,alpha,
        };
        var width = 12;
        cursorDefaultTexture.Resize(width, cursorRaw.Length / width);
        for (int i = 0; i < cursorRaw.Length; i++)
        {
            var x = i % width;
            var y = cursorDefaultTexture.height - i / width;
            cursorDefaultTexture.SetPixel(x, y, cursorRaw[i]);
        }
        cursorDefaultTexture.Apply();
        currentTexture = cursorDefaultTexture;

        UnityCursorVisible = true;
    }

    public static void SetCursorTexture(Texture2D texture)
    {
        if (texture == null)
            currentTexture = cursorDefaultTexture;
        else
            currentTexture = texture;
    }

    public static void Update()
    {
        if (!Enabled)
            return;

        if (ShowCursor && UnityCursorVisible)
            GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, currentTexture.width, currentTexture.height), currentTexture);
    }
}

