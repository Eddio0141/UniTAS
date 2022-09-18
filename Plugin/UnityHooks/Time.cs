using System;
using System.Reflection;
using UniTASPlugin.UnityHooks.Helpers;

namespace UniTASPlugin.UnityHooks;

#pragma warning disable IDE1006

internal class Time : Base<Time>
{
    static MethodInfo captureDeltaTimeGetter;
    static MethodInfo captureDeltaTimeSetter;
    static MethodInfo captureFramerateGetter;
    static MethodInfo captureFramerateSetter;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        var captureFramerate = objType.GetProperty("captureFramerate", BindingFlags.Public | BindingFlags.Static);
        captureFramerateGetter = captureFramerate.GetGetMethod();
        captureFramerateSetter = captureFramerate.GetSetMethod();
        var captureDeltaTime = objType.GetProperty("captureDeltaTime", BindingFlags.Public | BindingFlags.Static);
        // HACK
        if (captureDeltaTime != null)
        {
            captureDeltaTimeGetter = captureDeltaTime.GetGetMethod();
            captureDeltaTimeSetter = captureDeltaTime.GetSetMethod();
        }
    }

    public static float captureDeltaTime
    {
        // HACK cleaner handling of TAS tool controling framerate
        get
        {
            if (PluginInfo.UnityVersion < new Helper.SemanticVersion(2021, 2, 14))
            {
                return (int)captureFramerateGetter.Invoke(null, new object[] { });
            }
            else
            {
                return (float)captureDeltaTimeGetter.Invoke(null, new object[] { });
            }
        }
        set
        {
            if (PluginInfo.UnityVersion < new Helper.SemanticVersion(2021, 2, 14))
            {
                captureFramerateSetter.Invoke(null, new object[] { (int)value });
            }
            else
            {
                captureDeltaTimeSetter.Invoke(null, new object[] { value });
            }
        }
    }

    public static int captureFramerate
    {
        get => (int)captureFramerateGetter.Invoke(null, new object[] { });
        set => captureFramerateSetter.Invoke(null, new object[] { value });
    }
}
