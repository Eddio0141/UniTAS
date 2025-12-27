using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.UniTASTest
{
    public static class TestFrameworkSetup
    {
        [MenuItem("Test/Setup")]
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

            var testObj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(o => o.name == TestObjName);

            if (testObj != null)
            {
                AddTests(testObj);
                SetupTestScene(testObj);

                if (!EditorSceneManager.SaveOpenScenes())
                {
                    throw new InvalidOperationException("failed to save opened scenes");
                }
            }

            Debug.Log("Finished loading UniTAS testing framework");
            _preventAfterReload = false;
        }


        private const string ScriptsDir = "Assets/Scripts";
        private const string TestsDir = ScriptsDir + "/Tests";

        private static (string sharedScriptsDir, string sharedEditorDir, string testsDir) GetRepoDirs()
        {
            var repoDir = Directory.GetCurrentDirectory();
            while (Path.GetFileName(repoDir) != "UniTAS")
            {
                repoDir = Directory.GetParent(repoDir)?.FullName;
                if (repoDir != null) continue;
                throw new Exception("Failed to find repository base directory, failed file setup");
            }

            var sharedDir = Path.Combine(repoDir, "TestShared");
            AssertDirExists(sharedDir);
            var sharedScriptsDir = Path.Combine(sharedDir, "Scripts");
            AssertDirExists(sharedScriptsDir);
            var sharedEditorDir = Path.Combine(sharedDir, "Editor");
            AssertDirExists(sharedEditorDir);
            var testsDir = Path.Combine(sharedDir, "Tests");
            AssertDirExists(testsDir);
            return (sharedScriptsDir, sharedEditorDir, testsDir);
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
            var links = new[] { (sharedScriptsDir, ScriptsDir), (sharedEditorDir, editorDir) };
            foreach (var (sourceDir, destDir) in links)
            {
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

                Debug.Log($"found matching test file `{fileNameNoExt}`");
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
                Debug.LogWarning($"tests directory `{TestsDir}` doesn't exist");
                return;
            }

            foreach (var testPath in Directory.GetFiles(TestsDir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(testPath);
                var scriptType = script.GetClass();
                if (testObj.GetComponent(scriptType) != null) continue;
                testObj.AddComponent(scriptType);
            }
        }

        private const string TinySep = "_";
        private const string BigSep = "__";

        private static bool MatchesVersion(string testName)
        {
            var exampleTestName = $"`Category{BigSep}2022{TinySep}3{TinySep}41{BigSep}2023{TinySep}3`";
            var versionStartIdx = testName.IndexOf(BigSep, StringComparison.InvariantCulture);
            switch (versionStartIdx)
            {
                case -1:
                    throw new InvalidOperationException(
                        $"test name `{testName}` is formatted wrong, missing initial `{BigSep}` before stating" +
                        " minimum unity version like so: " + exampleTestName);
                case 0:
                    Debug.LogWarning($"test name `{testName}` has got no category prefixed in the name like so: " +
                                     exampleTestName);
                    break;
            }

            var fullVersionRaw = testName[(versionStartIdx + BigSep.Length)..];
            var versionSepIdx = fullVersionRaw.IndexOf(BigSep, StringComparison.InvariantCulture);
            switch (versionSepIdx)
            {
                case -1:
                    throw new InvalidOperationException(
                        $"test name `{testName}` doesn't have a max version defined for the test, only the min version" +
                        "you need to add the maximum inclusive version like so: " + exampleTestName);
                case 0:
                    throw new InvalidOperationException(
                        $"test name `{testName}` minimum version is non-existent, you need to define it like so: " +
                        exampleTestName);
            }

            var versionMinRaw = fullVersionRaw[..versionSepIdx];
            var versionMaxRaw = fullVersionRaw[(versionSepIdx + BigSep.Length)..];
            if (versionMaxRaw.Trim().Length == 0)
            {
                throw new InvalidOperationException(
                    $"test name `{testName}` maximum version is non-existent, you need to define it like so: " +
                    exampleTestName);
            }

            using var versionMin = GetVersionFromRaw(versionMinRaw).GetEnumerator();
            using var versionMax = GetVersionFromRaw(versionMaxRaw).GetEnumerator();
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

        private static IEnumerable<int> GetVersionFromRaw(string rawVersion)
        {
            var split = rawVersion.Split(TinySep);
            return split.Select(v =>
            {
                if (int.TryParse(v.Replace('f', '0'), out var success))
                {
                    return success;
                }

                throw new InvalidOperationException(
                    $"invalid version: `{rawVersion}`, make sure each version number is separated by `{TinySep}`");
            });
        }

        private static void RelativeSymlinkFile(string source, string target)
        {
            var targetWorking = Directory.GetParent(target)?.FullName;
            if (targetWorking == null)
            {
                throw new ArgumentException($"path `{target}` doesn't have a parent directory", nameof(target));
            }

            // find out how much we need to go back to reach source dir
            var sourceRel = string.Empty;
            while (!source.StartsWith(targetWorking))
            {
                targetWorking = Directory.GetParent(targetWorking)?.FullName;
                if (targetWorking == null)
                {
                    throw new InvalidOperationException("Directory.GetParent returned null, this should never happen" +
                                                        $", source: `{source}`, target: `{target}`");
                }

                sourceRel += $"..{Path.DirectorySeparatorChar}";
            }

            // now push rest of the path
            source = sourceRel + source[(targetWorking.Length + 1)..];
            var plat = Environment.OSVersion.Platform;
            var success = plat switch
            {
                PlatformID.Unix => symlink(source, target) == 0,
                PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE =>
                    CreateSymbolicLink(target, source, SymbolicLink.File),
                _ => throw new NotImplementedException($"symlink operation not implemented for platform {plat}")
            };
            if (!success)
            {
                throw new Exception($"symlink failed: error code {Marshal.GetLastWin32Error()}");
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
                        continue;
                    throw new InvalidOperationException("Test return type must be void or IEnumerable<TestYield>");
                }

                if (!hasTests) continue;
                var injectFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(f => (f.Name, f.GetCustomAttribute<TestInjectAttribute>(true), f.FieldType))
                    .Where(tuple => tuple.Item2 != null);
                var prop = new SerializedObject(monoBeh);
                foreach (var (fieldName, attr, fieldType) in injectFields)
                {
                    var field = prop.FindProperty(fieldName);
                    if (field == null)
                    {
                        throw new InvalidOperationException($"Field {fieldName} not found");
                    }

                    Debug.Log($"Injecting field {type.FullName}.{fieldName}");
                    InjectField(type, attr, fieldType, field);
                }

                prop.ApplyModifiedProperties();
            }
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
                    InjectFieldPrefab(fieldType, field);
                    break;
                case TestInjectResource resource:
                    InjectFieldResource(monoBehType, fieldType, field, resource);
                    break;
                case TestInjectAssetBundle assetBundle:
                    InjectAssetBundle(monoBehType, fieldType, field, assetBundle);
                    break;
                default:
                    throw new InvalidOperationException($"Injection type `{attr}` is not handled");
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
                        $"Failed to get asset bundle parent directory, which should be impossible. Path is `{path}`");
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
                throw new InvalidOperationException($"Field type is not `{nameof(OnceOnlyPath)}`");
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
                    $"Asset property was expected to be {nameof(Dictionary<string, ITestAsset>)}");
            }

            var assets = (Dictionary<string, ITestAsset>)assetRaw;
            var paths = new string[assets.Count];
            var assetsIter = assets.GetEnumerator();

            var assetsPath = Path.Combine(TestFrameworkRuntime.AssetBundlePath, "assets");
            assetsPath = AssetDatabase.GenerateUniqueAssetPath(assetsPath);
            Directory.CreateDirectory(assetsPath);

            for (var i = 0; i < assets.Count; i++)
            {
                assetsIter.MoveNext();
                var (assetPath, asset) = assetsIter.Current;

                var j = i;
                var assetReady = new Action<string>(path =>
                {
                    paths[j] = path;

                    if (j + 1 < assets.Count)
                        return;

                    // last entry is done
                    var assetBundlePath =
                        AssetDatabase.GenerateUniqueAssetPath(Path.Combine(TestFrameworkRuntime.AssetBundlePath,
                            "bundle.bundle"));
                    var assetBundleName = Path.GetFileName(assetBundlePath);

                    BuildPipeline.BuildAssetBundles(new BuildAssetBundlesParameters
                    {
                        bundleDefinitions = new[]
                        {
                            new AssetBundleBuild
                            {
                                assetBundleName = assetBundleName,
                                assetNames = paths
                            }
                        },
                        outputPath = TestFrameworkRuntime.AssetBundlePath
                    });

                    inner.stringValue = assetBundlePath;
                    inner.serializedObject.ApplyModifiedProperties();
                });

                InitAsset(asset, assetsPath, assetReady, assetPath);
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
                throw new InvalidOperationException($"Field type is not `{nameof(OnceOnlyPath)}`");
            }

            var property = monoBehType.GetProperty(resource.AssetProperty);
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
                throw new InvalidOperationException($"Asset property was expected to be {nameof(ITestAsset)}");
            }

            InitAsset((ITestAsset)assetRaw, TestFrameworkRuntime.ResourcesPath,
                path =>
                {
                    const string key = "/Resources/";
                    var idx = path.IndexOf(key, StringComparison.InvariantCulture);
                    if (idx < 0)
                    {
                        Debug.LogWarning($"Somehow, the path `{path}` isn't in the resources directory");
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

            var scenePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(TestFrameworkRuntime.SceneAssetPath,
                "generated.unity"));

            Debug.Log($"Creating scene at `{scenePath}`");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save scene {scenePath}");
            }

            EditorSceneManager.CloseScene(scene, true);

            field.stringValue = scenePath;
            HelperEditor.DelaySaveOpenScenes();

            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void InjectFieldPrefab(Type fieldType, SerializedProperty field)
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

            var prefabBase = new GameObject();

            var prefabPath =
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(TestFrameworkRuntime.PrefabAssetPath,
                    "generated.prefab"));
            Debug.Log($"Creating prefab at `{prefabPath}`");
            PrefabUtility.SaveAsPrefabAsset(prefabBase, prefabPath, out var success);
            Object.DestroyImmediate(prefabBase);

            EditorApplication.delayCall += () =>
            {
                field.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                field.serializedObject.ApplyModifiedProperties();
                HelperEditor.DelaySaveOpenScenes();
            };

            if (!success)
                throw new InvalidOperationException("Failed to save prefab");
        }

        private static void InitAsset(ITestAsset testAsset, string pathPrefix, Action<string> assetReady,
            string fileName = null)
        {
            switch (testAsset)
            {
                case GameObjectAsset:
                    {
                        var prefab = new GameObject();

                        var path = fileName ?? "asset.prefab";
                        if (pathPrefix != null)
                        {
                            if (!Directory.Exists(pathPrefix))
                            {
                                Directory.CreateDirectory(pathPrefix);
                            }

                            path = Path.Combine(pathPrefix, path);
                        }

                        if (fileName == null)
                        {
                            path = AssetDatabase.GenerateUniqueAssetPath(path);
                        }

                        PrefabUtility.SaveAsPrefabAsset(prefab, path, out var success);
                        Object.DestroyImmediate(prefab);

                        EditorApplication.delayCall += () =>
                        {
                            assetReady(path);
                            HelperEditor.DelaySaveOpenScenes();
                        };

                        if (!success)
                            throw new InvalidOperationException("Failed to save prefab");

                        break;
                    }

                default:
                    throw new InvalidOperationException($"Asset type `{testAsset}` is not handled");
            }
        }

        [MenuItem("Test/Run General Tests")]
        private static void RunGeneralTests()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("click play first");
                return;
            }

            TestFrameworkRuntime.RunGeneralTests();
        }

        [MenuItem("Test/Build")]
        private static void Build()
        {
            var activeProfile = EditorUserBuildSettings.activeBuildTarget;
            BuildScript.Build(activeProfile);
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
            if (options.TryGetValue("buildVersion", out var buildVersion) && buildVersion != "none")
            {
                PlayerSettings.bundleVersion = buildVersion;
                PlayerSettings.macOS.buildNumber = buildVersion;
            }

            if (options.TryGetValue("androidVersionCode", out var versionCode) && versionCode != "0")
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
            ParseCommandLineArguments(out var validatedOptions);

            if (validatedOptions.TryGetValue("buildTarget", out var buildTarget))
            {
                if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
                {
                    Console.WriteLine($"{buildTarget} is not a defined {nameof(BuildTarget)}");
                    EditorApplication.Exit(121);
                }
            }

            return validatedOptions;
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            var args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
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
                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
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

            filePath = Path.Combine(TestFrameworkRuntime.BuildPath, $"build.{filePath}");

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
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Build results      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Duration: {summary.totalTime.ToString()}{Eol}" +
                $"Warnings: {summary.totalWarnings.ToString()}{Eol}" +
                $"Errors: {summary.totalErrors.ToString()}{Eol}" +
                $"Size: {summary.totalSize.ToString()} bytes{Eol}" +
                $"{Eol}"
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
                case BuildResult.Unknown:
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }
    }
}

public static class HelperEditor
{
    public static void DelaySaveOpenScenes()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorSceneManager.SaveOpenScenes())
            {
                Debug.LogError("failed to save open scenes");
            }
        };
    }
}
