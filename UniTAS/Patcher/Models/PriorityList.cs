using System.Collections.Generic;

namespace UniTAS.Patcher.Models;

/// <summary>
/// Priority list that allows you to insert items at a specific priority.
/// The lower the priority value, the earlier it gets inserted.
/// </summary>
public class PriorityList<T>
{
    // list of actual items, with the priority index as the value
    private readonly List<(T, int)> _contents = new();
    private readonly List<int> _priorities = new();

    public void Add(T item, int priority)
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

    public void AddRange(IEnumerable<T> items, int priority)
    {
        // is the priority list smaller than priority?
        var lastPriorityIndex = _priorities.Count > 0 ? _priorities[_priorities.Count - 1] : 0;
        while (_priorities.Count <= priority)
        {
            // fill in the gaps with last index
            _priorities.Add(lastPriorityIndex);
        }

        // actually insert item
        var itemsList = new List<(T, int)>();
        foreach (var item in items)
        {
            itemsList.Add((item, priority));
        }

        _contents.InsertRange(_priorities[priority], itemsList);

        // update priority list indexes
        for (var i = priority; i < _priorities.Count; i++)
        {
            _priorities[i] += itemsList.Count;
        }
    }

    public void Remove(T item)
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

    public int Count => _contents.Count;

    public T this[int index] => _contents[index].Item1;
}