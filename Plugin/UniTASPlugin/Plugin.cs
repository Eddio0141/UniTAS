using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTASPlugin;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "UniTASPlugin";
    public const string NAME = "UniTAS";
    public const string VERSION = "0.1.0";

    internal static BepInEx.Logging.ManualLogSource Log;

    // TODO do i use static constructor here?
    private void Awake()
    {
        Log = Logger;

        //var unityVersion = Helper.UnityVersion();
        //Log.LogInfo($"Internally found version: {unityVersion}");
        // TODO make this version compatible, v3.5.1 doesn't have those
        //Logger.Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        // TODO fix this
        var unityVersionEnum = UnityVersionFromString("3.0.0");
        var UnityVersion = unityVersionEnum.UnityVersion;

        switch (unityVersionEnum.UnitySupportStatus)
        {
            case UnitySupportStatus.UsingFirstVersion:
                Log.LogWarning($"Unity version is lower than the lowest supported, falling back compatibility to {UnityVersion}");
                break;
            case UnitySupportStatus.FoundMatch:
                Log.LogInfo($"Unity version {UnityVersion} is supported in tool");
                break;
            case UnitySupportStatus.UsingLowerVersion:
                Log.LogWarning($"No matching unity version found, falling back compatibility to {UnityVersion}");
                break;
            default:
                throw new InvalidOperationException();
        }

        var harmony = new Harmony($"{Core.PluginInfo.NAME}HarmonyPatch");
        harmony.PatchAll();

        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        Log.LogInfo($"Plugin {NAME} is loaded!");
    }

    /// <summary>
    /// Get UnityVersion enum from string.
    /// If version isn't directly supported, it will try to get the lower unity version instead.
    /// If version lower than input isn't found, will try use the first supported version as a last effort.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    // TODO move this somewhere else
    public static UnityVersionFromStringResult UnityVersionFromString(string version)
    {
        var versionParsed = new Helper.SemanticVersion(version);

        var allUnityVersions = Enum.GetValues(typeof(UnityVersion)).Cast<UnityVersion>();
        var allUnityVersionsSemantic = new List<Helper.SemanticVersion>();

        foreach (var unityVersion in allUnityVersions)
        {
            allUnityVersionsSemantic.Add(new Helper.SemanticVersion(unityVersion.ToString()));
        }

        // versions are already sorted, search through them all

        // TODO faster search with binary search on all rather than linear
        //      search result needs to point to lowest index of `allUnityVersionsSemantic`, and resulting unity version needs to be equal or less than input version
        var findingMajor = true;
        var findingMinor = true;

        var foundVersion = allUnityVersionsSemantic[0];
        foreach (var unityVersion in allUnityVersionsSemantic)
        {
            var unityMajor = unityVersion.Major;
            var unityMinor = unityVersion.Minor;
            var unityPatch = unityVersion.Patch;

            if (findingMajor)
            {
                if (unityMajor < versionParsed.Major)
                {
                    foundVersion.Major = unityMajor;
                    foundVersion.Minor = unityMinor;
                    foundVersion.Patch = unityPatch;
                    continue;
                }
                if (unityMajor == versionParsed.Major)
                {
                    foundVersion.Major = unityMajor;
                    foundVersion.Minor = unityMinor;
                    foundVersion.Patch = unityPatch;
                }
                findingMajor = false;
            }

            if (unityMajor > foundVersion.Major)
                break;

            if (findingMinor)
            {
                if (unityMinor < versionParsed.Minor)
                {
                    foundVersion.Minor = unityMinor;
                    foundVersion.Patch = unityPatch;
                    continue;
                }
                if (unityMinor == versionParsed.Minor)
                {
                    foundVersion.Minor = unityMinor;
                    foundVersion.Patch = unityPatch;
                }
                findingMinor = false;
            }

            if (unityMinor > foundVersion.Minor)
                break;

            if (unityPatch < versionParsed.Patch)
            {
                foundVersion.Patch = unityPatch;
                continue;
            }
            else if (unityPatch == versionParsed.Patch)
            {
                foundVersion.Patch = unityPatch;
                break;
            }
        }

        var foundStatus = UnitySupportStatus.UsingLowerVersion;
        if (foundVersion == versionParsed)
            foundStatus = UnitySupportStatus.FoundMatch;
        else if (versionParsed.Major < foundVersion.Major || (versionParsed.Major <= foundVersion.Major && versionParsed.Minor < foundVersion.Minor) || (versionParsed.Major <= foundVersion.Major && versionParsed.Minor <= foundVersion.Minor && versionParsed.Patch < foundVersion.Patch))
            foundStatus = UnitySupportStatus.UsingFirstVersion;

        var result = Enum.Parse(typeof(UnityVersion), $"v{foundVersion.Major}_{foundVersion.Minor}_{foundVersion.Patch}");

        return new UnityVersionFromStringResult((UnityVersion)result, foundStatus);
    }

    public enum UnitySupportStatus
    {
        FoundMatch,
        UsingLowerVersion,
        UsingFirstVersion,
    }

    public class UnityVersionFromStringResult
    {
        public UnityVersion UnityVersion;
        public UnitySupportStatus UnitySupportStatus;

        public UnityVersionFromStringResult(UnityVersion unityVersion, UnitySupportStatus unitySupportStatus)
        {
            UnityVersion = unityVersion;
            UnitySupportStatus = unitySupportStatus;
        }
    }

    private void Update()
    {
        GameCapture.Update();
        Core.TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            var text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            var movie = new Movie("test.uti", text, out var err);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }

            Core.TAS.Main.RunMovie(movie);
        }
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            Core.SaveState.Main.Save();
        }
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.O))
        {
            Core.SaveState.Main.Load();
        }
    }

    private void FixedUpdate()
    {
        Core.TAS.Main.FixedUpdate();
    }
}

/// <summary>
/// Supported Unity versions.
/// Follows semantic versioning.
/// </summary>
// The order of the enum matters
public enum UnityVersion
{
    v2018_4_25,
    v2021_2_14,
}