using System;
using System.Linq;
using HarmonyLib;

namespace UniTAS.Patcher.Models.Movie;

public class PropertiesModel(DateTime startTime, float frameTime, long seed, WindowState windowState, string[] fsPassthrough, UpdateType updateType)
{
    public UpdateType UpdateType { get; } = updateType;

    // public Os Os { get; }
    public WindowState WindowState { get; } = windowState;
    public DateTime StartTime { get; } = startTime;
    public float FrameTime { get; } = frameTime;
    public long Seed { get; } = seed;
    public string[] FsPassthrough { get; } = fsPassthrough;

    public override string ToString()
    {
        return $"UpdateType: {UpdateType}, WindowState: {WindowState}, StartTime: {StartTime}, FrameTime: {FrameTime}, Seed: {Seed}, FsPassthrough: [{FsPassthrough.Join()}]";
    }
}
