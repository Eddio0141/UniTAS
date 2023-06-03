namespace UniTAS.Plugin.Services;

public interface IGameInfo
{
    public string UnityVersion { get; }
    public string MscorlibVersion { get; }
    public string NetStandardVersion { get; }
    public bool Net20Subset { get; }

    public string GameDirectory { get; }
    public string ProductName { get; }
}