using System;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IInputSystemTrackerUpdate
{
    void NewOnBeforeUpdateEvent(Action action);
}