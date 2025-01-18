using System.Collections;
using System.Collections.Generic;

namespace UniTAS.Patcher.Services.GameExecutionControllers;

public interface IMonoBehaviourController
{
    bool PausedExecution { get; set; }
    bool PausedUpdate { get; set; }
    HashSet<IEnumerator> IgnoreCoroutines { get; }
}