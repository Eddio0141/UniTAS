using Core.UnityHooks.Helpers;
using System;
using System.Globalization;

namespace Core.UnityHooks;

public class Vector2 : Base<Vector2>, To
{
    public float x;
    public float y;

#pragma warning disable IDE1006
    public static Vector2 zero { get => new(0, 0); }
#pragma warning restore IDE1006

    public Vector2() : this(0, 0) { }

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

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

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static Vector2 operator *(Vector2 a, float b)
    {
        return new Vector2(a.x * b, a.y * b);
    }

    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    public static Vector2 operator /(Vector2 a, float b)
    {
        return new Vector2(a.x / b, a.y / b);
    }

    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x / b.x, a.y / b.y);
    }

    public static bool operator ==(Vector2 a, Vector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2 a, Vector2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

    public string ToString(string format)
    {
        return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        bool nullOrEmpty = string.IsNullOrEmpty(format);
        if (nullOrEmpty)
        {
            format = "F2";
        }
        bool formatProviderIsNull = formatProvider == null;
        if (formatProviderIsNull)
        {
            formatProvider = CultureInfo.InvariantCulture.NumberFormat;
        }
        return string.Format(CultureInfo.InvariantCulture.NumberFormat, "({0}, {1})", new object[]
        {
            x.ToString(format, formatProvider),
            y.ToString(format, formatProvider)
        });
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() << 2;
    }

    public override bool Equals(object other)
    {
        bool flag = other is not Vector2;
        return !flag && Equals((Vector2)other);
    }

    public bool Equals(Vector2 other)
    {
        return x == other.x && y == other.y;
    }
}
