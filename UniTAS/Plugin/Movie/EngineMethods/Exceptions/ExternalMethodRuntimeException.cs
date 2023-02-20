using System;

namespace UniTAS.Plugin.Movie.EngineMethods.Exceptions;

public class ExternalMethodRuntimeException : Exception
{
    public ExternalMethodRuntimeException(string message) : base($"There has been a runtime exception: ")
    {
    }
}