using System;

namespace UniTASPlugin.ReverseInvoker;

public class PatchReverseInvoker
{
    public bool Invoking { get; private set; }

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

    public void SetProperty<T>(Action<T> property, T value)
    {
        Invoking = true;
        property.Invoke(value);
        Invoking = false;
    }

    public T GetProperty<T>(Func<T> property)
    {
        Invoking = true;
        var ret = property.Invoke();
        Invoking = false;
        return ret;
    }
}