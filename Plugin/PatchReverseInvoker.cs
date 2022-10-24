using System;

namespace UniTASPlugin;

public class PatchReverseInvoker
{
    public bool Invoking { get; private set; }

    public TRet Invoke<TRet>(Func<object, TRet> method, object arg1)
    {
        Invoking = true;
        var ret = method.Invoke(arg1);
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