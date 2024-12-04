using System.Collections.Generic;

namespace UniTAS.Patcher.Services.UnityInfo;

public interface IGameBuildScenesInfo
{
    Dictionary<string, int> PathToIndex { get; }
    Dictionary<string, string> PathToName { get; }
    Dictionary<string, string> NameToPath { get; }
}