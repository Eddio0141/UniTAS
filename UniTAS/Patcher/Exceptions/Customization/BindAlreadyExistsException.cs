using System;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Exceptions.Customization;

public class BindAlreadyExistsException : Exception
{
    public BindAlreadyExistsException(Bind bind) : base($"Bind with name {bind.Name} already exists")
    {
    }
}