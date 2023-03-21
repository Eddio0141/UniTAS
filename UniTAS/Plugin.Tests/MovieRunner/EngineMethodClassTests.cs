using MoonSharp.Interpreter;
using UniTAS.Plugin.Implementations.Movie.Engine.Modules;

namespace UniTAS.Plugin.Tests.MovieRunner;

public class EngineMethodClassTests
{
    [Fact]
    public void HiddenMembers()
    {
        const string input = @"
class_name = concurrent.ClassName
";

        var script = Utils.Setup(input).Item1;
        script.Update();

        Assert.Equal(DataType.Nil, script.Script.Globals.Get("class_name").Type);
    }

    [Fact]
    public void UnderscoreMembers()
    {
        var script = Utils.Setup("move_rel = mouse.move_rel").Item1;
        script.Update();

        var moveRel = script.Script.Globals.Get("move_rel");
        Assert.NotNull(moveRel);
        Assert.NotEqual(DataType.Nil, moveRel.Type);
    }

    [Fact]
    public void NoOriginalClass()
    {
        var script = Utils.Setup("").Item1;

        Assert.Equal(DataType.Nil, script.Script.Globals.Get(nameof(Env)).Type);
    }

    [Fact]
    public void Property()
    {
        var script = Utils.Setup("fps = env.fps").Item1;
        script.Update();

        var fps = script.Script.Globals.Get("fps");
        Assert.NotNull(fps);
        Assert.NotEqual(DataType.Nil, fps.Type);
    }

    [Fact]
    public void PropertyLowerCase()
    {
        var script = Utils.Setup("ft = env.frametime").Item1;
        script.Update();

        var fps = script.Script.Globals.Get("ft");
        Assert.NotNull(fps);
        Assert.NotEqual(DataType.Nil, fps.Type);
    }

    [Fact]
    public void BothMovieImplementations()
    {
        const string input = @"
playback_speed = movie.playback_speed
frame_advance = movie.frame_advance
";
        var script = Utils.Setup(input).Item1;
        script.Update();

        var playbackSpeed = script.Script.Globals.Get("playback_speed");
        Assert.NotNull(playbackSpeed);
        Assert.Equal(DataType.ClrFunction, playbackSpeed.Type);

        var frameAdvance = script.Script.Globals.Get("frame_advance");
        Assert.NotNull(frameAdvance);
        Assert.Equal(DataType.Function, frameAdvance.Type);
    }
}