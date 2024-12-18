using System;

namespace UniTAS.Patcher.Services;

public interface IPatchReverseInvoker
{
    bool Invoking { get; set; }
    void Invoke(Action method);
    void Invoke<T1>(Action<T1> method, T1 arg1);
    void Invoke<T1, T2, T3, T4, T5>(Action5<T1, T2, T3, T4, T5> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    TRet Invoke<TRet>(Func<TRet> method);
    TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1);
    TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2);

    delegate void Action5<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}