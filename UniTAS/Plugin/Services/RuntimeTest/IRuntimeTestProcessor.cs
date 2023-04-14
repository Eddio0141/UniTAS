using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.RuntimeTest;

namespace UniTAS.Plugin.Services.RuntimeTest;

public interface IRuntimeTestProcessor
{
    /// <summary>
    /// Runs all runtime tests with the attribute <see cref="RuntimeTestAttribute"/> in the assembly of the given type.
    /// </summary>
    /// <typeparam name="T">Checks assembly of this type.</typeparam>
    /// <returns>Test results</returns>
    void Test<T>();

    event DiscoveredTests OnDiscoveredTests;
    event TestRun OnTestRun;
    event TestEnd OnTestEnd;
}

public delegate void DiscoveredTests(int count);

public delegate void TestRun(string name);

public delegate void TestEnd(List<TestResult> results);