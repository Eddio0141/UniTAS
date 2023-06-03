namespace UniTAS.Patcher.Models.Movie;

public class PropertiesModel
{
    public StartupPropertiesModel StartupProperties { get; }

    public UpdateType UpdateType { get; }
    // public string LoadSaveStatePath { get; }
    // public string EndSavePath { get; }

    public PropertiesModel(StartupPropertiesModel startupPropertiesModel, UpdateType updateType)
    {
        StartupProperties = startupPropertiesModel;
        UpdateType = updateType;
    }

    // public PropertiesModel(string loadSaveStatePath)
    // {
    //     StartupProperties = null;
    // }
}