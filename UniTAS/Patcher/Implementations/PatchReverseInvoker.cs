using System;
using System.Diagnostics;
using System.Reflection;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class PatchReverseInvoker(ILogger logger) : IPatchReverseInvoker
{
    public bool Invoking => _depth > 0;
    private uint _depth;

    public MethodInfo RecursiveReversePatch(MethodInfo original) =>
        PatchReverseInvokerManual.RecursiveReversePatch(original);

    public bool InnerCall()
    {
        if (_depth > 0)
        {
            _depth++;
        }

        return _depth > 0;
    }

    public void Return()
    {
        switch (_depth)
        {
            case 1:
                logger.LogError($"Something went wrong returning from reverse invoking, {new StackTrace()}");
                return;
            case > 0:
                _depth--;
                break;
        }
    }

    public void Invoke(Action method)
    {
        StartInvoke();
        method.Invoke();
        FinishInvoke();
    }

    public TRet Invoke<TRet>(Func<TRet> method)
    {
        StartInvoke();
        var ret = method.Invoke();
        FinishInvoke();
        return ret;
    }

    public TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1)
    {
        StartInvoke();
        var ret = method.Invoke(arg1);
        FinishInvoke();
        return ret;
    }

    public TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2)
    {
        StartInvoke();
        var ret = method.Invoke(arg1, arg2);
        FinishInvoke();
        return ret;
    }

    private void StartInvoke()
    {
        _depth = 1;
    }

    private void FinishInvoke()
    {
        if (_depth != 1)
        {
            logger.LogWarning($"Depth isn't matching 1 after reverse invoking, {new StackTrace()}");
        }

        _depth = 0;
    }
}