using System;

namespace UniTAS.Patcher.Exceptions.GUI;

public class DuplicateTerminalCmdException : Exception
{
    public DuplicateTerminalCmdException(string[] dupes) : base(
        $"Duplicate terminal commands found: {string.Join(", ", dupes)}")
    {
    }
}
