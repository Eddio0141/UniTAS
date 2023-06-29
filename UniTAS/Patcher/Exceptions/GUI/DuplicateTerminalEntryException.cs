using System;

namespace UniTAS.Patcher.Exceptions.GUI;

public class DuplicateTerminalEntryException : Exception
{
    public DuplicateTerminalEntryException(string[] dupes) : base(
        $"Duplicate terminal entries found: {string.Join(", ", dupes)}")
    {
    }
}