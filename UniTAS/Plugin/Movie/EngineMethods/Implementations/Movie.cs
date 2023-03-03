using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.MainThreadSpeedController;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[MoonSharpModule(Namespace = "movie")]
public class Movie
{
    private static readonly IMainThreadSpeedControl MainThreadSpeedControl =
        Plugin.Kernel.GetInstance<IMainThreadSpeedControl>();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [MoonSharpModuleMethod]
    public static DynValue playback_speed(ScriptExecutionContext _, CallbackArguments args)
    {
        var speed = args.AsType(0, "playback_speed", DataType.Number).Number;
        MainThreadSpeedControl.SpeedMultiplier = (float)speed;
        return DynValue.Nil;
    }
}