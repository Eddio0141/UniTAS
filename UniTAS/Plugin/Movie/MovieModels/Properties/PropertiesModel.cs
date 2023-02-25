namespace UniTAS.Plugin.Movie.MovieModels.Properties;

public class PropertiesModel
{
    public string Name { get; }
    public string Description { get; }
    public string Author { get; }

    public StartupPropertiesModel StartupProperties { get; }
    public string LoadSaveStatePath { get; }

    public string EndSavePath { get; }
    public string LuaScriptPath { get; }

    public PropertiesModel(string name, string description, string author, string endSavePath,
        StartupPropertiesModel startupPropertiesModel, string luaScriptPath)
    {
        Name = name;
        Description = description;
        Author = author;
        EndSavePath = endSavePath;
        StartupProperties = startupPropertiesModel;
        LuaScriptPath = luaScriptPath;
        LoadSaveStatePath = null;
    }

    public PropertiesModel(string name, string description, string author, string endSavePath, string loadSaveStatePath,
        string luaScriptPath)
    {
        Name = name;
        Description = description;
        Author = author;
        LoadSaveStatePath = loadSaveStatePath;
        LuaScriptPath = luaScriptPath;
        EndSavePath = endSavePath;
        StartupProperties = null;
    }
}