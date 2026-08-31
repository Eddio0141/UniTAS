using System.Collections.Generic;

namespace UniTAS.Patcher.Services.Localization;

public interface ILocalization
{
    Dictionary<string, Dictionary<string, string>> Tables { get; set; }
    string Locale { get; set; }
    void Load(string locale, string path);
    string Get(string key);
}