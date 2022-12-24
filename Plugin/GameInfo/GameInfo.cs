using System;
using System.Diagnostics;
using System.Linq;
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

    private string _unityVersion;

    public string UnityVersion
    {
        get
        {
            if (_unityVersion != null) return _unityVersion;

            const string unityPlayerPath = @".\UnityPlayer.dll";
            var rev = _reverseInvokerFactory.GetReverseInvoker();
            if (rev.Invoke(System.IO.File.Exists, unityPlayerPath))
            {
                var fullPath = rev.Invoke(System.IO.Path.GetFullPath, unityPlayerPath);
                var fileVersion = FileVersionInfo.GetVersionInfo(fullPath);
                _unityVersion = fileVersion.FileVersion;
            }
            else
            {
                _unityVersion = Application.unityVersion;
            }

            return _unityVersion;
        }
    }

    private string _mscorlibVersion;

    public string MscorlibVersion
    {
        get
        {
            if (_mscorlibVersion != null) return _mscorlibVersion;

            _mscorlibVersion = typeof(object).Assembly.GetName().Version.ToString();

            return _mscorlibVersion;
        }
    }

    private bool _gotNetStandardVersion;
    private string _netStandardVersion;

    public string NetStandardVersion
    {
        get
        {
            if (_gotNetStandardVersion) return _netStandardVersion;

            // find netstandard assembly
            // TODO use of optimized reflection

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var netStandardAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "netstandard");
            if (netStandardAssembly == null)
            {
                _gotNetStandardVersion = true;
                return null;
            }

            _netStandardVersion = netStandardAssembly.GetName().Version.ToString();
            _gotNetStandardVersion = true;
            return _netStandardVersion;
        }
    }
}