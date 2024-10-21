using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class MouseCoordsOverlay(WindowDependencies windowDependencies)
    : BuiltInOverlay(windowDependencies, "Mouse coords")
{
    protected override AnchoredOffset DefaultOffset { get; } = new(0, 0, 0, 90);

    protected override string Update()
    {
        var mousePos = UnityInput.Current.mousePosition;
        return $"Mouse X: {mousePos.x}, Y: {mousePos.y}";
    }
}