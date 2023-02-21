using System.Collections.Generic;

namespace UniTAS.Patcher;

public static class Tracker
{
    // ReSharper disable once UnusedMember.Global
    public static List<object> DontDestroyOnLoadObjects { get; set; } = new();
}