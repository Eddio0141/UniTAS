using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher;

public static class Tracker
{
    // ReSharper disable once UnusedMember.Global
    public static List<GameObject> DontDestroyOnLoadObjects { get; } = new();
}