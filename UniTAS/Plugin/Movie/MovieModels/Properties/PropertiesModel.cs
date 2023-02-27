namespace UniTAS.Plugin.Movie.MovieModels.Properties;

public class PropertiesModel
{
    public StartupPropertiesModel StartupProperties { get; }
    // public string LoadSaveStatePath { get; }
    // public string EndSavePath { get; }

    public PropertiesModel()
    {
    }

    public PropertiesModel(StartupPropertiesModel startupPropertiesModel)
    {
        StartupProperties = startupPropertiesModel;
    }

    // public PropertiesModel(string loadSaveStatePath)
    // {
    //     StartupProperties = null;
    // }
}