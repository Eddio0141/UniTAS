using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;

namespace UniTAS.Patcher.Interfaces.RuntimeTest;

/// <summary>
/// Attribute for marking methods as runtime tests.
/// If a method returns a bool and returns false, the test will be skipped.
/// The skip check is only done before coroutines are started if the method returns a <see cref="ValueTuple"/>.
/// You can return an <see cref="IEnumerable{T}"/> with <see cref="CoroutineWait"/> to turn the test into a coroutine.
/// In order to use multiple return types as defined above, use <see cref="Tuple"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RuntimeTestAttribute : Attribute
{
}