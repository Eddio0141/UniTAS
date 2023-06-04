using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.TASRenderer;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[MoonSharpModule(Namespace = "movie")]
public class Movie
{
    private static readonly IMainThreadSpeedControl MainThreadSpeedControl =
        ContainerStarter.Kernel.GetInstance<IMainThreadSpeedControl>();

    private static readonly IGameRender GameRender = ContainerStarter.Kernel.GetInstance<IGameRender>();

    private static readonly IMovieRunnerEvents MovieRunnerEvents =
        ContainerStarter.Kernel.GetInstance<IMovieRunnerEvents>();

    private static readonly IMovieLogger MovieLogger = ContainerStarter.Kernel.GetInstance<IMovieLogger>();

    static Movie()
    {
        MovieRunnerEvents.OnMovieEnd += () => GameRender.Stop();
    }

    [MoonSharpModuleMethod]
    public static DynValue playback_speed(ScriptExecutionContext _, CallbackArguments args)
    {
        var speed = args.AsType(0, "playback_speed", DataType.Number).Number;
        MainThreadSpeedControl.SpeedMultiplier = (float)speed;
        return DynValue.Nil;
    }

    [MoonSharpModuleMethod]
    public static DynValue start_capture(ScriptExecutionContext context, CallbackArguments args)
    {
        // args
        // width, height, fps
        var arg = args.RawGet(0, true);

        var argTable = arg is { Type: DataType.Table }
            ? args.AsType(0, "start_capture", DataType.Table).Table
            : new(context.GetScript());

        var width = Utils.MoonSharp.GetTableArg(argTable, "width", 1920);
        var height = Utils.MoonSharp.GetTableArg(argTable, "height", 1080);
        var fps = Utils.MoonSharp.GetTableArg(argTable, "fps", 60);
        var path = Utils.MoonSharp.GetTableArg(argTable, "path", "output.mp4");

        if (width <= 1)
        {
            MovieLogger.LogWarning("width must be 2 or greater, falling back to 2");
            width = 2;
        }
        else if (width % 2 != 0)
        {
            MovieLogger.LogWarning("width must be even, falling back to width + 1");
            width += 1;
        }

        if (height <= 1)
        {
            MovieLogger.LogWarning("height must be 2 or greater, falling back to 2");
            height = 2;
        }
        else if (height % 2 != 0)
        {
            MovieLogger.LogWarning("height must be even, falling back to height + 1");
            height += 1;
        }

        if (fps <= 0)
        {
            MovieLogger.LogWarning("fps must be 1 or greater, falling back to 1");
            fps = 1;
        }

        GameRender.Width = width;
        GameRender.Height = height;
        GameRender.Fps = fps;
        GameRender.VideoPath = path;

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