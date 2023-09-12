using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton(timing: RegisterTiming.Entry)]
public class ClassStaticInfoTracker : IClassStaticInfoTracker, IClassStaticInfoTrackerUpdate
{
    private readonly List<Type> _staticCtorInvokeOrder = new();
    private readonly List<FieldInfo> _staticFields = new();

    public ReadOnlyCollection<Type> StaticCtorInvokeOrder => _staticCtorInvokeOrder.AsReadOnly();
    public ReadOnlyCollection<FieldInfo> StaticFields => _staticFields.AsReadOnly();

    public void AddStaticCtorForTracking(Type type)
    {
        _staticCtorInvokeOrder.Add(type);
    }

    public void AddStaticFields(IEnumerable<FieldInfo> fields)
    {
        _staticFields.AddRange(fields);
    }
}