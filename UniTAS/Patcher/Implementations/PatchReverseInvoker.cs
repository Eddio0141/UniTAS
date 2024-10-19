using System;
using System.Threading;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class PatchReverseInvoker : IPatchReverseInvoker
{
    private readonly ThreadLocal<bool> _invoking = new(() => false);
    public bool Invoking => _invoking.Value;

    public void Invoke(Action method)
    {
        _invoking.Value = true;
        method();
        _invoking.Value = false;
    }

    public TRet Invoke<TRet>(Func<TRet> method)
    {
        _invoking.Value = true;
        var ret = method();
        _invoking.Value = false;
        return ret;
    }

    public TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1)
    {
        _invoking.Value = true;
        var ret = method(arg1);
        _invoking.Value = false;
        return ret;
    }

    public TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2)
    {
        _invoking.Value = true;
        var ret = method(arg1, arg2);
        _invoking.Value = false;
        return ret;
    }
}