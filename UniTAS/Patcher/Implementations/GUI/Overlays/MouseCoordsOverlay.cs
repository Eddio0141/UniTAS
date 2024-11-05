using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class MouseCoordsOverlay(WindowDependencies windowDependencies, IUnityInputWrapper inputWrapper)
    : BuiltInOverlay(windowDependencies, "Mouse coords")
{
    protected override AnchoredOffset DefaultOffset { get; } = new(0, 0, 0, 90);

    protected override string Update()
    {
        var mousePos = inputWrapper.GetMousePosition(false);
        return $"Mouse X: {mousePos.x}, Y: {mousePos.y}";
    }
}