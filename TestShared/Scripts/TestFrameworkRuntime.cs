using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Extensions;
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
    private readonly List<Result> _generalTestResults = new List<Result>();
    private readonly List<Result> _initTestResults = new List<Result>();
    private readonly List<Result> _movieTestResults = new List<Result>();
    private Test[] _generalTests;
    private Test[] _eventTests;
    private MovieTest[] _movieTests;
    private Test[] _initTestsAwake;

    public static TestFrameworkRuntime Instance { get; private set; }

    private static bool _generalTestsDone;

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
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        Instance = this;
    }

    private bool _discoveredTests;

    private void DiscoverTestsIfNot()
    {
        if (_discoveredTests) return;
        _discoveredTests = true;
        var generalTests = new List<Test>();
        var eventTests = new List<Test>();
        var movieTests = new List<MovieTest>();
        var initTestsAwake = new List<Test>();

        foreach (var monoBeh in GetComponents<MonoBehaviour>())
        {
            var monoBehType = monoBeh.GetType();
            var movieTestAttr = monoBehType.GetCustomAttribute<MovieTestAttribute>();
            var methods = GetTestFuncs(monoBehType);
            var testsIter = methods.Select(m =>
            {
                var attr = m.GetCustomAttribute<TestAttribute>();
                return new Test(string.Format("{0}.{1}", monoBehType.FullName, m.Name), monoBehType.FullName, m, monoBeh, attr.EventTiming,
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
                            string.Format("Test {0} is a movie test and the event timing argument is ineffective", test.Name));
                    }
                }

                movieTests.Add(new MovieTest(monoBehType.FullName, movieTestAttr, testsIter));
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
        Debug.Log(string.Format("Discovered {0} general tests", _generalTests.Length) +
                  string.Format(", {0} event tests", _eventTests.Length) +
                  string.Format(", {0} movie tests", _movieTests.Length) +
                  string.Format(", {0} init tests (Awake)", _initTestsAwake.Length));
    }

    private static Test[] AllInitTests()
    {
        Instance.DiscoverTestsIfNot();
        return Instance._initTestsAwake;
    }

    public static IEnumerable<MethodInfo> GetTestFuncs(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic).Where(m => m.GetCustomAttributes<TestAttribute>().Any());
    }

    public static void RunTestsEditor()
    {
        if (!InstanceSetCheckAndLog()) return;
        Instance.DiscoverTestsIfNot();
        Instance.StartCoroutine(Instance.RunAllTests());
    }

    public static void RunGeneralTests(string[] tests = null)
    {
        if (!InstanceSetCheckAndLog()) return;
        Instance.DiscoverTestsIfNot();
        Instance.StartCoroutine(Instance.RunGeneralInternal(tests));
    }

    public static void ResetGeneralTests()
    {
        if (!InstanceSetCheckAndLog()) return;
        Instance.ResetGeneralTestsInternal();
    }

    private void ResetGeneralTestsInternal()
    {
        _generalTestResults.Clear();
        _generalTestsDone = false;
    }

    private static bool InstanceSetCheckAndLog()
    {
        if (Instance != null) return true;

        Debug.LogError("wait for the test runner instance to be instantiated");
        return false;
    }

    private IEnumerator RunGeneralInternal(string[] tests)
    {
        _generalTestsDone = false;

        if (tests != null)
        {
            tests = tests.Where(t => t == null || t.Trim().Length == 0).ToArray();
        }

        foreach (var test in _generalTests)
        {
            // filter
            if (tests != null && tests.Length != 0 && tests.All(t => !test.Name.Like(t))) continue;

            yield return TestCleanup();
            yield return TestSafetyDelay();

            yield return RunTest(test, _generalTestResults);

            yield return TestCleanup();
            yield return TestSafetyDelay();
        }

        _generalTestsDone = true;
        Debug.Log("General tests finished");
    }

    private IEnumerable<Test> AllTests
    {
        get { return _generalTests.Concat(_eventTests).Concat(_initTestsAwake); }
    }

    /// <summary>
    /// Only used internally for editor
    /// </summary>
    public IEnumerator RunAllTests()
    {
        // dummy result list
        var results = new List<Result>();
        foreach (var test in AllTests)
        {
            yield return TestCleanup();
            yield return TestSafetyDelay();

            yield return RunTest(test, results);

            yield return TestCleanup();
            yield return TestSafetyDelay();
        }

        Debug.Log("All tests finished");
    }

    /// <summary>
    /// Only used internally for editor
    /// </summary>
    public static IEnumerator RunTestByName(string name)
    {
        Instance.DiscoverTestsIfNot();

        var test = Instance.AllTests.FirstOrDefault(t => t.Name.Like(name));
        if (test.Name == null)
        {
            Debug.LogWarning("couldn't find test");
            yield break;
        }

        // if (tests.MoveNext())
        // {
        //     Debug.LogWarning("more than 1 match with test");
        // }

        yield return RunTest(test, new List<Result>());
    }

    private static IEnumerator RunTest(Test test, List<Result> results)
    {
        Debug.Log(string.Format("Running test {0}", test.Name));
        var executeIter = test.Execute();
        while (executeIter.MoveNext())
        {
            if (executeIter.Current is Result)
            {
                var result = (Result)executeIter.Current;
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

    private static IEnumerator TestCleanup()
    {
        Debug.Log("cleaning up...");

        var defaultWidth = 1920;
        var defaultHeight = 1080;
        if (Screen.width != defaultWidth || Screen.height != defaultHeight || Screen.fullScreen)
        {
            Screen.SetResolution(1920, 1080, false);
        }

        // restore default scene
        SceneManager.LoadScene(0);
        yield return null;

        var sceneCount = SceneManager.sceneCount;
        if (sceneCount != 1)
        {
            throw new InvalidProgramException(string.Format("cleanup failure, expected 1 scene to be ready but there are `{0}` scenes", sceneCount));
        }

        AssetBundle.UnloadAllAssetBundles(true);
        yield return Resources.UnloadUnusedAssets();

        Debug.Log("done cleanup");
    }

    private Test? _currentEventTest;

    public static IEnumerator AwakeTestHook()
    {
        if (!InstanceSetCheckAndLog()) yield break;

        yield return Instance.MovieTestCheckAndRun(MovieTestTiming.Awake);
        yield return Instance.InitTestCheckAndRun(InitTestTiming.Awake);
    }

    private void CheckExecTestFlag()
    {
        const string checkMsg = "check if you are trying to run init test / movie test multiple times without restart";

        if (_execTestRun)
        {
            throw new InvalidOperationException(string.Format("Execute test is already running, {0}", checkMsg));
        }

        if (_initTestMethodToRun != null && _movieTestClassToRun != null)
        {
            throw new InvalidOperationException(string.Format("Execute test flag is conflicting, {0}", checkMsg));
        }
    }

    private IEnumerator InitTestCheckAndRun(InitTestTiming timing)
    {
        if (_initTestMethodToRun == null) yield break;
        CheckExecTestFlag();
        // check format
        Debug.Log(string.Format("Init test is set to be executed: `{0}`", _initTestMethodToRun));
        DiscoverTestsIfNot();
        var testIdx = Array.FindIndex(_initTestsAwake, t => t.InitTiming == timing && t.Name == _initTestMethodToRun);
        if (testIdx < 0)
        {
            throw new InvalidOperationException("Init test not found");
        }
        _execTestRun = true;
        var test = _initTestsAwake[testIdx];

        yield return RunTest(test, _initTestResults);
    }

    private IEnumerator MovieTestCheckAndRun(MovieTestTiming movieTestTiming)
    {
        if (_movieTestClassToRun == null) yield break;
        CheckExecTestFlag();
        Debug.Log(string.Format("Movie test is set to be executed: `{0}`", _movieTestClassToRun));
        DiscoverTestsIfNot();
        var testPairIdx = Array.FindIndex(_movieTests, t => t.ClassName.Like(_movieTestClassToRun));
        if (testPairIdx < 0)
        {
            throw new InvalidOperationException("Movie test not found");
        }
        var movieTest = _movieTests[testPairIdx];
        var testTiming = movieTest.Attrs.Timing;
        if (testTiming != movieTestTiming)
        {
            Debug.Log(string.Format("Test found but mismatching timing, need timing {0} but current at {1}, skipping test execution", testTiming, movieTestTiming));
            yield break;
        }

        _execTestRun = true;
        var tests = movieTest.Tests;

        Debug.Log(string.Format("Running {0} movie tests", tests.Length));
        foreach (var test in tests)
        {
            yield return RunTest(test, _movieTestResults);
        }
    }

    private struct MovieTest
    {
        public readonly string ClassName;
        public readonly MovieTestAttribute Attrs;
        public readonly Test[] Tests;

        public MovieTest(string className, MovieTestAttribute attrs, Test[] tests)
        {
            ClassName = className;
            Attrs = attrs;
            Tests = tests;
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

    private struct Test : IEquatable<Test>
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
                    string.Format("Test {0} has event timing and init timing specified, choose one, ", name) +
                    "event timing are tests that can be ran at any point in the lifetime of unity games, " +
                    "init tests are ran automatically on the specified timing");
            }
        }

        private static string GetExceptionMsg(Exception ex)
        {
            if (ex.InnerException is AssertionException)
            {
                var assertionException = (AssertionException)ex.InnerException;
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
                testRet = _method.Invoke(_objInstance, new object[0]);
            }
            catch (Exception e)
            {
                success = false;
                if (msg == null)
                    msg = GetExceptionMsg(e);
            }

            if (!_testDoesIter || !success)
            {
                yield return new Result(Name, msg, success);
#pragma warning disable CS0618 // Type or member is obsolete
                Application.RegisterLogCallback(null);
#pragma warning restore CS0618 // Type or member is obsolete
                yield break;
            }

            var iter = (IEnumerator<TestYield>)testRet;
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
                    if (msg == null)
                        msg = GetExceptionMsg(e);
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
            if (!(obj is Test)) return false;
            var other = (Test)obj;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return (_method != null ? _method.GetHashCode() : 0);
        }
    }
}

