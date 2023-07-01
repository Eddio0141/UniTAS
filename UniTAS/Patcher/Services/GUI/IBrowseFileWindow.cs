using System;

namespace UniTAS.Patcher.Services.GUI;

public interface IBrowseFileWindow
{
    void Open(string title, string path);
    event Action<string> OnFileSelected;
}