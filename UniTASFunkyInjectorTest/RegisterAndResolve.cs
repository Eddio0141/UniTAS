using System.Collections.Generic;
using System.Linq;
using UniTASFunkyInjector;
using Xunit;

// ReSharper disable IdentifierTypo

// ReSharper disable ClassNeverInstantiated.Local

namespace UniTASFunkyInjectorTest;

public class RegisterAndResolve
{
    private class Foo : IFoo
    {
        public string Test()
        {
            return "Foo";
        }
    }

    private class Foo2 : IFoo
    {
        public string Test()
        {
            return "Foo2";
        }
    }

    private interface IFoo
    {
        string Test();
    }

    private class Bar
    {
        public string WhatEver { get; set; } = "Bar";
        public IFoo InnerFoo { get; }

        public Bar(IFoo innerFoo)
        {
            InnerFoo = innerFoo;
        }
    }

    private class Baz
    {
        public List<IFoo> Fooz { get; }

        public Baz(IEnumerable<IFoo> fooz)
        {
            Fooz = fooz.ToList();
        }
    }

    [Fact]
    public void RegisterResolve()
    {
        var container = new FunkyInjectorContainer();
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo>());
        var foo = container.Resolve<IFoo>();
        Assert.NotNull(foo);
    }

    [Fact]
    public void RegisterResolveConstructor()
    {
        var container = new FunkyInjectorContainer();
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo>());
        var bar = container.Resolve<Bar>();
        Assert.NotNull(bar);
        Assert.NotNull(bar.InnerFoo);
        Assert.Equal("Foo", bar.InnerFoo.Test());
    }

    [Fact]
    public void RegisterSingleton()
    {
        var container = new FunkyInjectorContainer();
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo>());
        container.Register(ComponentStarter.For<Bar>().LifestyleSingleton());
        var bar1 = container.Resolve<Bar>();
        var bar2 = container.Resolve<Bar>();
        Assert.Same(bar1, bar2);
        bar1.WhatEver = "Foo";
        Assert.Equal("Foo", bar2.WhatEver);
    }

    [Fact]
    public void ResolveAll()
    {
        var container = new FunkyInjectorContainer();
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo>());
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo2>());
        // ReSharper disable once IdentifierTypo
        var fooz = container.ResolveAll<IFoo>().ToList();
        Assert.Equal(2, fooz.Count());
        Assert.Equal("Foo", fooz[0].Test());
        Assert.Equal("Foo2", fooz[1].Test());
    }

    [Fact]
    public void EnumerableResolve()
    {
        var container = new FunkyInjectorContainer();
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo>());
        container.Register(ComponentStarter.For<IFoo>().ImplementedBy<Foo2>());
        var baz = container.Resolve<Baz>();
        Assert.Equal(2, baz.Fooz.Count);
        Assert.Equal("Foo", baz.Fooz[0].Test());
        Assert.Equal("Foo2", baz.Fooz[1].Test());
    }
}