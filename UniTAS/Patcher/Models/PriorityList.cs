using System.Collections.Generic;
using System.Linq;

namespace UniTAS.Patcher.Models;

/// <summary>
/// Priority list that allows you to insert items at a specific priority.
/// The lower the priority value, the earlier it gets inserted.
/// </summary>
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
        var processingPriority = priority;
        while (priorityIndex < 0)
        {
            processingPriority--;
            if (processingPriority < 0)
            {
                // we reached the first without hitting anything
                priorityIndex = 0;
                _priorities[priority] = 0;

                break;
            }

            priorityIndex = _priorities[processingPriority];
            // found the insert index, update the priority list
            _priorities[priority] = priorityIndex;
        }

        // actually insert
        _contents.Insert(priorityIndex, item);

        // update priority list indexes
        for (var i = priority; i < _priorities.Count; i++)
        {
            if (_priorities[i] >= 0)
            {
                _priorities[i]++;
            }
        }
    }

    public void Remove(T item)
    {
        var index = _contents.IndexOf(item);
        if (index < 0) return;

        _contents.RemoveAt(index);

        // finding index would be the insert index so +1
        var priorityIndex = _priorities.IndexOf(index + 1);

        // update priority list indexes
        for (var i = priorityIndex; i < _priorities.Count; i++)
        {
            if (_priorities[i] >= 0)
            {
                _priorities[i]--;
            }
        }

        // remove useless priority values
        while (_priorities.Last() < 0)
        {
            _priorities.RemoveAt(_priorities.Count - 1);
        }
    }

    public int Count => _contents.Count;

    public T this[int index] => _contents[index];
}