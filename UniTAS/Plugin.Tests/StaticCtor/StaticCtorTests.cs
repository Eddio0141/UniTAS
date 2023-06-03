namespace UniTAS.Plugin.Tests.StaticCtor;

public partial class StaticCtorTests
{
    private static StringWriter Setup()
    {
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        return stdout;
    }

    [Fact]
    public void StaticCtorInvokeOrder()
    {
        var stdout = Setup();

        var _ = new StaticCtorTest();

        var final = stdout.ToString();
        var expected = new[]
        {
            "StaticField",
            "StaticCtorTest",
            "StaticField2",
        };

        Assert.Equal(expected, final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void StaticCtorInvokeOrderDerived()
    {
        var stdout = Setup();

        var _ = new StaticCtorTestDerived();

        var final = stdout.ToString();
        var expected = new[]
        {
            "StaticFieldDerived",
            "StaticFieldDerived2",
            "StaticCtorTest Derived",
            "StaticFieldBase",
            "StaticFieldBase2",
            "StaticCtorTest Base",
        };

        Assert.Equal(expected, final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void StaticCtorInvokeOrderDerived2()
    {
        var stdout = Setup();

        var _ = new StaticCtorTestDerived2();

        var final = stdout.ToString();
        var expected = new[]
        {
            "StaticField2Derived",
            "StaticField2Derived2",
            "StaticFieldBase",
            "StaticFieldBase2",
            "StaticCtorTest Base",
            "Accessing StaticCtorTestBase",
            "StaticCtorTest Derived 2"
        };

        var finalSplit = final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(expected, finalSplit);
    }

    [Fact]
    public void StaticMethodInvoke()
    {
        var stdout = Setup();

        var _ = StaticCtorMethodTest.StaticMethod();

        var final = stdout.ToString();
        var expected = new[]
        {
            "StaticField",
            "StaticCtorMethodTest",
            "StaticMethod"
        };

        Assert.Equal(expected, final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void TypeAccessTest()
    {
        var stdout = Setup();

        var _ = typeof(StaticCtorAccessTest);
#pragma warning disable CS0219
        var test = nameof(StaticCtorAccessTest.StaticField);
#pragma warning restore CS0219

        var final = stdout.ToString();

        Assert.Equal(Array.Empty<string>(), final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void StaticCtorGenericArgTest()
    {
        var stdout = Setup();

        var _ = new StaticCtorGenericTest<int>();
        var __ = new StaticCtorGenericTest<string>();

        var final = stdout.ToString();
        var expected = new[]
        {
            "StaticField",
            "StaticCtorGenericTest",
            "StaticField",
            "StaticCtorGenericTest",
        };

        Assert.Equal(expected, final.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }
}