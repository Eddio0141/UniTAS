using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(0)]
public class TestFrameworkRuntime : MonoBehaviour
{
    public const string AssetPath = "Assets/TestFramework";
    public const string SceneAssetPath = AssetPath + "/Scenes";
    public const string PrefabAssetPath = AssetPath + "/Prefabs";
    public const string TestingScenePath = AssetPath + "/Scenes/general.unity";
    public const string ResourcesPath = AssetPath + "/Resources";
    public const string AssetBundlePath = AssetPath + "/AssetBundles";
    public const string BuildPath = "build";

    private static TestFrameworkRuntime _instance;
    private readonly List<Result> _generalTestResults = new List<Result>();
    private readonly List<Result> _initTestResults = new List<Result>();
    private readonly List<Result> _movieTestResults = new List<Result>();
    private Test[] _generalTests;
    private Test[] _eventTests;
    private (MovieTestAttribute, Test[])[] _movieTests;
    private Test[] _initTestsAwake;

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static bool _generalTestsDone;
#pragma warning restore CS0414 // Field is assigned but its value is never used

    /// <summary>
    /// Movie test class to run by name, setting this flag will make certain events check / start running movie tests
    /// </summary>
    private static string _movieTestClassToRun;

    /// <summary>
    /// Init test to run by name
    /// </summary>
    private static string _initTestMethodToRun;

    private bool _execTestRun;

