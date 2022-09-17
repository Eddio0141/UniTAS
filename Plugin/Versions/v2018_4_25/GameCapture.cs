using System.IO;
using UnityEngine;

namespace v2018_4_25;

public static class GameCapture
{
    static bool capturing;
    static string captureFolder;
    static ulong captureCount;

    static GameCapture()
    {
        capturing = false;
    }

    public static void Update()
    {
        if (capturing)
        {
            CaptureFrame();
        }
    }

    static void CaptureFrame()
    {
        var path = $"{captureFolder}{Path.DirectorySeparatorChar}{captureCount}.png";
        ScreenCapture.CaptureScreenshot(path);
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
