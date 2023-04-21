using UniTAS.Plugin.Exceptions;

namespace UniTAS.Plugin.Utils;

public static class RuntimeAssert
{
    public static void True(bool condition, string message = null)
    {
        if (!condition)
        {
            throw new RuntimeAssertException("RuntimeAssert.True failed" + (message == null ? "" : $" {message}"));
        }
    }

    public static void False(bool condition, string message = null)
    {
        if (condition)
        {
            throw new RuntimeAssertException("RuntimeAssert.False failed" + (message == null ? "" : $" {message}"));
        }
    }
}