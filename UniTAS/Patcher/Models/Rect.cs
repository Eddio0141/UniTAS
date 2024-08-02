namespace UniTAS.Patcher.Models;

/// <summary>
/// A rect struct, as same as unity's one
/// </summary>
public struct Rect
{
    public Rect(UnityEngine.Rect rect)
    {
        X = rect.x;
        Y = rect.y;
        Width = rect.width;
        Height = rect.height;
    }

    public UnityEngine.Rect ToUnityRect()
    {
        return new UnityEngine.Rect(X, Y, Width, Height);
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
