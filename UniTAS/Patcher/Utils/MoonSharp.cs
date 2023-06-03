using MoonSharp.Interpreter;

namespace UniTAS.Patcher.Utils;

public static class MoonSharp
{
    /// <summary>
    /// Get a value from a table, or return a default value if the key is not present
    /// </summary>
    public static T GetTableArg<T>(Table table, string key, T defaultValue)
    {
        var value = table.Get(key);
        return value.Type == DataType.Nil ? defaultValue : value.ToObject<T>();
    }
}