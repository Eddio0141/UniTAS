using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Tests.MovieRunner;

public class ConcurrentTests
{
    [Fact]
    public void ConcurrentPreUpdate()
    {
        const string input = @"
i = 0
j = 0

function preUpdate()
    i = i + 1
    adv()
    i = i + 2
end

concurrent.register(preUpdate, true)

adv()
j = j + 1
adv()
j = j + 2
adv()
j = j + 3
adv()
j = j + 4
";

        // execution should be like this:
        // i = 0
        // j = 0
        // i = 1 + 1 = 1 (concurrent)
        // --update--
        // i = i + 2 = 3 (concurrent)
        // j = j + 1 = 1
        // --update--
        // i = i + 1 = 4 (concurrent)
        // j = j + 2 = 3
        // --update--
        // i = i + 2 = 6 (concurrent)
        // j = j + 3 = 6
        // --update--
        // i = i + 1 = 7 (concurrent)
        // j = j + 4 = 10


        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.Equal(DataType.Nil, script.Globals.Get("j").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.Equal(0, script.Globals.Get("j").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update(); // --update--

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.Equal(1, script.Globals.Get("j").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update(); // --update--

        Assert.Equal(4, script.Globals.Get("i").Number);
        Assert.Equal(3, script.Globals.Get("j").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update(); // --update--

        Assert.Equal(6, script.Globals.Get("i").Number);
        Assert.Equal(6, script.Globals.Get("j").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update(); // --update--

        Assert.Equal(7, script.Globals.Get("i").Number);
        Assert.Equal(10, script.Globals.Get("j").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentWithArgs()
    {
        const string input = @"
function preUpdate(arg1, arg2)
    i = arg1 + arg2
end

i = 0
concurrent.register(preUpdate, true, 1, 2)
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentUpdateBoth()
    {
        // this is to test if preUpdate works
        const string input = @"
function preUpdate()
    i = i + 1
    print(""preUpdate: "" .. i)
end

function postUpdate()
    i = i + 2
    print(""postUpdate: "" .. i)
end

i = 0

concurrent.register(preUpdate, true)
concurrent.register(postUpdate, false)
";

        var (movieRunner, _, kernel) = Utils.Setup(input);
        var script = movieRunner.Script;
        var logger = kernel.GetInstance<Utils.DummyLogger>();

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);
        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.Equal("preUpdate: 1", logger.Infos[0]);
        Assert.Equal("postUpdate: 3", logger.Infos[1]);
        Assert.True(movieRunner.Finished);
    }
}