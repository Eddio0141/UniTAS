using System;

namespace UniTAS.Patcher.Services.UnityInfo;

public interface IGameInfo
{
    string UnityVersion { get; }
    string MscorlibVersion { get; }
    string NetStandardVersion { get; }
    bool Net20Subset { get; }

    string GameDirectory { get; }
    string ProductName { get; }
    
    bool IsFocused { get; }

    event Action<bool> OnFocusChange;
}