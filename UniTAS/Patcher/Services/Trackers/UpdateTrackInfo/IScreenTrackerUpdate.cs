namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IScreenTrackerUpdate
{
    void SetResolution(int width, int height, bool fullScreen);
    void SetResolution(int width, int height, bool fullScreen, int refreshRate);
    void SetResolution(int width, int height, object fullScreenMode, int refreshRate);
    void SetResolution(int width, int height, object fullScreenMode, object refreshRate);
}