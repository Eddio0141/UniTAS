using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.EngineMethods.Exceptions;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class RegisterExternalMethod : EngineExternalMethod
{
    public RegisterExternalMethod() : base("register", -1, 1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
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

        var defaultArgs = argsList.Count < 3 ? new() : argsList.GetRange(2, argsList.Count - 2);
        var defaultArgsGeneric = defaultArgs.Select(x => x as IEnumerable<ValueType>);

        var registeredIndex =
            runner.RegisterConcurrentMethod(argNameStr.Value, preUpdateBool.Value, defaultArgsGeneric);
        return new() { new IntValueType(registeredIndex) };
    }
}