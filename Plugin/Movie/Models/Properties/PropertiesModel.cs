namespace UniTASPlugin.Movie.Models.Properties;

public class PropertiesModel
{
    public string Name { get; }
    public string Description { get; }
    public string Author { get; }

    public string GameName { get; }
    public string GameVersion { get; }
    public string UnityVersion { get; }

    public MovieStartOption MovieStartOption { get; }
    public StartupPropertiesModel StartupProperties { get; }
    public SaveStatePropertiesModel SaveStateProperties { get; }

    public string EndSavePath { get; }
}