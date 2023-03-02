using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.EngineMethods.Implementations;

namespace UniTAS.Plugin.Tests.MovieRunner;

public class EngineMethodClassTests
{
    [Fact]
    public void HiddenMembers()
    {
        const string input = @"
class_name = concurrent.ClassName
";

        Assert.Throws<ScriptRuntimeException>(() => Utils.Setup(input));
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
}