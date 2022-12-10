using UnityEngine;

namespace UniTASPlugin.GameOverlay.GameConsole;

public static class Console
{
    private static bool opened;

    public static bool Opened
    {
        get => opened;
        set
        {
            if (value)
                justOpened = true;
            opened = value;
        }
    }

    private static bool justOpened;
    private static bool windowJustOpened;
    private static Rect windowRect;
    private const float WIDTH_MULT = 0.6f;
    private const float HEIGHT_MULT = 0.55f;
    private const int ID = 1000;

    private static string content = "";
    private static string input = "";
    private const string INPUT_CONTROL_NAME = "ConsoleInput";
    private static Vector2 scrollPos = Vector2.zero;

    public static void Update()
    {
        if (!Opened)
            return;

        if (justOpened)
        {
            var width = Screen.width * WIDTH_MULT;
            var height = Screen.height * HEIGHT_MULT;
            windowRect = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
            windowJustOpened = true;
            justOpened = false;
        }

        windowRect = GUILayout.Window(ID, windowRect, Window, "UniTAS Console");
    }

    private static void Window(int id)
    {
        if (windowJustOpened)
            GUI.FocusWindow(id);

        GUI.DragWindow(new Rect(0, 0, 20000, 20));

        GUILayout.BeginVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        _ = GUILayout.TextArea(content, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        GUI.SetNextControlName(INPUT_CONTROL_NAME);
        input = GUILayout.TextArea(input, GUILayout.ExpandHeight(false));
        if (GUI.changed)
        {
            if (input.Contains("\n"))
            {
                input = input.Replace("\r\n", "");
                input = input.Replace("\n", "");
                ExecInput();
            }
            else if (input.Contains("`"))
            {
                input = input.Replace("`", "");
                Opened = false;
            }
        }

        if (windowJustOpened)
        {
            GUI.FocusControl(INPUT_CONTROL_NAME);
            windowJustOpened = false;
        }

        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
        {
            content = "";
            scrollPos = Vector2.zero;
        }

        if (GUILayout.Button("Run", GUILayout.ExpandWidth(false)))
        {
            Executor.Process(input);
            input = "";
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    public static void ExecInput()
    {
        Executor.Process(input);
        input = "";
    }

    public static void Print(string value)
    {
        content += $"{value}\n";
    }

    public static void Clear()
    {
        content = "";
        scrollPos = Vector2.zero;
    }
}