﻿using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Logic;

public class OrOpCode : LogicComparison
{
    public OrOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}