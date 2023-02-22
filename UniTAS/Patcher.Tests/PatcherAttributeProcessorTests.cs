using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mono.Cecil;
using UniTAS.Patcher.PatcherUtils;
using Xunit;

namespace Patcher.Tests;

public class PatcherAttributeProcessorTests
{
    [Patcher(1000)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private static void TestPatcher(AssemblyDefinition dummy)
    {
    }

    [Patcher(500)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private static void TestPatcher2(AssemblyDefinition dummy)
    {
    }

    [Fact]
    public void PriorityOrder()
    {
        var methods = new[]
        {
            typeof(PatcherAttributeProcessorTests).GetMethod(nameof(TestPatcher),
                BindingFlags.Static | BindingFlags.NonPublic)!,
            typeof(PatcherAttributeProcessorTests).GetMethod(nameof(TestPatcher2),
                BindingFlags.Static | BindingFlags.NonPublic)!
        };

        var orderedMethods = PatcherAttributeProcessor.FilterPatcherMethods(methods)!;

        Assert.Equal(nameof(TestPatcher), orderedMethods[0].Name);
        Assert.Equal(nameof(TestPatcher2), orderedMethods[1].Name);
    }
}