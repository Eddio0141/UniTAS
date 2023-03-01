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
}