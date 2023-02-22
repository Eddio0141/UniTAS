using System.Collections.Generic;

namespace UniTAS.Patcher.Runtime;

public static class Tracker
{
    public static List<object> DontDestroyOnLoadObjects { get; } = new();
}