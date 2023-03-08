namespace UniTAS.Plugin.GameVideoRender;

public interface IGameRender
{
    void Start();
    void Stop();
    int Fps { get; set; }
}