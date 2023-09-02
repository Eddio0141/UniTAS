using HarmonyLib;

namespace UniTAS.Patcher.Utils;

public static class DebugHelp
{
    public static string PrintClass(object obj)
    {
        var type = obj.GetType();
        var fields = AccessTools.GetDeclaredFields(type);
        var str = $"{type.Name} " + "{\n";
        foreach (var field in fields)
        {
            str += $"    {field.Name}: {field.GetValue(obj)},\n";
        }

        return str;
    }
}