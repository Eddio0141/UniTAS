using System.Linq;
using UniTASPlugin.GameRestart;
using UniTASPlugin.LegacyGameOverlay.GameConsole;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.Movie;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin.LegacyGameOverlay;

internal static class Overlay
{
    public static bool Enabled { get; set; } = true;
    public static bool ShowCursor { get; set; } = true;
    public static bool UnityCursorVisible { get; set; } = true;
    private static readonly Texture2D cursorDefaultTexture = new(2, 2);
    private static Texture2D currentTexture = new(2, 2);

    static Overlay()
    {
        var alpha = new Color(0, 0, 0, 0);
        var black = Color.black;
        var white = Color.white;
        var cursorRaw = new[]
        {
            black, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha,
            black, black, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha,
            black, white, black, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha,
            black, white, white, black, alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha,
            black, white, white, white, black, alpha, alpha, alpha, alpha, alpha, alpha, alpha,
            black, white, white, white, white, black, alpha, alpha, alpha, alpha, alpha, alpha,
            black, white, white, white, white, white, black, alpha, alpha, alpha, alpha, alpha,
            black, white, white, white, white, white, white, black, alpha, alpha, alpha, alpha,
            black, white, white, white, white, white, white, white, black, alpha, alpha, alpha,
            black, white, white, white, white, white, white, white, white, black, alpha, alpha,
            black, white, white, white, white, white, white, white, white, white, black, alpha,
            black, white, white, white, white, white, white, white, white, white, white, black,
            black, white, white, white, white, white, white, black, black, black, black, black,
            black, white, white, white, black, white, white, black, alpha, alpha, alpha, alpha,
            black, white, white, black, alpha, black, white, white, black, alpha, alpha, alpha,
            black, white, black, alpha, alpha, black, white, white, black, alpha, alpha, alpha,
            black, black, alpha, alpha, alpha, alpha, black, white, white, black, alpha, alpha,
            alpha, alpha, alpha, alpha, alpha, alpha, black, white, white, black, alpha, alpha,
            alpha, alpha, alpha, alpha, alpha, alpha, alpha, black, black, alpha, alpha, alpha
        };
        var width = 12;
        _ = cursorDefaultTexture.Resize(width, cursorRaw.Length / width);
        for (var i = 0; i < cursorRaw.Length; i++)
        {
            var x = i % width;
            var y = cursorDefaultTexture.height - i / width;
            cursorDefaultTexture.SetPixel(x, y, cursorRaw[i]);
        }

        cursorDefaultTexture.Apply();
        currentTexture = cursorDefaultTexture;

        BGSurround.SetPixels(Enumerable.Repeat(new Color(1, 1, 1, 0.5f), MENU_SIZE_X * MENU_SIZE_Y).ToArray());
        BGSurround.Apply();

        // hide normal cursor
        //CursorWrap.Visible = false;
    }

    public static void SetCursorTexture(Texture2D texture)
    {
        currentTexture = texture ?? cursorDefaultTexture;
    }

    public static void Update()
    {
        var movieRunner = Plugin.Kernel.GetInstance<IMovieRunner>();
        if (movieRunner.MovieEnd && Input.GetKeyDown(KeyCode.F10))
        {
            Enabled = !Enabled;
        }

        if (movieRunner.MovieEnd && Input.GetKeyDown(KeyCode.F11))
        {
            CursorWrap.UnlockCursor();
            Plugin.Log.LogDebug($"Unlocked cursor");
        }

        if (movieRunner.MovieEnd && Input.GetKeyDown(KeyCode.BackQuote))
        {
            Console.Opened = !Console.Opened;
        }
    }

    public static void OnGUI()
    {
        Console.Update();
        DrawGUI();

        if (ShowCursor && UnityCursorVisible)
            GUI.DrawTexture(
                new(Input.mousePosition.x, Screen.height - Input.mousePosition.y, currentTexture.width,
                    currentTexture.height), currentTexture);
    }

    private static int _tabIndex;
    private static readonly string[] tabs = { "Movie", "Debug" };
    private static readonly Texture2D BGSurround = new(MENU_SIZE_X, MENU_SIZE_Y);
    private static string filePath = "";

