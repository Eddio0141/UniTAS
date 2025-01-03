using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Customization;

[Singleton]
[ExcludeRegisterIfTesting]
public class Binds(IContainer container) : IBinds
{
    private readonly List<Bind> _binds = new();

    public ReadOnlyCollection<Bind> AllBinds => _binds.AsReadOnly();

    public Bind Create(BindConfig bindConfig, bool noGenConfig = false)
    {
        var bind = container.GetInstance<Bind>(new ConstructorArg(nameof(bindConfig), bindConfig));
        // don't allow same name
        var sameName = Get(bind.Name);
        if (sameName != null)
        {
            return sameName;
        }

        _binds.Add(bind);
        bind.InitConfig(noGenConfig);
        return bind;
    }

    public Bind Get(string name)
    {
        return _binds.FirstOrDefault(bind => bind.Name == name);
    }
}