    private void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        _instance = this;
    }

    private bool _discoveredTests;

    private void DiscoverTestsIfNot()
    {
        if (_discoveredTests) return;
        _discoveredTests = true;
        var generalTests = new List<Test>();
        var eventTests = new List<Test>();
        var movieTests = new List<(MovieTestAttribute, Test[])>();
        var initTestsAwake = new List<Test>();

        foreach (var monoBeh in GetComponents<MonoBehaviour>())
        {
            var monoBehType = monoBeh.GetType();
            var movieTestAttr = monoBehType.GetCustomAttribute<MovieTestAttribute>();
            var methods = GetTestFuncs(monoBehType);
            var testsIter = methods.Select(m =>
            {
                var attr = m.GetCustomAttribute<TestAttribute>();
                return new Test($"{monoBehType.FullName}.{m.Name}", monoBehType.FullName, m, monoBeh, attr.EventTiming,
                    attr.InitTestTiming);
            }).ToArray();
            if (movieTestAttr != null)
            {
                foreach (var test in testsIter)
                {
                    if (test.EventTiming.HasValue)
                    {
                        // TODO: why warn here? the tests discovery isn't used in setup
                        Debug.LogWarning(
                            $"Test {test.Name} is a movie test and the event timing argument is ineffective");
                    }
                }

                movieTests.Add((movieTestAttr, testsIter));
                continue;
            }

            generalTests.AddRange(testsIter.Where(t => !t.EventTiming.HasValue && !t.InitTiming.HasValue));
            initTestsAwake.AddRange(testsIter.Where(t => t.InitTiming.HasValue));
            eventTests.AddRange(testsIter.Where(t => t.EventTiming.HasValue));
        }

        _generalTests = generalTests.ToArray();
        _eventTests = eventTests.ToArray();
        _movieTests = movieTests.ToArray();
        _initTestsAwake = initTestsAwake.ToArray();
        Debug.Log($"Discovered {_generalTests.Length} general tests" +
                  $", {_eventTests.Length} event tests" +
                  $", {_movieTests.Length} movie tests" +
                  $", {_initTestsAwake.Length} init tests (Awake)");
    }

    private static Test[] AllInitTests()
    {
        _instance.DiscoverTestsIfNot();
        return _instance._initTestsAwake;
    }

    public static IEnumerable<MethodInfo> GetTestFuncs(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic).Where(m => m.GetCustomAttributes<TestAttribute>().Any());
    }

    public static void RunGeneralTests()
    {
        if (!InstanceSetCheckAndLog()) return;
        _instance.DiscoverTestsIfNot();
        _instance.StartCoroutine(_instance.RunGeneralInternal());
    }

    public static void ResetGeneralTests()
    {
        if (!InstanceSetCheckAndLog()) return;
        _instance.ResetGeneralTestsInternal();
    }

    private void ResetGeneralTestsInternal()
    {
        _generalTestResults.Clear();
        _generalTestsDone = false;
    }

    private static bool InstanceSetCheckAndLog()
    {
        if (_instance != null) return true;

        Debug.LogError("wait for the test runner instance to be instantiated");
        return false;
    }

    private IEnumerator RunGeneralInternal()
    {
        _generalTestsDone = false;

        foreach (var test in _generalTests)
        {
            yield return null;
            yield return RunTest(test, _generalTestResults);
            yield return TestSafetyDelay();
        }

        TestCleanup();

        _generalTestsDone = true;
        Debug.Log("General tests finished");
    }

    private static IEnumerator RunTest(Test test, List<Result> results)
    {
        Debug.Log($"Running test {test.Name}");
        var executeIter = test.Execute();
        while (executeIter.MoveNext())
        {
            if (executeIter.Current is Result result)
            {
                Debug.Log(result);
                results.Add(result);
                break;
            }

            yield return executeIter.Current;
        }
    }

    private static IEnumerator TestSafetyDelay()
    {
        for (var i = 0; i < 5; i++)
        {
            yield return null;
        }
    }

    private static void TestCleanup()
    {
        Debug.Log("cleaning up...");

        // restore default scene
        SceneManager.LoadScene(0);

        Debug.Log("done cleanup");
    }

    private Test? _currentEventTest;

    public static IEnumerator AwakeTestHook()
    {
        if (!InstanceSetCheckAndLog()) yield break;

        yield return _instance.MovieTestCheckAndRun(MovieTestTiming.Awake);
        yield return _instance.InitTestCheckAndRun(InitTestTiming.Awake);
    }

    private void CheckExecTestFlag()
    {
        const string checkMsg = "check if you are trying to run init test / movie test multiple times without restart";

        if (_execTestRun)
        {
            throw new InvalidOperationException($"Execute test is already running, {checkMsg}");
        }

        if (_initTestMethodToRun != null && _movieTestClassToRun != null)
        {
            throw new InvalidOperationException($"Execute test flag is conflicting, {checkMsg}");
        }
    }

    private IEnumerator InitTestCheckAndRun(InitTestTiming timing)
    {
        if (_initTestMethodToRun == null) yield break;
        CheckExecTestFlag();
        // check format
        Debug.Log($"Init test is set to be executed: `{_initTestMethodToRun}`");
        DiscoverTestsIfNot();
        var testIdx = Array.FindIndex(_initTestsAwake, t => t.InitTiming == timing && t.Name == _initTestMethodToRun);
        if (testIdx < 0)
        {
            throw new InvalidOperationException("Init test not found");
        }
        _execTestRun = true;
        var test = _initTestsAwake[testIdx];

        yield return RunTest(test, _initTestResults);
        Debug.Log(_initTestResults[0]);
    }

    private IEnumerator MovieTestCheckAndRun(MovieTestTiming movieTestTiming)
    {
        if (_movieTestClassToRun == null) yield break;
        CheckExecTestFlag();
        Debug.Log($"Movie test is set to be executed: `{_movieTestClassToRun}`");
        DiscoverTestsIfNot();
        var testPairIdx = Array.FindIndex(_movieTests, t => t.Item1.Timing == movieTestTiming);
        if (testPairIdx < 0)
        {
            throw new InvalidOperationException("Movie test not found");
        }
        _execTestRun = true;
        var testPair = _movieTests[testPairIdx];
        var tests = testPair.Item2.Where(t => t.TypeName == _movieTestClassToRun).ToArray();

        Debug.Log($"Running {tests.Length} movie tests");
        foreach (var test in testPair.Item2)
        {
            yield return RunTest(test, _movieTestResults);
        }
    }

    private struct Result
    {
        public Result(string name, string message, bool success)
        {
            Name = name;
            Message = message;
            Success = success;
        }

        public readonly string Name;
        public readonly string Message;
        public readonly bool Success;

        public override string ToString()
        {
            return Success ? string.Format("success: {0}", Name) : string.Format("failure: {0}: {1}", Name, Message);
        }
    }

    private readonly struct Test : IEquatable<Test>
    {
        public readonly string Name;
        public readonly string TypeName;
        private readonly MethodInfo _method;
        private readonly bool _testDoesIter;
        private readonly MonoBehaviour _objInstance;
        public readonly EventTiming? EventTiming;
        public readonly InitTestTiming? InitTiming;

        public Test(string name, string typeName, MethodInfo method, MonoBehaviour objInstance,
            EventTiming? eventTiming,
            InitTestTiming? initTiming)
        {
            Name = name;
            TypeName = typeName;
            _method = method;
            _objInstance = objInstance;
            EventTiming = eventTiming;
            InitTiming = initTiming;
            _testDoesIter = method.ReturnType == typeof(IEnumerator<TestYield>);

            if (EventTiming != null && InitTiming != null)
            {
                throw new InvalidOperationException(
                    $"Test {name} has event timing and init timing specified, choose one, " +
                    "event timing are tests that can be ran at any point in the lifetime of unity games, " +
                    "init tests are ran automatically on the specified timing");
            }
        }

        private static string GetExceptionMsg(Exception ex)
        {
            if (ex.InnerException is AssertionException assertionException)
            {
                return assertionException.Message;
            }

            return ex.ToString();
        }

        /// <summary>
        /// Executes test, check return value for result
        /// </summary>
        /// <returns>Either unity coroutine yields or test result which indicates the test has finished</returns>
        public IEnumerator Execute()
        {
            string msg = null;
            var success = true;
            object testRet = null;

#pragma warning disable CS0618 // Type or member is obsolete
            Application.RegisterLogCallback((condition, _, type) =>
            {
                if (type != LogType.Exception) return;
                success = false;
                msg = condition;
            });
#pragma warning restore CS0618 // Type or member is obsolete

            try
            {
                testRet = _method.Invoke(_objInstance, Array.Empty<object>());
            }
            catch (Exception e)
            {
                success = false;
                msg ??= GetExceptionMsg(e);
            }

            if (!_testDoesIter || !success)
            {
                yield return new Result(Name, msg, success);
#pragma warning disable CS0618 // Type or member is obsolete
                Application.RegisterLogCallback(null);
#pragma warning restore CS0618 // Type or member is obsolete
                yield break;
            }

            var iter = (IEnumerator<TestYield>)testRet!;
            while (true)
            {
                bool moveNextResult;
                try
                {
                    moveNextResult = iter.MoveNext();
                }
                catch (Exception e)
                {
                    success = false;
                    msg ??= GetExceptionMsg(e);
                    break;
                }

                if (!moveNextResult) break;

                if (iter.Current == null)
                {
                    success = false;
                    msg = "Error: test yield returned null, which isn't expected";
                    break;
                }

                yield return iter.Current.Operation();
            }

            yield return new Result(Name, msg, success);
#pragma warning disable CS0618 // Type or member is obsolete
            Application.RegisterLogCallback(null);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public bool Equals(Test other)
        {
            return Equals(_method, other._method);
        }

        public override bool Equals(object obj)
        {
            return obj is Test other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_method != null ? _method.GetHashCode() : 0);
        }
    }
}

