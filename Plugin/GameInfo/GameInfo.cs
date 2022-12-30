using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HarmonyLib;
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
    private string _gameDirectory;
    private string _productName;
    private bool _gotNet20Subset;
    private bool _net20Subset;

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

    /// <summary>
    /// Assumes if net20 subset is present
    /// </summary>
    public bool Net20Subset
    {
        get
        {
            if (_gotNet20Subset) return _net20Subset;

            // find File.GetAccessControl
            var getAccessControl = typeof(System.IO.File).GetMethod("GetAccessControl", new[] { typeof(string) });

            _gotNet20Subset = true;
            _net20Subset = getAccessControl == null;
            return _net20Subset;
        }
    }

    public string GameDirectory
    {
        get
        {
            var rev = _reverseInvokerFactory.GetReverseInvoker();
            rev.Invoking = true;
            if (_gameDirectory != null) return _gameDirectory;
            var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            _gameDirectory = appBase ?? Path.GetFullPath(".");
            rev.Invoking = false;
            return _gameDirectory;
        }
    }

    public string ProductName
    {
        get
        {
            if (_productName != null) return _productName;

            var productNameTraverse = Traverse.Create<Application>().Property("productName");
            if (productNameTraverse.PropertyExists())
            {
                _productName = productNameTraverse.GetValue<string>();
                return _productName;
            }

            // fallback, try get in c# way
            var rev = _reverseInvokerFactory.GetReverseInvoker();
            var os = rev.GetProperty(() => Environment.OSVersion);

            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                {
                    const string crashHandlerExe = "UnityCrashHandler64.exe";
                    var foundExe = "";
                    var foundMultipleExe = false;
                    var rootDir = GameDirectory;
                    var rootFiles = rev.Invoke(System.IO.Directory.GetFiles, rootDir);

                    // iterate over exes in game root dir
                    foreach (var path in rootFiles)
                    {
                        if (path == crashHandlerExe)
                            continue;

                        if (path.EndsWith(".exe"))
                        {
                            if (foundExe != "")
                            {
                                foundMultipleExe = true;
                                break;
                            }

                            foundExe = path;
                        }
                    }

                    if (foundExe == "")
                        throw new("Could not find exe in game root dir");

                    if (!foundMultipleExe)
                    {
                        _productName = rev.Invoke(System.IO.Path.GetFileNameWithoutExtension, foundExe);
                        return _productName;
                    }

                    // use game dir name and see if it matches exe
                    var gameDirName = rev.Invoke(a => new System.IO.DirectoryInfo(a), rootDir).Name;

                    if (rev.Invoke(System.IO.File.Exists,
                            rev.Invoke(System.IO.Path.Combine, rootDir, $"{gameDirName}.exe")))
                    {
                        _productName = gameDirName;
                        return gameDirName;
                    }

                    throw new("Could not find product name, report this issue on github to get support");
                }
                case PlatformID.Unix:
                {
                    var rootDir = GameDirectory;
                    var rootFiles = rev.Invoke(System.IO.Directory.GetFiles, rootDir);

                    var exeExtensions = new[] { ".x86_64", ".x86" };

                    // iterate over exes in game root dir
                    foreach (var rootFile in rootFiles)
                    {
                        foreach (var exeExtension in exeExtensions)
                        {
                            if (rootFile.EndsWith(exeExtension))
                            {
                                _productName = rev.Invoke(System.IO.Path.GetFileNameWithoutExtension, rootFile);
                                return _productName;
                            }
                        }
                    }

                    throw new("Could not find product name, report this issue on github to get support");
                }
                // case PlatformID.Win32S:
                //     break;
                // case PlatformID.Win32Windows:
                //     break;
                // case PlatformID.WinCE:
                //     break;
                // case PlatformID.Xbox:
                //     break;
                // case PlatformID.MacOSX:
                //     break;
                default:
                    throw new PlatformNotSupportedException(
                        "Unsupported platform to get product name, report this issue on github to get support");
            }
        }
    }
}