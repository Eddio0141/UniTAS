﻿using System;
using System.Reflection;

namespace UniTAS.Patcher.Services;

public interface IPatchReverseInvoker
{
    MethodInfo RecursiveReversePatch(MethodInfo original);
    bool InnerCall();
    void Return();
    void Invoke(Action method);
    TRet Invoke<TRet>(Func<TRet> method);
    TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1);
    TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2);
}