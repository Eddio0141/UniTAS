using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Tabs;

public class MoviePlayTab : IMainMenuTab
{
    private string _tasPath = string.Empty;

    public string Name => "Movie Play";

    private string _tasRunInfo = string.Empty;

    public void Render(int windowID)
    {
        GUILayout.BeginVertical();
        TASPath();
        OperationButtons();
        TASRunInfo();
        GUILayout.EndVertical();
    }

    private void TASPath()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Movie Path", GUILayout.ExpandWidth(false));
        _tasPath = GUILayout.TextField(_tasPath);

        GUILayout.EndHorizontal();
    }

    private void OperationButtons()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Browse"))
        {
        }

        if (GUILayout.Button("Recent"))
        {
        }

        if (GUILayout.Button("Run"))
        {
        }

        GUILayout.EndHorizontal();
    }

    private void TASRunInfo()
    {
        GUILayout.TextArea(_tasRunInfo, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
    }
}