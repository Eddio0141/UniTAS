namespace UniTASPlugin.GameEnvironment.InnerState;

public class WindowState
{
    public int Width { get; }
    public int Height { get; }
    public bool IsFullscreen { get; }
    public bool IsFocused { get; }

    public WindowState(int width, int height, bool isFullscreen, bool isFocused)
    {
        Width = width;
        Height = height;
        IsFullscreen = isFullscreen;
        IsFocused = isFocused;
    }

    public override string ToString()
    {
        return $"Width: {Width}, Height: {Height}, IsFullscreen: {IsFullscreen}, IsFocused: {IsFocused}";
    }
}