public static class Assert
{
    public static void Log(string name, LogType expectedType, string expectedLog, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        _logHookStore = new LogHookStore(name, expectedType, expectedLog, message, file, line);
        Application.logMessageReceived += LogHook;
    }

    private static LogHookStore _logHookStore;

    private static void LogHook(string condition, string _, LogType type)
    {
        Application.logMessageReceived -= LogHook;

        var name = _logHookStore.Name;
        var file = _logHookStore.File;
        var line = _logHookStore.Line;
        Result result;
        if (_logHookStore.ExpectedType == type && _logHookStore.ExpectedLog == condition)
        {
            result = new Result(name, null, true);
        }
        else
        {
            var fullMsg = new StringBuilder();
            fullMsg.AppendLine("assertion failed `expected_log` == `actual_log` && `expected_msg` == `actual_msg`{0}");
            if (_logHookStore.ExpectedType != type)
            {
                fullMsg.AppendFormat(" expected_log: {0}", _logHookStore.ExpectedType).AppendLine();
                fullMsg.AppendFormat("   actual_log: {0}", type).AppendLine();
            }

            if (_logHookStore.ExpectedLog != condition)
            {
                fullMsg.AppendFormat(" expected_msg: {0}", ShowHiddenChars(_logHookStore.ExpectedLog)).AppendLine();
                fullMsg.AppendFormat("   actual_msg: {0}", ShowHiddenChars(condition)).AppendLine();
            }

            var fullMsgStr = AssertMsg(name, fullMsg.ToString(), _logHookStore.Message, file, line);
            result = new Result(name, fullMsgStr, false);
        }

        LogAssert(name, file, line, result);
        TestResults.Add(result);
    }

