using System.Linq;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;

namespace UniTASPlugin;

internal static partial class Helper
{
    public static SemanticVersion GetUnityVersion()
    {
        System.Diagnostics.FileVersionInfo fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(@".\UnityPlayer.dll");
        string versionRaw = fileVersion.FileVersion;
        return new SemanticVersion(versionRaw);
    }

    public static bool ValueHasDecimalPoints(float value)
    {
        return value.ToString().Contains(".");
    }

    public static Assembly[] GetGameAssemblies()
    {
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        string[] resetIgnoreAssemblies = new string[] {
            "mscorlib",
            "BepInEx.Preloader",
            "BepInEx",
            "System.Core",
            "0Harmony",
            "System",
            "HarmonyXInterop",
            "System.Configuration",
            "System.Xml",
            "DemystifyExceptions",
            "StartupProfiler",
            "Purchasing.Common",
            "netstandard",
            "UniTASPlugin",
        };
        string[] resetIgnoreAssmelibes_startsWith = new string[]
        {
            "Unity.",
            "UnityEngine.",
            "Mono.",
            "MonoMod.",
            "HarmonyDTFAssembly",
        };

        return assemblies.Where((assembly) =>
        {
            foreach (string assemblyCheck in resetIgnoreAssmelibes_startsWith)
                if (assembly.FullName.StartsWith(assemblyCheck))
                    return false;
            foreach (string assemblyCheck in resetIgnoreAssemblies)
                if (assembly.FullName == assemblyCheck)
                    return false;
            return true;
        }).ToArray();
    }

    public static string GameRootDir()
    {
        return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
    }

    public static string GameName()
    {
        return AppInfo.ProductName();
    }

    public static string GameExePath()
    {
        // TODO other platform support that's not windows
        return System.IO.Path.Combine(GameRootDir(), $"{GameName()}.exe");
    }
}