using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor.UniTASTest
{
    public static class TestFrameworkSetup
    {
        [MenuItem("Test/Setup ^#s")]
        private static void Setup()
        {
            Debug.Log("Loading UniTAS testing framework");
            InitDirs();
            var (sharedScriptsDir, sharedEditorDir, testsDir) = GetRepoDirs();
            LinkRunnerFiles(sharedScriptsDir, sharedEditorDir);
            InitTestScene();
            LinkAndAddTests(testsDir);

            Debug.Log("Domain reload");
            DomainReload();
        }

        private static void DomainReload()
        {
            // note that we want to force recompilation, and this isn't possible even on latest unity
            var dummyScript = Path.Combine(TestFrameworkRuntime.AssetPath, "dummyScript.cs");
            File.Create(dummyScript).Dispose();
            AssetDatabase.ImportAsset(dummyScript);
        }

        private static bool _preventAfterReload;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AfterReload()
        {
            if (_preventAfterReload)
            {
                Debug.LogError("prevented AfterReload call happening recursively, something is causing this");
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AfterReload;
                return;
            }

            if (EditorApplication.isPlaying) return;

            _preventAfterReload = true;

            // safety, because directory can be deleted and domain reload can happen
            InitDirs();

            var (_, _, testsDir) = GetRepoDirs();
            LinkAndAddTests(testsDir);

            var testObj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(o => o.name == TestObjName);

            if (testObj != null)
            {
                AddTests(testObj);
                SetupTestScene(testObj);
                SaveAssetBundles();

                if (!EditorSceneManager.SaveOpenScenes())
                {
                    throw new InvalidOperationException("failed to save opened scenes");
                }
            }

            ValidateSceneList();

            Debug.Log("Finished loading UniTAS testing framework");
            _preventAfterReload = false;
        }


        private const string ScriptsDir = "Assets/Scripts";
        private const string TestsDir = ScriptsDir + "/Tests";

        private static void GetRepoDirs(out string sharedScriptsDir, out string sharedEditorDir, out string testsDir)
        {
            var repoDir = Directory.GetCurrentDirectory();
            while (Path.GetFileName(repoDir) != "UniTAS")
            {
                var repoDirParent = Directory.GetParent(repoDir);
                if (repoDirParent != null)
                {
                    repoDir = repoDirParent.FullName;
                    if (repoDir != null) continue;
                }
                throw new Exception("Failed to find repository base directory, failed file setup");
            }

            var sharedDir = Path.Combine(repoDir, "TestShared");
            AssertDirExists(sharedDir);
            sharedScriptsDir = Path.Combine(sharedDir, "Scripts");
            AssertDirExists(sharedScriptsDir);
            sharedEditorDir = Path.Combine(sharedDir, "Editor");
            AssertDirExists(sharedEditorDir);
            testsDir = Path.Combine(sharedDir, "Tests");
            AssertDirExists(testsDir);
        }

        private static void AssertDirExists(string dir)
        {
            UnityEngine.Assertions.Assert.IsTrue(Directory.Exists(dir));
        }

        private static void InitDirs()
        {
            var createPaths = new[]
            {
                TestFrameworkRuntime.SceneAssetPath, TestFrameworkRuntime.PrefabAssetPath, TestsDir,
                TestFrameworkRuntime.ResourcesPath, TestFrameworkRuntime.AssetBundlePath, TestFrameworkRuntime.BuildPath
            };
            foreach (var path in createPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private static void LinkRunnerFiles(string sharedScriptsDir, string sharedEditorDir)
        {
            const string editorDir = "Assets/Editor";

            // TODO: figure out which unity version didn't work with symlinks

            // link everything
            var links = new Dictionary<string, string> { { sharedScriptsDir, ScriptsDir }, { sharedEditorDir, editorDir } };
            foreach (var sourceDir in links.Keys)
            {
                var destDir = links[sourceDir];
                foreach (var sourceFile in Directory.GetFiles(sourceDir, "*.cs", SearchOption.TopDirectoryOnly))
                {
                    var destFile = Path.Combine(destDir, Path.GetFileName(sourceFile));
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }

                    RelativeSymlinkFile(sourceFile, destFile);
                }
            }
        }

        private static void LinkAndAddTests(string testsDir)
        {
            foreach (var sourceFile in Directory.GetFiles(testsDir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(sourceFile);
                if (!MatchesVersion(fileNameNoExt))
                {
                    continue;
                }

                Debug.Log(string.Format("found matching test file `{0}`", fileNameNoExt));
                var destFile = Path.Combine(TestsDir, Path.GetFileName(sourceFile));
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }

                RelativeSymlinkFile(sourceFile, destFile);
            }
        }

        private static void AddTests(GameObject testObj)
        {
            if (!Directory.Exists(TestsDir))
            {
                Debug.LogWarning(string.Format("tests directory `{0}` doesn't exist", TestsDir));
                return;
            }

            foreach (var testPath in Directory.GetFiles(TestsDir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(testPath);
                var scriptType = script.GetClass();
                if (testObj.GetComponent(scriptType) != null) continue;
                testObj.AddComponent(scriptType);
                Debug.Log(string.Format("adding test component {0}", scriptType.FullName));
            }
        }

        private const string TinySep = "_";
        private const string BigSep = "__";

        private static bool MatchesVersion(string testName)
        {
            var exampleTestName = string.Format("`Category{0}2022{1}3{2}41{3}2023{4}3`", BigSep, TinySep, TinySep, BigSep, TinySep);
            var versionStartIdx = testName.IndexOf(BigSep, StringComparison.InvariantCulture);
            switch (versionStartIdx)
            {
                case -1:
                    throw new InvalidOperationException(
                        string.Format("test name `{0}` is formatted wrong, missing initial `{1}` before stating", testName, BigSep) +
                        " minimum unity version like so: " + exampleTestName);
                case 0:
                    Debug.LogWarning(string.Format("test name `{0}` has got no category prefixed in the name like so: ", testName) +
                                     exampleTestName);
                    break;
            }

            var fullVersionRaw = testName.Substring(versionStartIdx + BigSep.Length);
            var versionSepIdx = fullVersionRaw.IndexOf(BigSep, StringComparison.InvariantCulture);
            switch (versionSepIdx)
            {
                case -1:
                    throw new InvalidOperationException(
                        string.Format("test name `{0}` doesn't have a max version defined for the test, only the min version", testName) +
                        "you need to add the maximum inclusive version like so: " + exampleTestName);
                case 0:
                    throw new InvalidOperationException(
                        string.Format("test name `{0}` minimum version is non-existent, you need to define it like so: ", testName) +
                        exampleTestName);
            }

            var versionMinRaw = fullVersionRaw.Substring(0, versionSepIdx);
            var versionMaxRaw = fullVersionRaw.Substring(versionSepIdx + BigSep.Length);
            if (versionMaxRaw.Trim().Length == 0)
            {
                throw new InvalidOperationException(
                    string.Format("test name `{0}` maximum version is non-existent, you need to define it like so: ", testName) +
                    exampleTestName);
            }

            using (var versionMin = GetVersionFromRaw(versionMinRaw).GetEnumerator())
            {
                using (var versionMax = GetVersionFromRaw(versionMaxRaw).GetEnumerator())
                {
                    var currentVersion = Application.unityVersion.Split('.').Select(v => int.Parse(v.Replace('f', '0')))
                        .ToArray();
                    foreach (var currentVersionEntry in currentVersion)
                    {
                        if (!versionMin.MoveNext())
                        {
                            break;
                        }

                        if (currentVersionEntry > versionMin.Current) break;
                        if (currentVersionEntry < versionMin.Current) return false;
                    }

                    foreach (var currentVersionEntry in currentVersion)
                    {
                        if (!versionMax.MoveNext())
                        {
                            break;
                        }

                        if (currentVersionEntry < versionMax.Current) break;
                        if (currentVersionEntry > versionMax.Current) return false;
                    }

                    return true;
                }
            }
        }

        private static IEnumerable<int> GetVersionFromRaw(string rawVersion)
        {
            var split = rawVersion.Split(TinySep);
            return split.Select(v =>
            {
                int success;
                if (int.TryParse(v.Replace('f', '0'), out success))
                {
                    return success;
                }

                throw new InvalidOperationException(
                    string.Format("invalid version: `{0}`, make sure each version number is separated by `{1}`", rawVersion, TinySep));
            });
        }

        private static void RelativeSymlinkFile(string source, string target)
        {
            var targetWorking = Directory.GetParent(target);
            if (targetWorking == null)
            {
                throw new ArgumentException(string.Format("path `{0}` doesn't have a parent directory", target), nameof(target));
            }
            targetWorking = targetWorking.FullName;

            // find out how much we need to go back to reach source dir
            var sourceRel = string.Empty;
            while (!source.StartsWith(targetWorking))
            {
                targetWorking = Directory.GetParent(targetWorking);
                if (targetWorking == null)
                {
                    throw new InvalidOperationException("Directory.GetParent returned null, this should never happen" +
                                                        string.Format(", source: `{0}`, target: `{1}`", source, target));
                }
                targetWorking = targetWorking.FullName;

                sourceRel += string.Format("..{0}", Path.DirectorySeparatorChar);
            }

            // now push rest of the path
            source = sourceRel + source.Substring(targetWorking.Length + 1);
            var plat = Environment.OSVersion.Platform;
            bool success;
            switch (plat)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    success = CreateSymbolicLink(target, source, SymbolicLink.File);
                    break;
                case PlatformID.Unix:
                    success = symlink(source) == 0;
                    break;
                default:
                    throw new NotImplementedException(string.Format("symlink operation not implemented for platform {0}", plat));
            }

            if (!success)
            {
                throw new Exception(string.Format("symlink failed: error code {0}", Marshal.GetLastWin32Error()));
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName,
            SymbolicLink dwFlags);

        [DllImport("libc", SetLastError = true)]
        private static extern int symlink(string oldname, string newname);

        private enum SymbolicLink
        {
            File = 0
        }

        private const string TestObjName = "Tests";

        private static void InitTestScene()
        {
            var saveScene = false;
            var scene = AssetDatabase.AssetPathExists(TestFrameworkRuntime.TestingScenePath)
                ? EditorSceneManager.OpenScene(TestFrameworkRuntime.TestingScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            const string eventHooksObjName = "EventHooks";
            var testObj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(o => o.name == TestObjName);
            if (testObj == null)
            {
                testObj = new GameObject(TestObjName);
                saveScene = true;
            }

            if (testObj.GetComponent<TestFrameworkRuntime>() == null)
            {
                testObj.AddComponent<TestFrameworkRuntime>();
                saveScene = true;
            }

            var eventHooksObj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(o => o.name == eventHooksObjName);
            if (eventHooksObj == null)
            {
                eventHooksObj = new GameObject(eventHooksObjName);
                saveScene = true;
            }

            if (eventHooksObj.GetComponent<EventHooks>() == null)
            {
                eventHooksObj.AddComponent<EventHooks>();
                saveScene = true;
            }

            if (saveScene) EditorSceneManager.SaveScene(scene, TestFrameworkRuntime.TestingScenePath);

            var sceneSetting = EditorBuildSettings.scenes.FirstOrDefault(x => x.path == TestFrameworkRuntime.TestingScenePath);
            if (sceneSetting == null)
            {
                EditorBuildSettings.scenes = new[]
                { new EditorBuildSettingsScene(TestFrameworkRuntime.TestingScenePath, true) };
                return;
            }

            sceneSetting.enabled = true;
        }

        private static void SetupTestScene(GameObject tests)
        {
            foreach (var monoBeh in tests.GetComponents<MonoBehaviour>())
            {
                if (monoBeh == null) continue;
                var type = monoBeh.GetType();
                var testMethods = TestFrameworkRuntime.GetTestFuncs(type);
                var hasTests = false;
                foreach (var testMethod in testMethods)
                {
                    hasTests = true;
                    if (testMethod.ReturnType == typeof(void) ||
                        testMethod.ReturnType == typeof(IEnumerator<TestYield>))
                    {
                        continue;
                    }

                    throw new InvalidOperationException("Test return type must be void or IEnumerable<TestYield>");
                }

                if (!hasTests) continue;
                var prop = new SerializedObject(monoBeh);
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attrs = field.GetCustomAttribute<TestInjectAttribute>(true);
                    if (attrs == null) continue;
                    var fieldName = field.Name;
                    var fieldType = field.FieldType;

                    var fieldProp = prop.FindProperty(fieldName);
                    if (fieldProp == null)
                        throw new InvalidOperationException(string.Format("Field {0} not found", fieldName));
                    Debug.Log(string.Format("Injecting field {0}.{1}", type.FullName, fieldName));
                    InjectField(type, attr, fieldType, fieldProp);
                }

                prop.ApplyModifiedProperties();
            }
        }

        private static readonly List<AssetBundleBuild> _saveAssetBundles = new List();

        private static void SaveAssetBundles()
        {
            if (_saveAssetBundles.Count == 0) return;

            BuildPipeline.BuildAssetBundles(new BuildAssetBundlesParameters
            {
                bundleDefinitions = _saveAssetBundles.ToArray(),
                outputPath = TestFrameworkRuntime.AssetBundlePath
            });

            _saveAssetBundles.Clear();
        }

        private static void ValidateSceneList()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes.Length);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!AssetDatabase.AssetPathExists(scene.path)) continue;

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private const string AlreadyInjected = "Field already injected";

        private static void InjectField(Type monoBehType, TestInjectAttribute attr, Type fieldType,
            SerializedProperty field)
        {
            switch (attr)
            {
                case TestInjectSceneAttribute:
                    InjectFieldScene(fieldType, field);
                    break;
                case TestInjectPrefabAttribute:
                    var prefab = (TestInjectPrefabAttribute)attr;
                    InjectFieldPrefab(monoBehType, fieldType, field, prefab);
                    break;
                case TestInjectResource:
                    var resource = (TestInjectResource)attr;
                    InjectFieldResource(monoBehType, fieldType, field, resource);
                    break;
                case TestInjectAssetBundle:
                    var assetBundle = (TestInjectAssetBundle)attr;
                    InjectAssetBundle(monoBehType, fieldType, field, assetBundle);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Injection type `{0}` is not handled", attr));
            }
        }

        internal static void CopyEditorAssetBundleToBuildPath()
        {
            var paths = Directory.GetFileSystemEntries(TestFrameworkRuntime.AssetPath, "*.bundle",
                SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                var dest = Path.Combine(TestFrameworkRuntime.BuildPath, path);
                var destDir = Path.GetDirectoryName(dest);
                if (destDir == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Failed to get asset bundle parent directory, which should be impossible. Path is `{0}`", path));
                }

                Directory.CreateDirectory(destDir);
                File.Copy(path, dest, true);
            }
        }

        private static void InjectAssetBundle(Type monoBehType, Type fieldType, SerializedProperty field,
            TestInjectAssetBundle assetBundle)
        {
            var inner = field.FindPropertyRelative(OnceOnlyPath.InnerFieldName);
            if (!string.IsNullOrEmpty(inner.stringValue) && File.Exists(inner.stringValue))
            {
                Debug.Log(AlreadyInjected);
                return;
            }

            if (fieldType != typeof(OnceOnlyPath))
            {
                throw new InvalidOperationException(string.Format("Field type is not `{0}`", nameof(OnceOnlyPath)));
            }

            var property = monoBehType.GetProperty(assetBundle.AssetProperty);
            if (property == null)
            {
                throw new InvalidOperationException("`AssetProperty` isn't pointing to a valid property");
            }

            var assetRaw = property.GetValue(null);
            if (assetRaw == null)
            {
                throw new InvalidOperationException("Asset is null");
            }

            if (assetRaw.GetType() != typeof(Dictionary<string, ITestAsset>))
            {
                throw new InvalidOperationException(
                    string.Format("Asset property was expected to be {0}", nameof(Dictionary<string, ITestAsset>)));
            }

            var assets = (Dictionary<string, ITestAsset>)assetRaw;
            if (assets.Count == 0)
            {
                throw new InvalidOperationException("Asset contains no objects");
            }
            var paths = new string[assets.Count];

            var assetsPath = Path.Combine(TestFrameworkRuntime.AssetBundlePath, "assets");
            assetsPath = AssetDatabase.GenerateUniqueAssetPath(assetsPath);
            Directory.CreateDirectory(assetsPath);
            Debug.Log(string.Format("new assets directory `{0}`", assetsPath));

            var i = 0;
            foreach (var assetPath in assets.Keys)
            {
                var asset = assets[assetPath];
                InitAsset(asset, assetsPath, path =>
                {
                    paths[i] = path;
                    i++;

                    if (i < assets.Count)
                        return;

                    // last entry is done
                    var assetBundlePath =
                        AssetDatabase.GenerateUniqueAssetPath(Path.Combine(TestFrameworkRuntime.AssetBundlePath,
                            "bundle.bundle"));
                    var assetBundleName = Path.GetFileName(assetBundlePath);

                    File.Create(assetBundlePath);

                    inner.stringValue = assetBundlePath;
                    inner.serializedObject.ApplyModifiedProperties();

                    // required as this is delayed
                    if (!EditorSceneManager.SaveOpenScenes())
                    {
                        throw new InvalidOperationException("failed to save open scenes");
                    }

                    _saveAssetBundles.Add(new AssetBundleBuild
                    {
                        assetBundleName = assetBundleName,
                        assetNames = paths
                    });
                }, assetPath);
            }
        }

        private static void InjectFieldResource(Type monoBehType, Type fieldType, SerializedProperty field,
            TestInjectResource resource)
        {
            var inner = field.FindPropertyRelative(OnceOnlyPath.InnerFieldName);
            var filename = inner.stringValue;
            var resources = Directory.GetFiles(TestFrameworkRuntime.ResourcesPath);
            if (!string.IsNullOrEmpty(filename) && resources.Any(v => Path.GetFileNameWithoutExtension(v) == filename))
            {
                Debug.Log(AlreadyInjected);
                return;
            }

            if (fieldType != typeof(OnceOnlyPath))
            {
                throw new InvalidOperationException(string.Format("Field type is not `{0}`", nameof(OnceOnlyPath)));
            }

            InitAssetByProperty(monoBehType, resource.AssetProperty, TestFrameworkRuntime.ResourcesPath,
                path =>
                {
                    const string key = "/Resources/";
                    var idx = path.IndexOf(key, StringComparison.InvariantCulture);
                    if (idx < 0)
                    {
                        Debug.LogWarning(string.Format("Somehow, the path `{0}` isn't in the resources directory", path));
                        return;
                    }

                    path = path.Substring(idx + key.Length);
                    var pathDir = Path.GetDirectoryName(path);
                    var filenameWithoutExt = Path.GetFileNameWithoutExtension(path);
                    path = pathDir == null ? filenameWithoutExt : Path.Combine(pathDir, filenameWithoutExt);

                    inner.stringValue = path;
                    inner.serializedObject.ApplyModifiedProperties();
                });
        }

        private static void InjectFieldScene(Type fieldType, SerializedProperty field)
        {
            if (fieldType != typeof(string))
            {
                throw new InvalidOperationException("Field type is not string");
            }

            if (!string.IsNullOrEmpty(field.stringValue) && AssetDatabase.AssetPathExists(field.stringValue))
            {
                Debug.Log(AlreadyInjected);
                return;
            }

            InitAsset(new SceneAsset(), TestFrameworkRuntime.SceneAssetPath, path =>
            {
                field.stringValue = path;
                field.serializedObject.ApplyModifiedProperties();

                if (EditorBuildSettings.scenes.Any(x => x.path == path)) return;
                var scenes = EditorBuildSettings.scenes.ToList();
                scenes.Add(new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            });
        }

        private static void InjectFieldPrefab(Type monoBehType, Type fieldType, SerializedProperty field, TestInjectPrefabAttribute prefab)
        {
            if (fieldType != typeof(GameObject))
            {
                throw new InvalidOperationException("Field type is not GameObject");
            }

            if (field.objectReferenceValue != null)
            {
                Debug.Log(AlreadyInjected);
                return;
            }

            InitAssetByProperty(monoBehType, prefab.AssetProperty, TestFrameworkRuntime.PrefabAssetPath, path =>
            {
                field.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                field.serializedObject.ApplyModifiedProperties();
            });
        }

        private static void InitAssetByProperty(Type monoBehType, string propertyName, string pathPrefix, Action<string> assetReady, string fileName = null)
        {
            var property = monoBehType.GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException("`AssetProperty` isn't pointing to a valid property");
            }

            var assetRaw = property.GetValue(null);
            if (assetRaw == null)
            {
                throw new InvalidOperationException("Asset is null");
            }

            if (assetRaw.GetType().GetInterfaces().All(t => t != typeof(ITestAsset)))
            {
                throw new InvalidOperationException(string.Format("Asset property `{0}.{1}` was expected to be {2}", monoBehType.Name, property.Name, nameof(ITestAsset)));
            }

            InitAsset((ITestAsset)assetRaw, pathPrefix, assetReady, fileName);
        }

        private static void InitAsset(ITestAsset testAsset, string pathPrefix, Action<string> assetReady,
            string fileName = null)
        {
            switch (testAsset)
            {
                case GameObjectAsset:
                    {
                        var path = InitAssetPath(fileName ?? "asset.prefab", pathPrefix);

                        var prefab = new GameObject();
                        bool success;
                        PrefabUtility.SaveAsPrefabAsset(prefab, path, out success);
                        Object.DestroyImmediate(prefab);

                        if (!success)
                            throw new InvalidOperationException("Failed to save prefab");

                        assetReady(path);
                        break;
                    }

                case SceneAsset:
                    {
                        var path = InitAssetPath(fileName ?? "generated.unity", pathPrefix);

                        Debug.Log(string.Format("Creating scene at `{0}`", path));
                        if (!EditorSceneManager.SaveOpenScenes())
                        {
                            throw new InvalidOperationException("failed to save open scenes before creating scene");
                        }
                        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                        if (!EditorSceneManager.SaveScene(scene, path))
                        {
                            throw new InvalidOperationException(string.Format("Failed to save scene {0}", path));
                        }
                        EditorSceneManager.CloseScene(scene, true);

                        assetReady(path);
                        break;
                    }

                default:
                    throw new InvalidOperationException(string.Format("Asset type `{0}` is not handled", testAsset));
            }
        }

        private static string InitAssetPath(string fileName, string pathPrefix)
        {
            var path = fileName;
            if (pathPrefix != null)
            {
                if (!Directory.Exists(pathPrefix))
                {
                    Directory.CreateDirectory(pathPrefix);
                }

                path = Path.Combine(pathPrefix, path);
            }

            return AssetDatabase.GenerateUniqueAssetPath(path);
        }

        [MenuItem("Test/Run Tests ^t")]
        private static void RunTests()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("click play first");
                return;
            }

            TestFrameworkRuntime.RunTestsEditor();
        }

        [MenuItem("Test/Build ^b")]
        private static void Build()
        {
            var activeProfile = EditorUserBuildSettings.activeBuildTarget;
            BuildScript.Build(activeProfile);
        }

        [MenuItem("Test/Run test ^#t")]
        private static void RunTest()
        {
            ScriptableObject.CreateInstance<RunTestDialog>().ShowUtility();
        }
    }

    public class RunTestDialog : EditorWindow
    {
        private string testToRun;

        private void OnGUI()
        {
            testToRun = EditorGUILayout.TextField(testToRun);

            if (GUILayout.Button("Run"))
                TestFrameworkRuntime.Instance.StartCoroutine(TestFrameworkRuntime.RunTestByName(testToRun));

            if (GUILayout.Button("Cancel"))
                Close();
        }
    }

    public static class BuildScript
    {
        private static readonly string Eol = Environment.NewLine;

        private static readonly string[] Secrets =
            { "androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass" };

        public static void Build()
        {
            // Gather values from args
            var options = GetValidatedOptions();

            // Set version for this build
            string? buildVersion;
            if (options.TryGetValue("buildVersion", out buildVersion) && buildVersion != "none")
            {
                PlayerSettings.bundleVersion = buildVersion;
                PlayerSettings.macOS.buildNumber = buildVersion;
            }

            string? versionCode;
            if (options.TryGetValue("androidVersionCode", out versionCode) && versionCode != "0")
            {
                PlayerSettings.Android.bundleVersionCode = int.Parse(options["androidVersionCode"]);
            }

            // Apply build target
            var buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                    // PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    break;
            }

            // Custom build
            var result = Build(buildTarget);
            ExitWithResult(result.result);
        }

        private static Dictionary<string, string> GetValidatedOptions()
        {
            Dictionary<string, string>? validatedOptions;
            ParseCommandLineArguments(out validatedOptions);

            string? buildTarget;
            if (validatedOptions.TryGetValue("buildTarget", out buildTarget) && !Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                Console.WriteLine(string.Format("{0} is not a defined {1}", buildTarget, nameof(BuildTarget)));
                EditorApplication.Exit(121);
            }

            return validatedOptions;
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            var args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                Eol +
                "###########################" + Eol +
                "#    Parsing settings     #" + Eol +
                "###########################" + Eol +
                Eol
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                var isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                var flag = args[current].TrimStart('-');

                // Parse optional value
                var flagHasValue = next < args.Length && !args[next].StartsWith("-");
                var value = flagHasValue ? args[next].TrimStart('-') : "";
                var secret = Secrets.Contains(flag);
                var displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                // Assign
                Console.WriteLine(string.Format("Found flag \"{0}\" with value {1}.", flag, displayValue));
                providedArguments.Add(flag, value);
            }
        }

        internal static BuildSummary Build(BuildTarget buildTarget)
        {
            TestFrameworkSetup.CopyEditorAssetBundleToBuildPath();

            string filePath;
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    filePath = "exe";
                    break;
                case BuildTarget.StandaloneLinux64:
                    filePath = "x86_64";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null);
            }

            filePath = Path.Combine(TestFrameworkRuntime.BuildPath, string.Format("build.{0}", filePath));

            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                target = buildTarget,
                //                targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget),
                locationPathName = filePath,
                //                options = UnityEditor.BuildOptions.Development
            };

            var buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            ReportSummary(buildSummary);
            return buildSummary;
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Console.WriteLine(
                Eol +
                "###########################" + Eol +
                "#      Build results      #" + Eol +
                "###########################" + Eol +
                Eol +
                "Duration: " + summary.totalTime.ToString() + Eol +
                "Warnings: " + summary.totalWarnings.ToString() + Eol +
                "Errors: " + summary.totalErrors.ToString() + Eol +
                "Size: " + summary.totalSize.ToString() + " bytes" + Eol +
                Eol
            );
        }

        private static void ExitWithResult(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    Console.WriteLine("Build succeeded!");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Console.WriteLine("Build failed!");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Console.WriteLine("Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }
    }
}
