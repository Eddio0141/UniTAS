using System;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Exceptions.Customization;

public class ConflictingBindNameException : Exception
{
    public ConflictingBindNameException(Bind bind) : base($"Bind with name {bind.Name} already exists")
    {
    }
}