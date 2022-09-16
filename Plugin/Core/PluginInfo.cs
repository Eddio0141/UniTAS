using System;
using System.Collections.Generic;
using System.Linq;

namespace Core;

public static class PluginInfo
{
    public const string GUID = "UniTASPlugin";
    public const string NAME = "UniTAS";
    public const string VERSION = "0.1.0";

    public static UnityVersion UnityVersion { get; private set; }

    public static void Init(string unityVersion)
    {
        var unityVersionEnum = UnityVersionFromString(unityVersion);
        UnityVersion = unityVersionEnum.Item1;

        switch (unityVersionEnum.Item2)
        {
            case UnitySupportStatus.UsingFirstVersion:
                Log.LogWarning($"Unity version is lower than the lowest supported, falling back to {UnityVersion}");
                break;
            case UnitySupportStatus.FoundMatch:
                Log.LogInfo($"Unity version {UnityVersion} is supported");
                break;
            case UnitySupportStatus.UsingLowerVersion:
                Log.LogWarning($"No matching unity version found, falling back to {UnityVersion}");
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Get UnityVersion enum from string.
    /// If version isn't directly supported, it will try to get the lower unity version instead.
    /// If version lower than input isn't found, will try use the first supported version as a last effort.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static (UnityVersion, UnitySupportStatus) UnityVersionFromString(string version)
    {
        Log.LogDebug($"version text: {version}");

        var (major, minor, patch) = Helper.SemanticVersioningFromString(version);

        if (major < 1)
        {
            throw new Exception("Unity version too low");
        }

        var allUnityVersions = Enum.GetValues(typeof(UnityVersion)).Cast<UnityVersion>();
        var allUnityVersionsSemantic = new List<(int, int, int)>();

        foreach (var unityVersion in allUnityVersions)
        {
            allUnityVersionsSemantic.Add(Helper.SemanticVersioningFromString(unityVersion.ToString()));
        }

        // versions are already sorted, search through them all

        // TODO faster search with binary search on all rather than linear
        //      search result needs to point to lowest index of `allUnityVersionsSemantic`, and resulting unity version needs to be equal or less than input version
        var findingMajor = true;
        var findingMinor = true;

        var (foundMajor, foundMinor, foundPatch) = (allUnityVersionsSemantic[0].Item1, allUnityVersionsSemantic[0].Item2, allUnityVersionsSemantic[0].Item3);
        foreach (var (unityMajor, unityMinor, unityPatch) in allUnityVersionsSemantic)
        {
            if (findingMajor)
            {
                if (unityMajor < major)
                {
                    foundMajor = unityMajor;
                    foundMinor = unityMinor;
                    foundPatch = unityPatch;
                    continue;
                }
                if (unityMajor == major)
                {
                    foundMajor = unityMajor;
                    foundMinor = unityMinor;
                    foundPatch = unityPatch;
                }
                findingMajor = false;
            }

            if (unityMajor > foundMajor)
                break;

            if (findingMinor)
            {
                if (unityMinor < minor)
                {
                    foundMinor = unityMinor;
                    foundPatch = unityPatch;
                    continue;
                }
                if (unityMinor == minor)
                {
                    foundMinor = unityMinor;
                    foundPatch = unityPatch;
                }
                findingMinor = false;
            }

            if (unityMinor > foundMinor)
                break;

            if (unityPatch < patch)
            {
                foundPatch = unityPatch;
                continue;
            }
            else if (unityPatch == patch)
            {
                foundPatch = unityPatch;
                break;
            }
        }

        var foundStatus = UnitySupportStatus.UsingLowerVersion;
        if (foundMajor == major && foundMinor == minor && foundPatch == patch)
            foundStatus = UnitySupportStatus.FoundMatch;
        else if (major < foundMajor || (major <= foundMajor && minor < foundMinor) || (major <= foundMajor && minor <= foundMinor && patch < foundPatch))
            foundStatus = UnitySupportStatus.UsingFirstVersion;

        Enum.TryParse(typeof(UnityVersion), $"v{foundMajor}_{foundMinor}_{foundPatch}", out var result);

        return ((UnityVersion)result, foundStatus);
    }

    public enum UnitySupportStatus
    {
        FoundMatch,
        UsingLowerVersion,
        UsingFirstVersion,
    }
}

/// <summary>
/// Supported Unity versions.
/// Follows semantic versioning.
/// </summary>
public enum UnityVersion
{
    v2021_2_14,
}