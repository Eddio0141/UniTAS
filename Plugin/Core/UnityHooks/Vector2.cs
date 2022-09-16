using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

public class Vector2 : Base<Vector2>, To
{
    public float x;
    public float y;

    static MethodInfo zeroGetter;
    static FieldInfo x_;
    static FieldInfo y_;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                zeroGetter = objType.GetMethod("zero", BindingFlags.Public | BindingFlags.Static);
                x_ = objType.GetField("x", BindingFlags.Instance | BindingFlags.Public);
                y_ = objType.GetField("y", BindingFlags.Instance | BindingFlags.Public);
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

    public static Vector2 zero
    {
        get
        {
            var zero = zeroGetter.Invoke(null, null);
            var x = xFieldGet(zero);
            var y = yFieldGet(zero);

            return new Vector2(x, y);
        }
    }

    public static float xFieldGet(object instance)
    {
        return (float)x_.GetValue(instance);
    }

    public static void xFieldSet(object instance, float value)
    {
        x_.SetValue(instance, value);
    }

    public static float yFieldGet(object instance)
    {
        return (float)y_.GetValue(instance);
    }

    public static void yFieldSet(object instance, float value)
    {
        y_.SetValue(instance, value);
    }
}
