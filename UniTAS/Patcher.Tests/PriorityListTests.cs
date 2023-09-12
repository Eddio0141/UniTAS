using UniTAS.Patcher.Models.Utils;

namespace Patcher.Tests;

public class PriorityListTests
{
    [Fact]
    public void AddDiffPriorities()
    {
        var list = new PriorityList<int>();
        list.Add(3, 5);
        list.Add(1, 0);
        list.Add(2, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void AddDiffPriorities2()
    {
        var list = new PriorityList<int>();
        list.Add(0, 0);
        list.Add(5, 5);
        list.Add(3, 3);
        list.Add(2, 2);
        list.Add(1, 1);
        list.Add(4, 4);

        Assert.Equal(6, list.Count);
        Assert.Equal(0, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(2, list[2]);
        Assert.Equal(3, list[3]);
        Assert.Equal(4, list[4]);
        Assert.Equal(5, list[5]);
    }

    [Fact]
    public void AddSamePriorities()
    {
        var list = new PriorityList<int>();
        list.Add(3, 5);
        list.Add(1, 0);
        list.Add(2, 5);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
        Assert.Equal(2, list[2]);
    }

    [Fact]
    public void RemoveDiffsPriorities()
    {
        var list = new PriorityList<int>();
        list.Add(3, 5);
        list.Add(1, 0);
        list.Add(2, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);

        // remove
        list.Remove(1);

        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0]);
        Assert.Equal(3, list[1]);

        // remove non-existent
        list.Remove(1);
        Assert.Equal(2, list.Count);

        // remove the rest
        list.Remove(2);
        list.Remove(3);

        Assert.Equal(0, list.Count);

        // re-add and check
        list.Add(3, 5);
        list.Add(1, 0);
        list.Add(2, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);

        // remove in odd order
        list.Remove(3);
        list.Remove(1);
        list.Remove(2);

        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void RemoveSamePriorities()
    {
        var list = new PriorityList<int>();
        list.Add(1, 0);
        list.Add(2, 0);
        list.Add(3, 0);
        list.Add(4, 0);
        list.Add(5, 0);

        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
        Assert.Equal(5, list[4]);

        // remove
        list.Remove(3);

        Assert.Equal(4, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(4, list[2]);
        Assert.Equal(5, list[3]);

        // remove non-existent
        list.Remove(3);
        Assert.Equal(4, list.Count);

        // remove the rest
        list.Remove(1);
        list.Remove(2);
        list.Remove(4);
        list.Remove(5);

        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void AddSameItemsSamePriorities()
    {
        var list = new PriorityList<int>();
        list.Add(1, 0);
        list.Add(1, 0);
        list.Add(1, 0);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(1, list[2]);
    }

    [Fact]
    public void AddSameItemsDiffPriorities()
    {
        var list = new PriorityList<int>();
        list.Add(1, 0);
        list.Add(1, 1);
        list.Add(1, 2);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(1, list[2]);
    }

    [Fact]
    public void AddRangeDiffPriorities()
    {
        var list = new PriorityList<int>();
        list.AddRange(new[] { 4, 5, 6 }, 1);
        list.AddRange(new[] { 1, 2, 3 }, 0);
        list.AddRange(new[] { 7, 8, 9 }, 2);

        Assert.Equal(9, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
        Assert.Equal(5, list[4]);
        Assert.Equal(6, list[5]);
        Assert.Equal(7, list[6]);
        Assert.Equal(8, list[7]);
        Assert.Equal(9, list[8]);
    }
}