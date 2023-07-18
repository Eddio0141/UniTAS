using UniTAS.Patcher.Implementations.FrameAdvancing;

namespace UniTAS.Patcher.Services.FrameAdvancing;

public interface IFrameAdvancing
{
    void FrameAdvance(uint frames, FrameAdvanceMode frameAdvanceMode);
    void Resume();
}