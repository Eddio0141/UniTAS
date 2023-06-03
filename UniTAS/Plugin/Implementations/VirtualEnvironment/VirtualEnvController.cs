using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
public class VirtualEnvController : IVirtualEnvController
{
    private readonly IOnVirtualEnvStatusChange[] _onVirtualEnvStatusChange;

    private bool _runVirtualEnvironment;

    public VirtualEnvController(IOnVirtualEnvStatusChange[] onVirtualEnvStatusChange)
    {
        _onVirtualEnvStatusChange = onVirtualEnvStatusChange;
    }

    public event VirtualEnvStatusChange OnVirtualEnvStatusChange;

    public bool RunVirtualEnvironment
    {
        get => _runVirtualEnvironment;
        set
        {
            if (_runVirtualEnvironment == value) return;
            _runVirtualEnvironment = value;

            OnVirtualEnvStatusChange?.Invoke(value);

            foreach (var onVirtualEnvStatusChange in _onVirtualEnvStatusChange)
            {
                onVirtualEnvStatusChange.OnVirtualEnvStatusChange(value);
            }
        }
    }
}