using System;

namespace UniTAS.Patcher.Services.GUI;

public interface IBrowseFileWindow
{
    event Action<string> OnFileSelected;
    event Action OnClosed;
}