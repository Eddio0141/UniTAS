using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface IBrowseFileWindowFactory
{
    IBrowseFileWindow Open(BrowseFileWindowArgs args);
}