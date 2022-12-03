using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class RegisterExternalMethod : EngineExternalMethod
{
    private readonly ScriptEngineMovieRunner _mainRunner;

    // "method name"
    public RegisterExternalMethod(ScriptEngineMovieRunner runner) : base("register", -1)
    {
        _mainRunner = runner;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args)
    {
        var argsList = args.Select(x => x.ToList()).ToList();
        switch (argsList.Count)
        {
            case < 1:
                throw new ExceptedConcurrentMethodNameException();
            case < 2:
                throw new MissingUpdateTimingFlagException();
        }

        var argName = argsList[0].First();
        if (argName is not StringValueType argNameStr)
        {
            throw new ExceptedConcurrentMethodNameException();
        }

        var preUpdate = argsList[1].First();
        if (preUpdate is not BoolValueType preUpdateBool)
        {
            throw new ExceptedConcurrentMethodNameException();
        }

        var defaultArgs = argsList.Count < 3 ? new List<List<ValueType>>() : argsList.GetRange(2, argsList.Count - 2);
        var defaultArgsGeneric = defaultArgs.Select(x => x as IEnumerable<ValueType>);

        _mainRunner.RegisterConcurrentMethod(argNameStr.Value, preUpdateBool.Value, defaultArgsGeneric);
        return new();
    }
}