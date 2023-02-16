using System.Collections.Generic;
using UniTASPlugin.Logger;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class PrintExternalMethod : EngineExternalMethod
{
    private readonly IMovieLogger _logger;

    public PrintExternalMethod(IMovieLogger logger) : base("print", -1)
    {
        _logger = logger;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        foreach (var arg in args)
        {
            var values = new List<string>();
            foreach (var value in arg)
            {
                values.Add(value.ToString());
            }

            _logger.LogInfo(string.Join(", ", values.ToArray()));
        }

        return new();
    }
}