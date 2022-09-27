using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniTASPlugin.TASMovie;
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
        BGSurround.SetPixels(Enumerable.Repeat(new Color(1, 1, 1, 0.5f), MENU_SIZE_X * MENU_SIZE_Y).ToArray());
        BGSurround.Apply();
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
        // TODO temporary for debugging
        if (!TAS.Running && Input.GetKeyDown(KeyCode.F10))
        {
            Enabled = !Enabled;
        }
        if (!TAS.Running && Input.GetKeyDown(KeyCode.F11))
        {
            CursorWrap.TempCursorLockToggle(!CursorWrap.TempUnlocked);
            Plugin.Log.LogDebug($"Unlocked cursor: {CursorWrap.TempUnlocked}");
        }
    }

    public static void OnGUI()
    {
        DrawGUI();

        if (CursorWrap.TempUnlocked || (ShowCursor && UnityCursorVisible))
            GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, currentTexture.width, currentTexture.height), currentTexture);
    }

    static int tabIndex = 0;
    static readonly string[] tabs = new string[] { "Main", "Debug" };
    static Texture2D BGSurround = new Texture2D(MENU_SIZE_X, MENU_SIZE_Y);

    const int MENU_SIZE_X = 200;
    const int MENU_SIZE_Y = 200;
    const int BUTTON_HEIGHT = 20;
    const int MAIN_CONTENT_START_Y = 55;
    const int SPACING = 10;

    static void DrawGUI()
    {
        if (!Enabled)
            return;

        GUI.BeginGroup(new Rect(10, 10, MENU_SIZE_X, MENU_SIZE_Y));
        GUI.DrawTexture(new Rect(0, 0, MENU_SIZE_X, MENU_SIZE_Y), BGSurround);
        GUI.Box(new Rect(0, 0, MENU_SIZE_X, MENU_SIZE_Y), $"{Plugin.NAME} Menu");

        tabIndex = GUI.Toolbar(new Rect(SPACING, 30, 180, 20), tabIndex, tabs);

        switch (tabIndex)
        {
            case 0:
                break;
            default:
                // debug
                {
                    if (GUI.Button(new Rect(SPACING, MAIN_CONTENT_START_Y, 100, BUTTON_HEIGHT), "test TAS") && !TAS.Running)
                    {
                        string text = "";
                        if (File.Exists("C:\\Users\\Yuki\\Documents\\test.uti"))
                            text = File.ReadAllText("C:\\Users\\Yuki\\Documents\\test.uti");
                        else if (File.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti"))
                            text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
                        var movie = new Movie("test.uti", text, out var err, out List<string> warnings);

                        if (err != "")
                        {
                            Plugin.Log.LogError(err);
                            return;
                        }
                        if (warnings.Count > 1)
                        {
                            foreach (string warn in warnings)
                            {
                                Plugin.Log.LogWarning(warn);
                            }
                        }

                        TAS.RunMovie(movie);
                    }
                    break;
                }
        }

        GUI.EndGroup();
    }
}