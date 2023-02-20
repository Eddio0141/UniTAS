using System;
using UniTAS.Plugin.GameEnvironment.InnerState;

namespace UniTAS.Plugin.GameEnvironment;

public class UnityPaths
{
    public string PersistentDataPath { get; }

    public UnityPaths(Os os, string username)
    {
        switch (os)
        {
            case Os.Windows:
            {
                // ReSharper disable once CommentTypo
                // %userprofile%\AppData\LocalLow\<companyname>\<productname>
                // TODO get product name
                // TODO get company name
                PersistentDataPath =
                    $@"C:\Users\{username}\AppData\LocalLow\DefaultCompany\DefaultProduct";
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os), os, null);
        }
    }
}