﻿using UniTASPlugin.ReversePatches.__System.__IO;

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
        _ = $"{captureFolder}{Path.DirectorySeparatorChar}{captureCount}.png";
        // TODO sort out depending on unity version
        //ScreenCapture.CaptureScreenshot(path);
        captureCount++;
    }

    public static bool StartCapture(string path)
    {
        // check if path is a valid folder
        if (!Directory.Exists(path))
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