    private static string ShowHiddenChars(string str)
    {
        if (str == null) return null;
        return str.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    public static void Null<T>(T actual, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
        where T : class
    {
        if (actual == null) return;
        throw new AssertionException(string.Format("assertion failed `actual` == null{{0}}\n actual: {0}", actual),
            message, file, line);
    }

    public static void NotNull<T>(T actual, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
        where T : class
    {
        if (actual != null) return;
        throw new AssertionException("assertion failed `actual` != null{0}", message, file, line);
    }

    public static void True(bool success, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        if (success) return;
        throw new AssertionException("assertion failed{0}", message, file, line);
    }

    public static void False(bool success, string message = null, [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        if (!success) return;
        throw new AssertionException("assertion failed{0}", message, file, line);
    }

    public static void Throws<T>(T expected, Action action, string message = null,
        [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        where
        T : Exception
    {
        try
        {
            action();
            var msg = new StringBuilder();
            msg.AppendLine("assertion failed throw `expected`{0}");
            msg.AppendFormat(" expected: {0}: {1}", expected.GetType().FullName, expected.Message);
            throw new AssertionException(msg.ToString(), message, file, line);
        }
        catch (Exception e) when (e is not AssertionException)
        {
            if (e.GetType() == expected.GetType() && e.Message == expected.Message)
                return;

            var msg = new StringBuilder();
            msg.AppendLine("assertion failed `expected` == `actual`{0}");
            msg.AppendFormat(" expected: {0}: {1}", expected.GetType().FullName, expected.Message);
            msg.AppendLine();
            msg.AppendFormat("   actual: {0}: {1}", e.GetType().FullName, e.Message);
            throw new AssertionException(msg.ToString(), message, file, line);
        }
    }

    public static void Equal<T>(T left, T right, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        EqualBase(left, right, file, line, message, true);
    }

    public static void NotEqual<T>(T left, T right, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        EqualBase(left, right, file, line, message, false);
    }

    private static void EqualBase<T>(T left, T right, string file, int line, string message, bool equal)
    {
        var result = Equals(left, right);
        if (result == equal) return;
        var assertMsg = new StringBuilder();
        assertMsg.AppendFormat("assertion failed `left` {0}= `right`{{0}}", equal ? "=" : "!");
        assertMsg.AppendLine();
        if (typeof(T) == typeof(string) && left != null && right != null)
        {
            var sLeft = (string)(object)left;
            var sRight = (string)(object)right;
            sLeft = ShowHiddenChars(sLeft);
            sRight = ShowHiddenChars(sRight);
            assertMsg.AppendFormat(" left: {0}", sLeft).AppendLine();
            assertMsg.AppendLine();
            assertMsg.AppendFormat("   right: {0}", sRight).AppendLine();
        }
        else
        {
            assertMsg.AppendFormat(" left: {0}", left).AppendLine();
            assertMsg.AppendLine();
            assertMsg.AppendFormat("   right: {0}", right).AppendLine();
        }

        throw new AssertionException(assertMsg.ToString(), message, file, line);
    }

    private static string AssertMsg(string name, string assertMsg, string userMsg, string file, int line)
    {
        userMsg = userMsg == null ? string.Empty : string.Format(": {0}", userMsg);
        return string.Format("test {0} failed at {1}:{2}:\n", name, file, line) + string.Format(assertMsg, userMsg);
    }

    private static void LogAssert(string name, string file, int line, Result result)
    {
        Debug.Log(string.Format("Assertion `{0}` at {1}:{2}, {3}", name, file, line,
            result.Success ? "success" : "failure"));
    }

    private static readonly List<Result> TestResults = new List<Result>();
    private static bool _testsDone;

    private static void Reset()
    {
        _testsDone = false;
        TestResults.Clear();
    }

    public static void Finish()
    {
        _testsDone = true;
        Debug.Log("tests finished");
        foreach (var result in TestResults)
        {
            Debug.Log(result);
        }
    }

    private struct Result
    {
        public Result(string name, string message, bool success)
        {
            Name = name;
            Message = message;
            Success = success;
        }

        public readonly string Name;
        public readonly string Message;
        public readonly bool Success;

        public override string ToString()
        {
            return Success ? string.Format("success: {0}", Name) : string.Format("failure: {0}: {1}", Name, Message);
        }
    }

    private struct LogHookStore
    {
        public readonly string Name;
        public readonly LogType ExpectedType;
        public readonly string ExpectedLog;
        public readonly string Message;
        public readonly string File;
        public readonly int Line;

        public LogHookStore(string name, LogType expectedType, string expectedLog, string message, string file,
            int line)
        {
            Name = name;
            ExpectedType = expectedType;
            ExpectedLog = expectedLog;
            Message = message;
            File = file;
            Line = line;
        }
    }
}

public class AssertionException : Exception
{
    public AssertionException(string assertMsg, string userMsg, string file, int line) : base(AssertMsg(assertMsg,
        userMsg, file, line))
    {
    }

    private static string AssertMsg(string assertMsg, string userMsg, string file, int line)
    {
        userMsg = userMsg == null ? string.Empty : string.Format(": {0}", userMsg);
        return string.Format("Assertion failed at {0}:{1}:\n", file, line) + string.Format(assertMsg, userMsg);
    }
}

// test yield
public abstract class TestYield
{
    public abstract IEnumerator Operation();
}

public class UnityYield : TestYield
{
    private readonly object _yield;

    public UnityYield(object yield)
    {
        _yield = yield;
    }

    public override IEnumerator Operation()
    {
        yield return _yield;
    }
}

public class SceneSwitchYield : TestYield
{
    private readonly string _scenePath;

    public SceneSwitchYield(string scenePath)
    {
        _scenePath = scenePath;
    }

    public override IEnumerator Operation()
    {
        Helper.Scene.LoadScene(_scenePath);
        yield break;
    }
}

// test attributes
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class TestAttribute : Attribute
{
    public readonly EventTiming? EventTiming;
    public readonly InitTestTiming? InitTestTiming;

    // for some reason unity hates it when the constructor takes the nullable as an argument
    public TestAttribute()
    {
        EventTiming = null;
        InitTestTiming = null;
    }

    public TestAttribute(EventTiming eventTiming)
    {
        EventTiming = eventTiming;
    }

    public TestAttribute(InitTestTiming initTestTiming)
    {
        InitTestTiming = initTestTiming;
    }
}

public enum EventTiming
{
}

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class MovieTestAttribute : Attribute
{
    public readonly MovieTestTiming Timing;

    public MovieTestAttribute(MovieTestTiming timing)
    {
        Timing = timing;
    }
}

public enum MovieTestTiming
{
    Awake
}

public enum InitTestTiming
{
    Awake
}

// injection attributes

/// <summary>
/// A unity asset that is declarative
/// </summary>
public interface ITestAsset
{
}

public class GameObjectAsset : ITestAsset
{
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class TestInjectAttribute : Attribute
{
}

/// <summary>
/// <para>Injects a scene</para>
/// <para>Accepted forms:</para>
/// <para>- <see cref="string"/> - Scene path</para>
/// </summary>
public class TestInjectSceneAttribute : TestInjectAttribute
{
}

/// <summary>
/// <para>Injects a prefab</para>
/// <para>Accepted forms:</para>
/// <para>- <see cref="GameObject"/> - Reference to the prefab</para>
/// </summary>
public class TestInjectPrefabAttribute : TestInjectAttribute
{
}

/// <summary>
/// <para>Injects an asset accessible by <see cref="Resources"/> API</para>
/// <para>Accepted forms:</para>
/// <para>- <see cref="OnceOnlyPath"/> - Path to resource. Resource can only be used in a single test</para>
/// </summary>
public class TestInjectResource : TestInjectAttribute
{
    /// <param name="assetProperty">Name of the property that returns <see cref="ITestAsset"/> for this resource</param>
    public TestInjectResource(string assetProperty)
    {
        AssetProperty = assetProperty;
    }

    public string AssetProperty { get; }
}

/// <summary>
/// <para>Injects an asset bundle accessible by <see cref="AssetBundle"/> API</para>
/// <para>Accepted forms:</para>
/// <para>- <see cref="OnceOnlyPath"/> - Path to asset bundle. Asset can only be used in a single test</para>
/// </summary>
public class TestInjectAssetBundle : TestInjectAttribute
{
    /// <param name="assetProperty">Name of the property that returns <see cref="ITestAsset"/> for this resource</param>
    public TestInjectAssetBundle(string assetProperty)
    {
        AssetProperty = assetProperty;
    }

    public string AssetProperty { get; }
}

public static class Helper
{
    public static class Scene
    {
        public static void LoadScene(string scene)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Application.LoadLevel(scene);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}

/// <summary>
/// You can only access the inner value once
/// </summary>
[Serializable]
public class OnceOnlyPath
{
    public const string InnerFieldName = nameof(inner);

    [SerializeField]
    private string inner;
    private bool _used;

    private OnceOnlyPath(string inner)
    {
        this.inner = inner;
    }

    public static implicit operator OnceOnlyPath(string from)
    {
        return new OnceOnlyPath(from);
    }

    public static implicit operator string(OnceOnlyPath from)
    {
        if (from._used)
        {
            throw new InvalidOperationException("Inner value was already accessed");
        }

        from._used = true;
        return from.inner;
    }
}
