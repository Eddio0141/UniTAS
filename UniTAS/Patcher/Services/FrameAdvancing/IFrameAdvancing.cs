namespace UniTAS.Patcher.Services.FrameAdvancing;

public interface IFrameAdvancing
{
    void FrameAdvance(uint frames);
    void TogglePause();
}