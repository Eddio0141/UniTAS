namespace UniTAS.Patcher.Interfaces.TASRenderer;

public interface IGameRender
{
    void Start();
    void Stop();
    int Fps { set; }
    int Width { set; }
    int Height { set; }
    string VideoPath { set; }
}