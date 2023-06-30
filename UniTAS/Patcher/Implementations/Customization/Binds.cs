using System.Collections.Generic;
using StructureMap;
using UniTAS.Patcher.Exceptions.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Services.Customization;

namespace UniTAS.Patcher.Implementations.Customization;

[Singleton]
public class Binds : IBinds
{
    private readonly HashSet<Bind> _binds = new();

    private readonly IContainer _container;

    public Binds(IContainer container)
    {
        _container = container;
    }

    public Bind Create(BindConfig config)
    {
        var bind = _container.With(config).GetInstance<Bind>();
        if (_binds.Contains(bind)) throw new BindAlreadyExistsException(bind);
        _binds.Add(bind);
        bind.InitConfig();
        return bind;
    }
}