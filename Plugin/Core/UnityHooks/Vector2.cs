using Core.UnityHooks.Helpers;
using System;
using System.Globalization;

namespace Core.UnityHooks;

public class Vector2 : Base<Vector2>, To
{
    public float x;
    public float y;

    protected override void InitByUnityVersion(Type _, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                break;
        }
    }

    public object ConvertTo()
    {
        var newType = Activator.CreateInstance(ObjType);
        ObjType.GetField("x").SetValue(newType, x);
        ObjType.GetField("y").SetValue(newType, y);
        return newType;
    }
}
