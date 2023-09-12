using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.SingletonBindings.Trackers;

public static class ClassStaticInfoTracker
{
    private static IClassStaticInfoTrackerUpdate _classStaticInfoTrackerUpdate;
    private static IClassStaticInfoTracker _classStaticInfoTracker;

    static ClassStaticInfoTracker()
    {
        ContainerStarter.RegisterContainerInitCallback(RegisterTiming.Entry,
            kernel =>
            {
                _classStaticInfoTrackerUpdate = kernel.GetInstance<IClassStaticInfoTrackerUpdate>();
                _classStaticInfoTracker = kernel.GetInstance<IClassStaticInfoTracker>();
            });
    }

    public static void AddStaticCtorForTracking(Type type)
    {
        // TODO make into singleton
        _classStaticInfoTrackerUpdate?.AddStaticCtorForTracking(type);
    }

    public static void AddStaticFields(IEnumerable<FieldInfo> fields)
    {
        _classStaticInfoTrackerUpdate?.AddStaticFields(fields);
    }

    public static ReadOnlyCollection<Type> StaticCtorInvokeOrder => _classStaticInfoTracker?.StaticCtorInvokeOrder;
}