namespace UniTASPlugin.Movie.Models.Properties;

public class PropertiesModel
{
    public string Name { get; }
    public string Description { get; }
    public string Author { get; }

    public StartupPropertiesModel StartupProperties { get; }
    public string LoadSaveStatePath { get; }

    public string EndSavePath { get; }

    public PropertiesModel(string name, string description, string author, string endSavePath, StartupPropertiesModel startupPropertiesModel)
    {
        Name = name;
        Description = description;
        Author = author;
        EndSavePath = endSavePath;
        StartupProperties = startupPropertiesModel;
        LoadSaveStatePath = null;
    }

    public PropertiesModel(string name, string description, string author, string endSavePath, string loadSaveStatePath)
    {
        Name = name;
        Description = description;
        Author = author;
        LoadSaveStatePath = loadSaveStatePath;
        EndSavePath = endSavePath;
        StartupProperties = null;
    }
}