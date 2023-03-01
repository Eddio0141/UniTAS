using MoonSharp.Interpreter;

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
}