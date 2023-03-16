namespace UniTAS.Plugin.GameVideoRender;

public interface IGameRender
{
    void Start();
    void Stop();
    int Fps { set; }
    int Width { set; }
    int Height { set; }
}