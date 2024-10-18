using System.Reflection;
using UniTAS.Patcher.Services;

namespace Patcher.Tests.ReverseInvoker;

public partial class ReverseInvokerTests
{
    // [Fact]
    public void EmptyMethod()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(ReverseInvokerTests).GetMethod(nameof(Add), BindingFlags.NonPublic | BindingFlags.Static));
        var result = method.Invoke(null, [1, 2])!;
        Assert.Equal(3, result);
    }

    // [Fact]
    public void GenericMethod()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(ReverseInvokerTests).GetMethod(nameof(Generic), BindingFlags.NonPublic | BindingFlags.Static));
        var result = method.Invoke(null, ["foo"])!;
        Assert.Equal("foo", result);
    }

    // [Fact]
    public void InnerGenericMethod()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(GenericClass<>).GetMethod("InnerGeneric", BindingFlags.NonPublic | BindingFlags.Static));
        var result = method.Invoke(null, ["foo"])!;
        Assert.Equal("foo", result);
    }

    // [Fact]
    public void GenericFieldReference()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(GenericClass<>).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Static));
        method.Invoke(null, ["foo"]);
        Assert.Equal("foo", GenericClass<string>.Value);
    }

    // [Fact]
    public void GenericFieldReference2()
    {
        var kernel = KernelUtils.Init();
        var reverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        var method = reverseInvoker.RecursiveReversePatch(
            typeof(GenericClass<>).GetMethod("AddList", BindingFlags.NonPublic | BindingFlags.Static));
        method.Invoke(null, ["foo"]);
        Assert.Equal("foo", GenericClass<string>.List[0]);
    }
}