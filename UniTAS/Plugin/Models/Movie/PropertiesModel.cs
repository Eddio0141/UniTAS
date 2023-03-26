namespace UniTAS.Plugin.Models.Movie;

public class PropertiesModel
{
    public StartupPropertiesModel StartupProperties { get; }
    // public string LoadSaveStatePath { get; }
    // public string EndSavePath { get; }

    public PropertiesModel(StartupPropertiesModel startupPropertiesModel)
    {
        StartupProperties = startupPropertiesModel;
    }

    // public PropertiesModel(string loadSaveStatePath)
    // {
    //     StartupProperties = null;
    // }
}