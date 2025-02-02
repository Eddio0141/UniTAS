using System;
using System.Diagnostics;
using System.Threading;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
// TODO: probably make this into a manual service
public class PatchReverseInvoker : IPatchReverseInvoker
{
    private readonly ThreadLocal<bool> _invoking = new(() => false);

    public bool Invoking
    {
        get => _invoking.Value;
        set
        {
            if (_invoking.Value == value)
            {
                StaticLogger.LogWarning($"reverse patcher is already in use for this thread at {new StackTrace(true)}");
                return;
            }

            _invoking.Value = value;
        }
    }

    public void Invoke(Action method)
    {
        _invoking.Value = true;
        method();
        _invoking.Value = false;
    }

    public void Invoke<T1>(Action<T1> method, T1 arg1)
    {
        _invoking.Value = true;
        method(arg1);
        _invoking.Value = false;
    }

    public void Invoke<T1, T2>(Action<T1, T2> method, T1 arg1, T2 arg2)
    {
        _invoking.Value = true;
        method(arg1, arg2);
        _invoking.Value = false;
    }

    public void Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _invoking.Value = true;
        method(arg1, arg2, arg3, arg4);
        _invoking.Value = false;
    }

    public void Invoke<T1, T2, T3, T4, T5>(IPatchReverseInvoker.Action5<T1, T2, T3, T4, T5> method, T1 arg1, T2 arg2,
        T3 arg3, T4 arg4, T5 arg5)
    {
        _invoking.Value = true;
        method(arg1, arg2, arg3, arg4, arg5);
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