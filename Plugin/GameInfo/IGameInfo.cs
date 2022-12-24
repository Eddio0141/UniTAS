namespace UniTASPlugin.GameInfo;

public interface IGameInfo
{
    public string UnityVersion { get; }
    public string MscorlibVersion { get; }
    public string NetStandardVersion { get; }
}