using MoonSharp.Interpreter;

namespace Patcher.Tests.MovieRunner;

public class MoonsharpTests
{
    [Fact]
    public void ScriptRecycle()
    {
        // this is to test if the script can be recycled
        // NOTE: looks like you have to make a new script instance to make it work
        var script = new Script();
        const string input = @"
return function()
    i = 0
    coroutine.yield()
    i = i + 1
end
";


        var coroutine = ScriptRecycleSetup(script, input);
        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        coroutine.Resume();
        Assert.Equal(0, script.Globals.Get("i").Number);
        coroutine.Resume();
        Assert.Equal(1, script.Globals.Get("i").Number);

        // recycle
        script = new();
        ScriptRecycleSetup(script, input);
        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
    }

    private static Coroutine ScriptRecycleSetup(Script script, string input)
    {
        var method = script.DoString(input);
        return script.CreateCoroutine(method).Coroutine;
    }
}