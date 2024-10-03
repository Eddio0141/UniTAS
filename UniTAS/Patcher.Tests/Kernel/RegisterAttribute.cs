using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace Patcher.Tests.Kernel;

public class RegisterAttribute
{
    [Singleton(IncludeDifferentAssembly = true)]
    public class SingletonWith2Base : SingletonBase
    {
    }

    public abstract class SingletonBase : SingletonBase2, ITestDummy
    {
    }

    public abstract class SingletonBase2 : ITestDummy2
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public interface ITestDummy
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public interface ITestDummy2
    {
    }

    [Fact]
    public void SingletonBaseRecursiveRegister()
    {
        var kernel = KernelUtils.Init();

        var singleton = kernel.GetInstance<SingletonWith2Base>();
        Assert.NotNull(singleton);

        var base1 = kernel.GetInstance<SingletonBase>();
        Assert.NotNull(base1);

        Assert.Same(singleton, base1);

        var base1Interface = kernel.GetInstance<ITestDummy>();
        Assert.NotNull(base1Interface);

        Assert.Same(singleton, base1Interface);

        var base2 = kernel.GetInstance<SingletonBase2>();
        Assert.NotNull(base2);

        Assert.Same(singleton, base2);

        var base2Interface = kernel.GetInstance<ITestDummy2>();
        Assert.NotNull(base2Interface);

        Assert.Same(singleton, base2Interface);
    }
}