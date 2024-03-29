using System;
using UniTAS.Patcher.Exceptions;

namespace UniTAS.Patcher.Utils;

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

    public static void AreEqual<T>(T expected, T actual, string message = null)
    {
        if (!expected.Equals(actual))
        {
            throw new RuntimeAssertException($"RuntimeAssert.AreEqual failed, expected {expected}, got {actual}." +
                                             (message == null ? "" : $" {message}"));
        }
    }

    public static void FloatEquals(float expected, float actual, float precision, string message = null)
    {
        if (Math.Abs(expected - actual) >= precision)
        {
            throw new RuntimeAssertException($"RuntimeAssert.FloatEquals failed, expected {expected}, got {actual}." +
                                             (message == null ? "" : $" {message}"));
        }
    }
}