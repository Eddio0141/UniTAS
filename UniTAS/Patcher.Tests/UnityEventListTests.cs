using UniTAS.Patcher.Models.Utils;

namespace Patcher.Tests;

public class UnityEventListTests
{
    [Fact]
    public void AddDiffPriorities()
    {
        var list = new UnityEventList<int>
        {
            { 3, 5 },
            { 1, 0 },
            { 2, 3 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void AddDiffPriorities2()
    {
        var list = new UnityEventList<int>
        {
            { 0, 0 },
            { 5, 5 },
            { 3, 3 },
            { 2, 2 },
            { 1, 1 },
            { 4, 4 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(0, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(4, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(5, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void AddSamePriorities()
    {
        var list = new UnityEventList<int>
        {
            { 3, 5 },
            { 1, 0 },
            { 2, 5 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void RemoveDiffsPriorities()
    {
        var list = new UnityEventList<int>
        {
            { 3, 5 },
            { 1, 0 },
            { 2, 3 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove
        list.Remove(1);

        iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove non-existent
        list.Remove(1);

        iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove the rest
        list.Remove(2);
        list.Remove(3);

        iter = list.GetEnumerator();
        Assert.False(iter.MoveNext());
        iter.Dispose();

        // re-add and check
        list.Add(3, 5);
        list.Add(1, 0);
        list.Add(2, 3);

        iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove in odd order
        list.Remove(3);
        list.Remove(1);
        list.Remove(2);

        iter = list.GetEnumerator();
        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void RemoveSamePriorities()
    {
        var list = new UnityEventList<int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(4, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(5, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove
        list.Remove(3);

        iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(4, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(5, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove non-existent
        list.Remove(3);

        iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(4, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(5, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();

        // remove the rest
        list.Remove(1);
        list.Remove(2);
        list.Remove(4);
        list.Remove(5);

        iter = list.GetEnumerator();
        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void AddSameItemsSamePriorities()
    {
        var list = new UnityEventList<int>
        {
            { 1, 0 },
            { 1, 0 },
            { 1, 0 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void AddSameItemsDiffPriorities()
    {
        var list = new UnityEventList<int>
        {
            { 1, 0 },
            { 1, 1 },
            { 1, 2 }
        };

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void AddDuringIter()
    {
        var list = new UnityEventList<int>
        {
            { 1, 0 },
            { 2, 0 },
        };

        var i = 0;
        foreach (var _ in list)
        {
            list.Add(3 + i, 0);
            i++;
        }

        Assert.Equal(2, i);

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(4, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }

    [Fact]
    public void RemoveDuringIter()
    {
        var list = new UnityEventList<int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 },
        };

        foreach (var num in list)
        {
            if (num > 3)
            {
                list.Remove(num);
            }
        }

        var iter = list.GetEnumerator();
        Assert.True(iter.MoveNext());
        Assert.Equal(1, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(2, iter.Current);
        Assert.True(iter.MoveNext());
        Assert.Equal(3, iter.Current);

        Assert.False(iter.MoveNext());
        iter.Dispose();
    }
}