using System.Collections;
using System.Collections.Generic;

namespace UniTAS.Patcher.Models.Utils;

/// <summary>
/// Priority list that allows you to insert items at a specific priority.
/// The lower the priority value, the earlier it gets inserted.
///
/// It is safe to add / remove items during iteration
/// </summary>
public class UnityEventList<T> : IEnumerable<T>
{
    // list of actual items, with the priority index as the value
    private readonly List<(T, int)> _contents = new();
    private readonly List<int> _priorities = new();

    private readonly List<(T, int)> _pendingAdd = new(); // pending add applies next iteration
    private readonly HashSet<T> _pendingRemove = new(); // pending remove applies immediately

    public void Add(T item, int priority)
    {
        _pendingRemove.Remove(item);
        var pendingAddIdx = _pendingAdd.FindIndex(x => x.Item1.GetHashCode() == item.GetHashCode());
        if (pendingAddIdx >= 0)
        {
            _pendingAdd[pendingAddIdx] = (item, priority);
        }
        else
        {
            _pendingAdd.Add((item, priority));
        }
    }

    private void AddInternal(T item, int priority)
    {
        // is the priority list smaller than priority?
        var lastPriorityIndex = _priorities.Count > 0 ? _priorities[_priorities.Count - 1] : 0;
        while (_priorities.Count <= priority)
        {
            // fill in the gaps with last index
            _priorities.Add(lastPriorityIndex);
        }

        // actually insert item
        _contents.Insert(_priorities[priority], (item, priority));

        // update priority list indexes
        for (var i = priority; i < _priorities.Count; i++)
        {
            _priorities[i]++;
        }
    }

    public void Remove(T item)
    {
        var findIdx = _pendingAdd.FindIndex(x => x.Item1.GetHashCode() == item.GetHashCode());
        if (findIdx >= 0)
            _pendingAdd.RemoveAt(findIdx);
        _pendingRemove.Add(item);
    }

    private void RemoveInternal(T item)
    {
        if (_contents.Count <= 0) return;

        var itemIndex = 0;
        while (!_contents[itemIndex].Item1.Equals(item))
        {
            itemIndex++;
            if (_contents.Count <= itemIndex) return;
        }

        var priorityIndex = _contents[itemIndex].Item2;

        _contents.RemoveAt(itemIndex);

        // update priority list indexes
        for (var i = priorityIndex; i < _priorities.Count; i++)
        {
            _priorities[i]--;
        }

        // remove useless priority values, if first and last priority values are the 0
        if (_priorities[0] + _priorities[_priorities.Count - 1] <= 0)
        {
            _priorities.Clear();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var (t, priority) in _pendingAdd)
        {
            AddInternal(t, priority);
        }

        _pendingAdd.Clear();

        foreach (var (content, _) in _contents)
        {
            if (_pendingRemove.Contains(content)) continue;
            yield return content;
        }

        foreach (var remove in _pendingRemove)
        {
            RemoveInternal(remove);
        }

        _pendingRemove.Clear();
    }
}