    private enum Tabs
    {
        Movie
    }

    private const int MENU_SIZE_X = 600;
    private const int MENU_SIZE_Y = 200;
    private const int MENU_X = 10;
    private const int MENU_Y = 10;
    private const int EDGE_SPACING = 5;

    private const int TAS_MOVIE_BROWSER_WIDTH = 1000;
    private const int TAS_MOVIE_BROWSER_HEIGHT = 400;

    // private static readonly FileBrowser tasMovieBrowser = new(
    //     Application.dataPath, new(
    //         Screen.width / 2 - TAS_MOVIE_BROWSER_WIDTH / 2,
    //         Screen.height / 2 - TAS_MOVIE_BROWSER_HEIGHT / 2,
    //         TAS_MOVIE_BROWSER_WIDTH,
    //         TAS_MOVIE_BROWSER_HEIGHT),
    //     "Select TAS Movie", 0, FileBrowser.FileBrowserType.Open, new[]
    //     {
    //         new FileBrowser.Extension("UniTAS Movie", new[] { "*.uti" }),
    //         new FileBrowser.Extension("Text file", new[] { "*.txt" }),
    //         new()
    //     });

    private static void DrawGUI()
    {
        if (!Enabled)
            return;

        var kernel = Plugin.Kernel;
        var movieRunner = kernel.GetInstance<IMovieRunner>();

        GUI.DrawTexture(new(MENU_X, MENU_Y, MENU_SIZE_X, MENU_SIZE_Y), BGSurround);
        GUI.Box(new(MENU_X, MENU_Y, MENU_SIZE_X, MENU_SIZE_Y), $"{MyPluginInfo.PLUGIN_NAME} Menu");
        GUILayout.BeginArea(new(MENU_X + EDGE_SPACING, MENU_Y + EDGE_SPACING + 30, MENU_SIZE_X - EDGE_SPACING * 2,
            MENU_SIZE_Y - EDGE_SPACING * 2 - 30));

        _tabIndex = GUILayout.Toolbar(_tabIndex, tabs);
        switch ((Tabs)_tabIndex)
        {
            case Tabs.Movie:
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Movie Path", GUILayout.Width(70));
                filePath = GUILayout.TextField(filePath);
                if (GUILayout.Button("Run", GUILayout.Width(40)))
                {
                    var rev = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
                    if (rev.Invoke(System.IO.File.Exists, filePath) && movieRunner.MovieEnd)
                    {
                        var text = rev.Invoke(System.IO.File.ReadAllText, filePath);
                        try
                        {
                            movieRunner.RunFromInput(text);
                        }
                        catch (System.Exception e)
                        {
                            Plugin.Log.LogError(e);
                        }
                    }
                }

                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    // tasMovieBrowser.Open();
                }

                GUILayout.EndHorizontal();
                break;
            }
            default:
                // debug
            {
                if (GUILayout.Button("test TAS") && movieRunner.MovieEnd)
                {
                    var path = "";
                    var rev = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
                    if (rev.Invoke(System.IO.File.Exists, "C:\\Users\\Yuki\\Documents\\test.uti"))
                        path = "C:\\Users\\Yuki\\Documents\\test.uti";
                    else if (rev.Invoke(System.IO.File.Exists,
                                 "/home/yuu0141/test.uti"))
                        path =
                            "/home/yuu0141/test.uti";
                    var file = rev.Invoke(System.IO.File.ReadAllText, path);
                    try
                    {
                        movieRunner.RunFromInput(file);
                    }
                    catch (System.Exception e)
                    {
                        Plugin.Log.LogError(e);
                    }
                }

                if (GUILayout.Button("Soft restart") && movieRunner.MovieEnd)
                {
                    var restart = kernel.GetInstance<IGameRestart>();
                    restart.SoftRestart(new(2000, 1, 1));
                }

                break;
            }
        }

        GUILayout.EndArea();

        // tasMovieBrowser.Update();
        // tasMovieBrowser.GetFinalPath(ref filePath);
    }
}