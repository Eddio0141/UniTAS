using System.Collections.Generic;
using UniTAS.Patcher.Extensions;

namespace Patcher.Tests.Extensions;

public class MathExtensionTests
{
    [Fact]
    public void AverageUlong()
    {
        var list = new List<ulong> { 1ul, 2ul, 3ul, 4ul, 5ul, 6ul, 7ul, 8ul, 9ul };
        var avg = list.Average();
        Assert.Equal(5ul, avg);
    }

    [Fact]
    public void AverageUlongEmpty()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var list = new List<ulong>();
        var avg = list.Average();
        Assert.Equal(0ul, avg);
    }

    [Fact]
    public void AverageUlongNonDivisible()
    {
        var list = new List<ulong> { 1ul, 2ul };
        var avg = list.Average();
        Assert.Equal(1ul, avg);
    }
}