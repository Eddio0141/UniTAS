using System.Reflection;
using UniTAS.Patcher.Services;

namespace Patcher.Tests.ReverseInvoker;

public partial class ReverseInvokerTests
{
    [Fact]
    public void EmptyMethod()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(ReverseInvokerTests).GetMethod(nameof(Add), BindingFlags.NonPublic | BindingFlags.Static));
        var result = method.Invoke(null, [1, 2])!;
        Assert.Equal(3, result);
    }

    [Fact]
    public void GenericInteractMethod()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(ReverseInvokerTests).GetMethod(nameof(GenericInteract), BindingFlags.NonPublic | BindingFlags.Static));
        var result = method.Invoke(null, ["foo"]);
        Assert.Equal("foo", result);
    }
}