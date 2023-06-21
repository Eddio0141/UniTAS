using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class FinalizeSuppressor : IFinalizeSuppressor
{
    public bool DisableFinalizeInvoke
    {
        set => Utils.FinalizeSuppressor.DisableFinalizeInvoke = value;
    }
}