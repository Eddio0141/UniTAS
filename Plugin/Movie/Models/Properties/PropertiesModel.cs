namespace UniTASPlugin.Movie.Models.Properties;

public class PropertiesModel
{
    public string MovieVersion { get; }

    public string Name { get; }
    public string Description { get; }
    public string Author { get; }

    public MovieStartOption MovieStartOption { get; }
    public StartupPropertiesModel StartupProperties { get; }
    public SaveStatePropertiesModel SaveStateProperties { get; }

    public string EndSavePath { get; }

    public PropertiesModel(string movieVersion, string name, string description, string author, string endSavePath, StartupPropertiesModel startupPropertiesModel)
    {
        MovieVersion = movieVersion;
        Name = name;
        Description = description;
        Author = author;
        EndSavePath = endSavePath;
        MovieStartOption = MovieStartOption.Startup;
        StartupProperties = startupPropertiesModel;
    }

    public PropertiesModel(string movieVersion, string name, string description, string author, SaveStatePropertiesModel saveStateProperties, string endSavePath)
    {
        MovieVersion = movieVersion;
        Name = name;
        Description = description;
        Author = author;
        MovieStartOption = MovieStartOption.SaveState;
        SaveStateProperties = saveStateProperties;
        EndSavePath = endSavePath;
    }
}