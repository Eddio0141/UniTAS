using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Services.Customization;

public interface IBinds
{
    Bind Create(BindConfig config);
}