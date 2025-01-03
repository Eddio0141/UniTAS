using System.Collections.ObjectModel;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Services.Customization;

/// <summary>
/// Interface for creating binds
/// Doesn't check for conflicting binds as this doesn't serve as a bind always listening for keys, only when you check it
/// </summary>
public interface IBinds
{
    Bind Create(BindConfig bindConfig, bool noGenConfig = false);
    Bind Get(string name);
    ReadOnlyCollection<Bind> AllBinds { get; }
}