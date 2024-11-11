using BenchmarkDotNet.Attributes;
using UniTAS.Patcher.Utils;

namespace Patcher.Bench.Benchmarks;

public class DeepCopyBench
{
    public class CopyType
    {
        public int IntValue;
        public double DoubleValue;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string StringValue;
        public List<bool> BoolList;
        public decimal[] DecimalArray;
        public CopyType Recursive;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    private readonly CopyType _copyType = new()
    {
        IntValue = 123, DoubleValue = 123.456, StringValue = "asdf",
        BoolList = Enumerable.Repeat(false, 10000).ToList(),
        DecimalArray = Enumerable.Repeat(decimal.MaxValue, 10000).ToArray(), Recursive = new()
        {
            IntValue = 123, DoubleValue = 123.456, StringValue = "asdf",
            BoolList = Enumerable.Repeat(false, 10000).ToList(),
            DecimalArray = Enumerable.Repeat(decimal.MaxValue, 10000).ToArray()
        }
    };

    [Benchmark]
    public CopyType DeepCopy1()
    {
        return DeepCopy.MakeDeepCopy<CopyType>(_copyType);
    }
}