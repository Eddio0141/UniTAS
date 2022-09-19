namespace UniTASPlugin;

internal static partial class Helper
{
    public static SemanticVersion GetUnityVersion()
    {
        System.Diagnostics.FileVersionInfo fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(@".\UnityPlayer.dll");
        var versionRaw = fileVersion.FileVersion;
        return new SemanticVersion(versionRaw);
    }

    public static bool ValueHasDecimalPoints(float value)
    {
        return value.ToString().Contains(".");
    }
}