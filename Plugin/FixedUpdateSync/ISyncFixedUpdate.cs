using System;

namespace UniTASPlugin.FixedUpdateSync;

public interface ISyncFixedUpdate
{
    void OnSync(Action callback);
}