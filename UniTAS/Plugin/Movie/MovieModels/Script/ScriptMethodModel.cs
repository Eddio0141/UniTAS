using System.Collections.Generic;
using System.Linq;
using UniTAS.Plugin.Movie.LowLevel.OpCodes;

namespace UniTAS.Plugin.Movie.MovieModels.Script;

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