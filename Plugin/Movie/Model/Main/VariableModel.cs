﻿using UniTASPlugin.Movie.Model.Main.ValueTypes;

namespace UniTASPlugin.Movie.Model.Main;

public class VariableModel<TRet> : IReturnable<TRet>
where TRet : IValueType
{
    public string Name { get; }
    private readonly TRet _value;

    public TRet GetReturn()
    {
        return _value;
    }
}