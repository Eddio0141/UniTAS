using MoonSharp.Interpreter;

namespace Patcher.Tests.MovieRunner;

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
    movie.frame_advance()
    i = i + 2
end

concurrent.register(preUpdate, true)

movie.frame_advance()
j = j + 1
movie.frame_advance()
j = j + 2
movie.frame_advance()
j = j + 3
movie.frame_advance()
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
    public void ConcurrentWithArgs2()
    {
        const string input = @"
function preUpdate(arg1, arg2)
    i = arg1 + arg2
end

i = 0
-- intentionally not passing args
concurrent.register(preUpdate)
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(DataType.Number, script.Globals.Get("i").Type);
        Assert.Equal(0, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentWithArgs3()
    {
        const string input = @"
function preUpdate(arg1, arg2)
    i = arg1 + arg2
end

i = 0
-- intentionally passing too many args
concurrent.register(preUpdate, true, 1, 2, 3)
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

concurrent.register(preUpdate)
concurrent.register(postUpdate, true)
";

        var (movieRunner, _, kernel) = Utils.Setup(input);
        var script = movieRunner.Script;
        var logger = kernel.GetInstance<KernelUtils.DummyLogger>();

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);
        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.Equal("preUpdate: 1", logger.Infos[0]);
        Assert.Equal("postUpdate: 3", logger.Infos[1]);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentLongRunning()
    {
        const int iterations = 10000;

        var input = $@"
function lotsOfAdv()
    for i = 1, 1000 do
        sum = sum + 1
        movie.frame_advance()
    end

    sum = sum + 1
end

function noAdv() end

sum = 0

concurrent.register(lotsOfAdv, true)
concurrent.register(noAdv, true)

for i = 1, {iterations} do
    movie.frame_advance()
end
";

        // mostly for performance testing
        var movieRunner = Utils.Setup(input).Item1;

        while (!movieRunner.Finished)
        {
            movieRunner.Update();
        }

        Assert.Equal(iterations + 1, movieRunner.Script.Globals.Get("sum").Number);
    }

    [Fact]
    public void ConcurrentRecycle()
    {
        const string input = @"
function preUpdate()
    i = i + 1
end

i = 0

concurrent.register(preUpdate, true)

for i = 1, 10 do
    movie.frame_advance()
end
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        for (var i = 0; i < 10; i++)
        {
            movieRunner.Update();
            Assert.Equal(i + 1, script.Globals.Get("i").Number);
            Assert.False(movieRunner.Finished);
        }

        movieRunner.Update();
        Assert.Equal(11, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void Unregister()
    {
        const string input = @"
function preUpdate()
    i = i + 1
end

i = 0

reference = concurrent.register(preUpdate)
movie.frame_advance()
concurrent.unregister(reference)
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(2, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(2, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void Unregister2()
    {
        const string input = @"
function preUpdate()
    i = i + 1
end

i = 0

reference = concurrent.register(preUpdate)
reference2 = concurrent.register(preUpdate)
movie.frame_advance()
concurrent.unregister(reference)
movie.frame_advance()
concurrent.unregister(reference2)
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(2, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(4, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(5, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(5, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void Unregister3()
    {
        const string input = @"
function preUpdate()
    i = i + 1
end

function preUpdate2()
    i = i + 2
end

i = 0

reference = concurrent.register(preUpdate)
reference2 = concurrent.register(preUpdate2)
movie.frame_advance()
concurrent.unregister(reference)
movie.frame_advance()
concurrent.unregister(reference2)
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(6, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(8, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(8, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void Unregister4()
    {
        const string input = @"
function preUpdate()
    i = i + 1
end

function preUpdate2()
    i = i + 2
end

i = 0

reference = concurrent.register(preUpdate)
reference2 = concurrent.register(preUpdate2, true)
movie.frame_advance()
concurrent.unregister(reference)
movie.frame_advance()
concurrent.unregister(reference2)
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        // first section runs
        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        // right after first movie.frame_advance(), unregister reference (but preUpdate so it still runs)
        Assert.Equal(6, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        // right after second movie.frame_advance(), unregister reference2 (not preUpdate so it doesn't run)
        Assert.Equal(6, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(6, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void InvalidConcurrentMethod()
    {
        const string input = @"
i = 0

reference = concurrent.register(i, true)

movie.frame_advance()

concurrent.unregister(reference)
";

        // basically it should do nothing as its not a function
        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(0, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(0, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentRunOnce()
    {
        const string input = @"
i = 0

function preUpdate()
    i = i + 1
end

concurrent.register_once(preUpdate)
movie.frame_advance()
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void ConcurrentRunOnce2()
    {
        const string input = @"
i = 0

function preUpdate()
    i = i + 1
    movie.frame_advance()
    i = i + 2
    movie.frame_advance()
end

concurrent.register_once(preUpdate)
movie.frame_advance()
movie.frame_advance()
movie.frame_advance()
movie.frame_advance()
";

        var movieRunner = Utils.Setup(input).Item1;
        var script = movieRunner.Script;

        Assert.Equal(DataType.Nil, script.Globals.Get("i").Type);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(1, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.False(movieRunner.Finished);

        movieRunner.Update();

        Assert.Equal(3, script.Globals.Get("i").Number);
        Assert.True(movieRunner.Finished);
    }
}