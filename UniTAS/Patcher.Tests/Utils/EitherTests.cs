using System;
using UniTAS.Patcher.Models.Utils;

namespace Patcher.Tests.Utils;

public class EitherTests
{
    [Fact]
    public void Left()
    {
        var either = new Either<int, string>(5);
        Assert.True(either.IsLeft);
        Assert.False(either.IsRight);
        Assert.Equal(5, either.Left);
        Assert.Throws<InvalidOperationException>(() => either.Right);
    }

    [Fact]
    public void Right()
    {
        var either = new Either<int, string>("test");
        Assert.False(either.IsLeft);
        Assert.True(either.IsRight);
        Assert.Equal("test", either.Right);
        Assert.Throws<InvalidOperationException>(() => either.Left);
    }

    [Fact]
    public void ImplicitLeft()
    {
        Either<int, string> either = 5;
        Assert.True(either.IsLeft);
        Assert.False(either.IsRight);
        Assert.Equal(5, either.Left);
        Assert.Throws<InvalidOperationException>(() => either.Right);
    }

    [Fact]
    public void ImplicitRight()
    {
        Either<int, string> either = "test";
        Assert.False(either.IsLeft);
        Assert.True(either.IsRight);
        Assert.Equal("test", either.Right);
        Assert.Throws<InvalidOperationException>(() => either.Left);
    }

    [Fact]
    public void Equality()
    {
        Either<int, string> either = "test";
        Either<int, string> either2 = "test";
        Assert.Equal(either, either2);

        either2 = new Either<int, string>(5);
        Assert.NotEqual(either, either2);
    }
}