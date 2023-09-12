using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace UniTAS.Patcher.Services.Trackers.TrackInfo;

public interface IClassStaticInfoTracker
{
    /// <summary>
    /// Contains the order in which static constructors were invoked.
    /// </summary>
    ReadOnlyCollection<Type> StaticCtorInvokeOrder { get; }

    /// <summary>
    /// Contains all static fields so far found.
    /// </summary>
    ReadOnlyCollection<FieldInfo> StaticFields { get; }
}