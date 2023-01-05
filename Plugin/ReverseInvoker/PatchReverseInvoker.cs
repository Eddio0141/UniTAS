using System;

namespace UniTASPlugin.ReverseInvoker;

// ReSharper disable once ClassNeverInstantiated.Global
public class PatchReverseInvoker
{
    public bool Invoking { get; set; }

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