public static class Assert
{
    /// <summary>
    /// <para>Asserts unity log.</para>
    /// <para>
    /// It is recommended you sandwich the function expected to produce the log with the following function
    /// <see cref="LogRecieved(string, string, int)"/>
    /// </para>
    /// </summary>
    public static void LogTrackNext()
    {
        Application.logMessageReceivedThreaded += LogHook;
    }

    public static void LogRecieved(LogType expectedType, string expectedLog = null, string message = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
        if (_logHookStore.Condition == null)
        {
            _logHookStore = default(LogHookStore);
            throw new AssertionException("assertion failed `no log recieved`{0}", message, file, line);
        }
        Equal(_logHookStore.Type, expectedType, message, file, line);
        if (expectedLog != null)
        {
            Equal(_logHookStore.Condition, expectedLog, message, file, line);
        }
        _logHookStore = default(LogHookStore);
    }

    private static LogHookStore _logHookStore;

    private static void LogHook(string condition, string _, LogType type)
    {
        Application.logMessageReceivedThreaded -= LogHook;
        _logHookStore = new LogHookStore(condition, type);
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
        catch (Exception e)
        {
            if (e is AssertionException) throw;
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

    public static void Equal(float left, float right, float precision, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {
        EqualBase(Mathf.Abs(left) - Mathf.Abs(right) <= precision, left, right, file, line, message, true);
    }

    public static void Equal(Vector2 left, Vector2 right, float precision, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {

        var diff = new Vector2(Mathf.Abs(left.x) - Mathf.Abs(right.x), Mathf.Abs(left.y) - Mathf.Abs(right.y));
        var result = diff.x <= precision && diff.y <= precision;
        EqualBase(result, left, right, file, line, message, true);
    }

    public static void Equal(Vector3 left, Vector3 right, float precision, string message = null,
        [CallerFilePath] string file = null,
        [CallerLineNumber] int line = 0)
    {

        var diff = new Vector3(Mathf.Abs(left.x) - Mathf.Abs(right.x), Mathf.Abs(left.y) - Mathf.Abs(right.y), Mathf.Abs(left.z) - Mathf.Abs(right.z));
        var result = diff.x <= precision && diff.y <= precision && diff.z <= precision;
        EqualBase(result, left, right, file, line, message, true);
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
        EqualBase(Equals(left, right), left, right, file, line, message, equal);
    }

    private static void EqualBase<T>(bool result, T left, T right, string file, int line, string message, bool equal)
    {
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
        public readonly LogType Type;
        public readonly string Condition;

        public LogHookStore(string condition, LogType type)
        {
            Condition = condition;
            Type = type;
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

public class SceneAsset : ITestAsset
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
    /// <summary>
    /// <param name="assetProperty">Name of the property that returns <see cref="ITestAsset"/> for this resource</param>
    /// </summary>
    public TestInjectPrefabAttribute(string assetProperty)
    {
        AssetProperty = assetProperty;
    }

    public string AssetProperty { get; }
}

/// <summary>
/// <para>Injects an asset accessible by <see cref="Resources"/> API</para>
/// <para>Accepted forms:</para>
/// <para>- <see cref="OnceOnlyPath"/> - Path to resource. Resource can only be used in a single test</para>
/// </summary>
public class TestInjectResource : TestInjectAttribute
{
    /// <summary>
    /// <param name="assetProperty">Name of the property that returns <see cref="ITestAsset"/> for this resource</param>
    /// </summary>
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
    /// <summary>
    /// <param name="assetProperty">Name of the property that returns <see cref="ITestAsset"/> for this resource</param>
    /// </summary>
    public TestInjectAssetBundle(string assetProperty)
    {
        AssetProperty = assetProperty;
    }

    public string AssetProperty { get; }
}

/// <summary>
/// You can only access the inner value once
/// </summary>
[Serializable]
public class OnceOnlyPath
{
    public const string InnerFieldName = "inner";

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

namespace Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
