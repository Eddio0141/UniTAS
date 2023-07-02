namespace UniTAS.Patcher.Models.GUI;

public class BrowseFileWindowArgs
{
    public string Title { get; }
    public string Path { get; }

    public BrowseFileWindowArgs(string title, string path)
    {
        Title = title;
        Path = path;
    }
}