using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.ScriptEngine.Models.Movie.Script;

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