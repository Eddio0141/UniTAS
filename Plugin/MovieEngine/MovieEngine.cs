using System;

namespace UniTASPlugin.MovieEngine;

public class MovieEngine
{
    private Register[] _registers;

    public MovieEngine()
    {
        _registers = new Register[Enum.GetNames(typeof(RegisterType)).Length];
    }
}