using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Tabs;

public class TasTab : IMainMenuTab
{
    private string _tasPath = string.Empty;

    public string Name => "TAS";

    public void Render(int windowID)
    {
        TASPath();
        OperationButtons();
        TASRunInfo();
    }

    private void TASPath()
    {
        _tasPath = GUILayout.TextField(_tasPath);
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
        // TODO: Show TAS run info
    }
}