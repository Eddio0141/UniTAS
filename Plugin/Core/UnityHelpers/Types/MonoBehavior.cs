using System;

namespace Core.UnityHelpers.Types;

public interface MonoBehavior
{
    static void StopAllCoroutines(MonoBehavior instance) => throw new NotImplementedException();
}
