using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class MouseCoordsOverlay : BuiltInOverlay
{
    public MouseCoordsOverlay(IConfig config, IDrawing drawing) : base(config, drawing)
    {
    }

    protected override AnchoredOffset DefaultOffset { get; } = new(0, 0, 0, 60);
    protected override string ConfigName => "MouseCoords";

    protected override string Update()
    {
        var mousePos = Input.mousePosition;
        return $"Mouse X: {mousePos.x}, Y: {Screen.height - mousePos.y}";
    }
}