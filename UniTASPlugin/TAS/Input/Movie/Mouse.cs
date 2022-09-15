namespace UniTASPlugin.TAS.Input.Movie;

public class Mouse
{
    public float X;
    public float Y;

    public bool Left;
    public bool Right;
    public bool Middle;
    // TODO scroll

    public Mouse() : this(0, 0, false, false, false) { }

    public Mouse(float x, float y) : this(x, y, false, false, false) { }

    public Mouse(float x, float y, bool left) : this(x, y, left, false, false) { }

    public Mouse(float x, float y, bool left, bool right) : this(x, y, left, right, false) { }

    public Mouse(float x, float y, bool left, bool right, bool middle)
    {
        X = x;
        Y = y;
        Left = left;
        Right = right;
        Middle = middle;
    }
}
