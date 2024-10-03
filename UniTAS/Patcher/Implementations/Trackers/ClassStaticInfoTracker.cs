using System;
using System.Collections.ObjectModel;
using System.Reflection;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.Trackers.TrackInfo;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton(timing: RegisterTiming.Entry)]
public class ClassStaticInfoTracker : IClassStaticInfoTracker
{
    public ReadOnlyCollection<Type> StaticCtorInvokeOrder =>
        ManualServices.Trackers.ClassStaticInfoTracker.StaticCtorInvokeOrder;

    public ReadOnlyCollection<FieldInfo> StaticFields => ManualServices.Trackers.ClassStaticInfoTracker.StaticFields;
}