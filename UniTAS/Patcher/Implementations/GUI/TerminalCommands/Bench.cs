using System;
using System.Linq;
using UniTAS.Patcher.Interfaces.GUI;
#if BENCH
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using UniTAS.Patcher.Utils;
#endif

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Bench : TerminalCmd
{
    public override string Name => "bench";

    public override string Description =>
        "Benchmark info and control. Arg 0: pass 'dump' to dump results to disk, pass 'reset' to reset results";

    public override Delegate Callback => BenchCall;

#if BENCH
    private static void BenchCall(string mode)
#else
    private static void BenchCall(MoonSharp.Interpreter.Script script, string mode)
#endif
    {
#if !BENCH
        script.Options.DebugPrint(
            "benchmarking is disabled on this build, you need to rebuild UniTAS with the `ReleaseBench` profile");
#else
        switch (mode)
        {
            case "dump":
            {
                var stats = ManualServices.Bench.GetStats().OrderBy(s => s.Key.Path).ThenBy(s => s.Key.LineNumber)
                    .ToList();
                var statsRaw = JsonConvert.SerializeObject(stats, Formatting.Indented);
                if (!Directory.Exists(UniTASPaths.Benchmarks))
                {
                    Directory.CreateDirectory(UniTASPaths.Benchmarks);
                }

                var entryNum = Directory.GetFiles(UniTASPaths.Benchmarks).Length;
                var path = Utility.CombinePaths(UniTASPaths.Benchmarks, $"{entryNum}.json");
                File.WriteAllText(path, statsRaw);
                break;
            }
            case "reset":
                ManualServices.Bench.Reset();
                break;
        }
#endif
    }
}