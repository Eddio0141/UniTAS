using System.Collections.Generic;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.Movie.Models.Properties;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin;

public static class TAS
{
    public static PropertiesModel CurrentPropertiesModel { get; private set; }
    public static ulong FrameCountMovie { get; private set; }
    static int pendingMovieStartFixedUpdate = -1;
    public static bool PreparingRun { get; private set; } = false;

    public static void Update()
    {
        SaveState.Main.Update();
        UpdateMovie();
    }

    public static void FixedUpdate()
    {
        if (pendingMovieStartFixedUpdate > -1)
        {
            if (pendingMovieStartFixedUpdate == 0)
            {
                RunMoviePending();
            }
            pendingMovieStartFixedUpdate--;
        }
    }

    static void UpdateMovie()
    {
        if (!Running)
            return;

        FrameCountMovie++;
        if (!CheckCurrentMovieEnd())
            return;

        var fb = CurrentPropertiesModel.Framebulks[currentFramebulkIndex];
        if (currentFramebulkFrameIndex >= fb.FrameCount)
        {
            currentFramebulkIndex++;
            if (!CheckCurrentMovieEnd())
                return;

            currentFramebulkFrameIndex = 0;
            fb = CurrentPropertiesModel.Framebulks[currentFramebulkIndex];
        }

        TimeWrap.captureFrametime = fb.Frametime;
        GameControl(fb);

        currentFramebulkFrameIndex++;
    }

    static bool CheckCurrentMovieEnd()
    {
        if (currentFramebulkIndex >= CurrentPropertiesModel.Framebulks.Count)
        {
            Running = false;

            Plugin.Log.LogInfo("Movie end");

            return false;
        }

        return true;
    }

    static void GameControl(Framebulk fb)
    {
        MouseState.Position = new Vector2(fb.Mouse.X, fb.Mouse.Y);
        MouseState.LeftClick = fb.Mouse.Left;
        MouseState.RightClick = fb.Mouse.Right;
        MouseState.MiddleClick = fb.Mouse.Middle;

        List<string> axisMoveSetDefault = new();
        foreach (var pair in AxisState.Values)
        {
            var key = pair.Key;
            if (!fb.Axises.AxisMove.ContainsKey(key))
                axisMoveSetDefault.Add(key);
        }
        foreach (var key in axisMoveSetDefault)
        {
            if (AxisState.Values.ContainsKey(key))
                AxisState.Values[key] = default;
            else
                AxisState.Values.Add(key, default);
        }
        foreach (var axisValue in fb.Axises.AxisMove)
        {
            var axis = axisValue.Key;
            var value = axisValue.Value;
            if (AxisState.Values.ContainsKey(axis))
            {
                AxisState.Values[axis] = value;
            }
            else
            {
                AxisState.Values.Add(axis, value);
            }
        }
    }

    public static void RunMovie(PropertiesModel propertiesModel)
    {
        PreparingRun = true;
        FrameCountMovie = 0;
        currentFramebulkIndex = 0;
        currentFramebulkFrameIndex = 1;

        CurrentPropertiesModel = propertiesModel;

        // force framerate to run fixed for Update and FixedUpdate sync
        if (CurrentPropertiesModel.Framebulks.Count > 0)
        {
            var firstFb = CurrentPropertiesModel.Framebulks[0];

            FakeGameState.InputLegacy.Main.Clear();
            TimeWrap.captureFrametime = firstFb.Frametime;
            GameControl(firstFb);

            if (currentFramebulkFrameIndex >= firstFb.FrameCount)
            {
                currentFramebulkFrameIndex = 0;
                currentFramebulkIndex++;
            }
        }

        pendingMovieStartFixedUpdate = 1;
        Plugin.Log.LogInfo("Starting movie, pending FixedUpdate call");
    }

    static void RunMoviePending()
    {
        PreparingRun = false;
        Running = true;

        FakeGameState.SystemInfo.DeviceType = CurrentPropertiesModel.DeviceType;
        // TODO fullscreen
        Screen.SetResolution(CurrentPropertiesModel.Width, CurrentPropertiesModel.Height, false, 60);

        GameRestart.SoftRestart(CurrentPropertiesModel.Time);
        Plugin.Log.LogInfo($"Movie start: {CurrentPropertiesModel}");
    }
}
