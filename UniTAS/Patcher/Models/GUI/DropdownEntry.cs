using System;

namespace UniTAS.Patcher.Models.GUI;

public readonly struct DropdownEntry
{
    public readonly string Title;
    public readonly Action EntryFunction;

    public DropdownEntry(string title, Action entryFunction)
    {
        Title = title;
        EntryFunction = entryFunction;
    }
}