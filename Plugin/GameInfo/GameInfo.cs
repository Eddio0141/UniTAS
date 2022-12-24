using System.Diagnostics;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin.GameInfo;

public class GameInfo : IGameInfo
{
    private readonly IReverseInvokerFactory _reverseInvokerFactory;

    public GameInfo(IReverseInvokerFactory reverseInvokerFactory)
    {
        _reverseInvokerFactory = reverseInvokerFactory;
    }

    public string UnityVersion
    {
        get
        {
            const string unityPlayerPath = @".\UnityPlayer.dll";
            string versionRaw;
            var rev = _reverseInvokerFactory.GetReverseInvoker();
            if (rev.Invoke(System.IO.File.Exists, unityPlayerPath))
            {
                var fullPath = rev.Invoke(System.IO.Path.GetFullPath, unityPlayerPath);
                var fileVersion = FileVersionInfo.GetVersionInfo(fullPath);
                versionRaw = fileVersion.FileVersion;
            }
            else
            {
                versionRaw = Application.unityVersion;
            }

            return versionRaw;
        }
    }

    public string MscorlibVersion => typeof(object).Assembly.GetName().Version.ToString();
}