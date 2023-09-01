using System.Collections.Generic;

namespace UniTAS.Patcher.Models;

public class PriorityList<T>
{
    private readonly List<T> _contents = new();
    private readonly List<int> _priorities = new();

    public void Add(T item, int priority)
    {
        // is the priority list smaller than priority?
        while (_priorities.Count <= priority)
        {
            // fill in invalid indexes
            _priorities.Add(-1);
        }

        var priorityIndex = _priorities[priority];

        // if the current index is valid, we insert there
        // otherwise search for the next valid index by going backwards in priority
        while (priorityIndex < 0)
        {
            priority--;
            if (priority < 0)
            {
                priority = 0;
                priorityIndex = 0;
                break;
            }

            priorityIndex = _priorities[priority];
        }

        // actually insert
        _contents.Insert(priorityIndex, item);

        // update priority list indexes
        for (var i = priority; i < _priorities.Count; i++)
        {
            _priorities[i]++;
        }
    }

    public void Remove(T item)
    {
        var index = _contents.IndexOf(item);
        if (index < 0) return;

        _contents.RemoveAt(index);

        // finding index would be the insert index so +1
        var priorityIndex = _priorities.BinarySearch(index + 1);

        // update priority list indexes
        for (var i = priorityIndex; i < _priorities.Count; i++)
        {
            _priorities[i]--;
        }
    }

    public int Count => _contents.Count;

    public T this[int index] => _contents[index];
}