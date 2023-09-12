using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IClassStaticInfoTrackerUpdate
{
    void AddStaticCtorForTracking(Type type);
    void AddStaticFields(IEnumerable<FieldInfo> fields);
}