using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Implementations;

public partial class MainMenu
{
    private const int MenuX = 10;
    private const int MenuY = 10;

    private const int MenuSizeX = 600;
    private const int MenuSizeY = 200;

    private const int EdgeSpacing = 5;

    private readonly Texture2D _bgSurround = new(MenuSizeX, MenuSizeY);

    private void RenderBackground()
    {
        UnityEngine.GUI.DrawTexture(new(MenuX, MenuY, MenuSizeX, MenuSizeY), _bgSurround);
        UnityEngine.GUI.Box(new(MenuX, MenuY, MenuSizeX, MenuSizeY), $"{MyPluginInfo.PLUGIN_NAME} Menu");
        GUILayout.BeginArea(new(MenuX + EdgeSpacing, MenuY + EdgeSpacing + 30, MenuSizeX - EdgeSpacing * 2,
            MenuSizeY - EdgeSpacing * 2 - 30));
    }
    
    private void FinishRenderBackground()
    {
        GUILayout.EndArea();
    }
}