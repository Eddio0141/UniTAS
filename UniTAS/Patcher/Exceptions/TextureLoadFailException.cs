using System;

namespace UniTAS.Patcher.Exceptions;

public class TextureLoadFailException : Exception
{
    public TextureLoadFailException(string message) : base(message)
    {
    }
}