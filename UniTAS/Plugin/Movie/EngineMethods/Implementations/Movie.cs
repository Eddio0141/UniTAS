using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.GameVideoRender;
using UniTAS.Plugin.MainThreadSpeedController;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[MoonSharpModule(Namespace = "movie")]
public class Movie
{
    private static readonly IMainThreadSpeedControl MainThreadSpeedControl =
        Plugin.Kernel.GetInstance<IMainThreadSpeedControl>();

    private static readonly IGameRender GameRender = Plugin.Kernel.GetInstance<IGameRender>();

    private static readonly IMovieRunner MovieRunner = Plugin.Kernel.GetInstance<IMovieRunner>();

    static Movie()
    {
        MovieRunner.OnMovieEnd += () => GameRender.Stop();
    }

    [MoonSharpModuleMethod]
    public static DynValue playback_speed(ScriptExecutionContext _, CallbackArguments args)
    {
        var speed = args.AsType(0, "playback_speed", DataType.Number).Number;
        MainThreadSpeedControl.SpeedMultiplier = (float)speed;
        return DynValue.Nil;
    }

    [MoonSharpModuleMethod]
    public static DynValue start_capture(ScriptExecutionContext _, CallbackArguments args)
    {
        GameRender.Start();
        return DynValue.Nil;
    }

    [MoonSharpModuleMethod]
    public static DynValue stop_capture(ScriptExecutionContext _, CallbackArguments args)
    {
        GameRender.Stop();
        return DynValue.Nil;
    }
}