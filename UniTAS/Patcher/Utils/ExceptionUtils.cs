using System;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class ExceptionUtils
{
    public static void UnityLogErrorOnThrow<T>(Action<T> action, T arg)
    {
        try
        {
            action(arg);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void UnityLogErrorOnThrow<T1, T2>(Action<T1, T2> action, T1 arg, T2 arg2)
    {
        try
        {
            action(arg, arg2);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}