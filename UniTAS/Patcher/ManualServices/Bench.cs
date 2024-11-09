using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
#if BENCH
using System.Collections.Concurrent;
using UniTAS.Patcher.Extensions;

#else
using System.Diagnostics.CodeAnalysis;
#endif

namespace UniTAS.Patcher.ManualServices;

public static class Bench
{
#if !BENCH
    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
#endif
    public record struct MeasurementEntry(string Section, int LineNumber, string Path);

#if BENCH
    private static readonly ConcurrentDictionary<MeasurementEntry, List<ulong>> Measurements = [];
#endif

    [MustUseReturnValue]
    public static IDisposable Measure([CallerMemberName] string section = "", [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string path = null)
    {
#if BENCH
        return new BenchmarkScope(new MeasurementEntry(section, lineNumber, path));
#else
        return new NoOpDisposable();
#endif
    }

    public static Dictionary<MeasurementEntry, BenchmarkStats> GetStats()
    {
#if BENCH
        var stats = new Dictionary<MeasurementEntry, BenchmarkStats>();

        foreach (var kvp in Measurements)
        {
            var measurements = kvp.Value;
            if (measurements.Count == 0) continue;

            measurements.Sort();
            var avg = measurements.Average();
            var min = measurements[0];
            var max = measurements[measurements.Count - 1];
            var median = measurements[measurements.Count / 2];
            var percentile95 = measurements[(int)(measurements.Count * 0.95)];

            stats[kvp.Key] = new BenchmarkStats
            {
                SampleCount = measurements.Count,
                AverageMs = avg / 10000.0,
                MinMs = min / 10000.0,
                MaxMs = max / 10000.0,
                MedianMs = median / 10000.0,
                Percentile95Ms = percentile95 / 10000.0
            };
        }


        return stats;
#else
        return [];
#endif
    }

    [Conditional("BENCH")]
    public static void Reset()
    {
#if BENCh
        Measurements.Clear();
#endif
    }

#if BENCH
    private class BenchmarkScope(MeasurementEntry entry) : IDisposable
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public void Dispose()
        {
            _stopwatch.Stop();

            var elapsedList = Measurements.GetOrAdd(entry, _ => []);
            elapsedList.Add((ulong)_stopwatch.ElapsedTicks);
        }
    }
#else
    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
#endif
}

public class BenchmarkStats
{
    public int SampleCount { get; set; }
    public double AverageMs { get; set; }
    public double MinMs { [UsedImplicitly] get; set; }
    public double MaxMs { [UsedImplicitly] get; set; }
    public double MedianMs { [UsedImplicitly] get; set; }
    public double Percentile95Ms { [UsedImplicitly] get; set; }
}