using System;
using System.IO;
using HarmonyLib;

namespace UniTASPlugin.ReverseInvoker;

// ReSharper disable once ClassNeverInstantiated.Global
public class PatchReverseInvoker
{
    private bool _invoking;

    public bool Invoking
    {
        get => _invoking;
        set
        {
            _invoking = value;
            AccessTools.Constructor(typeof(Path), searchForStatic: true).Invoke(null, null);
        }
    }

    public TRet Invoke<TRet>(Func<TRet> method)
    {
        Invoking = true;
        var ret = method.Invoke();
        Invoking = false;
        return ret;
    }

    public TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1)
    {
        Invoking = true;
        var ret = method.Invoke(arg1);
        Invoking = false;
        return ret;
    }

    public TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2)
    {
        Invoking = true;
        var ret = method.Invoke(arg1, arg2);
        Invoking = false;
        return ret;
    }
}