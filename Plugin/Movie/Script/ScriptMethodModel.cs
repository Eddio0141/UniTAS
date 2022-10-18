using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.MovieEngine.OpCodes;

namespace UniTASPlugin.Movie.Script;

public class ScriptMethodModel
{
    public OpCodeBase[] OpCodes { get; }
    public string Name { get; }

    public ScriptMethodModel() : this(null, new OpCodeBase[0]) { }

    public ScriptMethodModel(string name, IEnumerable<OpCodeBase> opCodes)
    {
        Name = name;
        OpCodes = opCodes.ToArray();
    }
}