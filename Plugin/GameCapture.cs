using System.IO;
using Ninject;

namespace UniTASPlugin;

public static class GameCapture
{
    private static bool capturing;
    private static string captureFolder;
    private static ulong captureCount;

    public static void Update()
    {
        if (capturing)
        {
            CaptureFrame();
        }
    }

    private static void CaptureFrame()
    {
        var rev = Plugin.Kernel.Get<PatchReverseInvoker>();
        _ = $"{captureFolder}{rev.GetProperty(() => Path.DirectorySeparatorChar)}{captureCount}.png";
        // TODO sort out depending on unity version
        //ScreenCapture.CaptureScreenshot(path);
        captureCount++;
    }

    public static bool StartCapture(string path)
    {
        // check if path is a valid folder
        var rev = Plugin.Kernel.Get<PatchReverseInvoker>();
        if (!rev.Invoke(Directory.Exists, path))
            return false;

        captureFolder = path;
        capturing = true;
        captureCount = 0;
        return true;
    }

    public static void StopCapture()
    {
        capturing = false;
    }
}
