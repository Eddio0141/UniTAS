using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
public class MonoBehaviourController : IMonoBehaviourController
{
    public bool PausedExecution { get; set; }
}