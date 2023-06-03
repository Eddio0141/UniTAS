using System;
using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Interfaces.RuntimeTest;

/// <summary>
/// Attribute for marking methods as runtime tests.
/// If a method returns a bool and returns false, the test will be skipped.
/// The skip check is only done before coroutines are started if the method returns a <see cref="Tuple{T1,T2}"/>.
/// You can return an <see cref="IEnumerator{T}"/> with <see cref="CoroutineWait"/> to turn the test into a coroutine.
/// In order to use multiple return types as defined above, use <see cref="Tuple{T1,T2}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RuntimeTestAttribute : Attribute
{
}