using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.LowLevel.OpCodes;

namespace UniTASPlugin.Movie.MovieModels.Script;

public class ScriptMethodModel
{
    public OpCode[] OpCodes { get; }
    public string Name { get; }

    public ScriptMethodModel() : this(null, new OpCode[0])
    {
    }

    public ScriptMethodModel(string name, IEnumerable<OpCode> opCodes)
    {
        Name = name;
        OpCodes = opCodes.ToArray();
    }
}