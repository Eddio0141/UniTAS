using System.Collections.Generic;
using System.Linq;
using StructureMap;
using UniTAS.Patcher.Exceptions.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Services.Customization;

namespace UniTAS.Patcher.Implementations.Customization;

[Singleton]
[ExcludeRegisterIfTesting]
public class Binds : IBinds
{
    private readonly HashSet<Bind> _binds = new();

    private readonly IContainer _container;

    public Binds(IContainer container)
    {
        _container = container;
    }

    public Bind Create(BindConfig config, bool noGenConfig = false)
    {
        var bind = _container.With(config).GetInstance<Bind>();
        if (_binds.Contains(bind)) throw new BindAlreadyExistsException(bind);
        _binds.Add(bind);
        bind.InitConfig(noGenConfig);
        return bind;
    }

    public Bind Get(string name)
    {
        return _binds.FirstOrDefault(bind => bind.Name == name);
    }
}