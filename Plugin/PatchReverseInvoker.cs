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

    public void SetProperty(Action<object> property, object value)
    {
        property.Invoke(value);
    }
}