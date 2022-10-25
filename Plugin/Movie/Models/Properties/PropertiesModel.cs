namespace UniTASPlugin.Movie.Models.Properties;

public class PropertiesModel
{
    public string MovieVersion { get; }

    public string Name { get; }
    public string Description { get; }
    public string Author { get; }
    
    public StartupPropertiesModel StartupProperties { get; }
    public string LoadSaveStatePath { get; }

    public string EndSavePath { get; }

    public PropertiesModel(string movieVersion, string name, string description, string author, string endSavePath, StartupPropertiesModel startupPropertiesModel)
    {
        MovieVersion = movieVersion;
        Name = name;
        Description = description;
        Author = author;
        EndSavePath = endSavePath;
        StartupProperties = startupPropertiesModel;
    }

    public PropertiesModel(string movieVersion, string name, string description, string author, string loadSaveStatePath, string endSavePath)
    {
        MovieVersion = movieVersion;
        Name = name;
        Description = description;
        Author = author;
        LoadSaveStatePath = loadSaveStatePath;
        EndSavePath = endSavePath;
    }
}