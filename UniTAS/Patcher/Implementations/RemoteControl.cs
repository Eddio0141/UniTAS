using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HarmonyLib;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class RemoteControl
{
    private readonly Script _script;

    private readonly Queue<string> _printResponse;
    private readonly object _printResponseLock;

    private readonly ILogger _logger;

    public RemoteControl(IConfig config, ILogger logger, ILiveScripting liveScripting, TerminalCmd[] commands)
    {
        var conf = config.BepInExConfigFile;

        var enableConf = conf.Bind(Config.Sections.Remote.SECTION_NAME, Config.Sections.Remote.ENABLE, false,
            "If enabled, starts a TCP/IP connection for remote controlling UniTAS");
        var ipAddrConf = conf.Bind(Config.Sections.Remote.SECTION_NAME, Config.Sections.Remote.ADDRESS, "127.0.0.1",
            "IP address to use for remote connection");
        var portConf = conf.Bind(Config.Sections.Remote.SECTION_NAME, Config.Sections.Remote.PORT, 8001,
            "Port to use for remote connection");

        if (!enableConf.Value) return;

        _logger = logger;

        _printResponse = new();
        _printResponseLock = new();
        _script = liveScripting.NewScript();
        _script.Options.DebugPrint = msg =>
        {
            lock (_printResponseLock)
            {
                _printResponse.Enqueue(msg);
            }
        };

        foreach (var cmd in commands)
        {
            _script.Globals[cmd.Name] = cmd.Callback;
            cmd.Setup();
        }

        logger.LogDebug("enabling remote control");

        if (!IPAddress.TryParse(ipAddrConf.Value, out var ipAddr))
        {
            logger.LogError($"failed to enable remote control, `{ipAddrConf.Value}` is an invalid IP address");
        }

        var thread = new Thread(() => ThreadLoop(ipAddr, portConf.Value));
        thread.Start();
    }

    private void ThreadLoop(IPAddress ipAddr, int port)
    {
        TcpListener server = new(ipAddr, port);
        try
        {
            server.Start();
        }
        catch (Exception e)
        {
            _logger.LogError($"failed to start remote server: {e}");
            return;
        }

        var data = new byte[256];

        while (true)
        {
            _logger.LogDebug($"connecting to client for remote control: {ipAddr}:{port}");

            NetworkStream networkStream;
            TcpClient client;
            try
            {
                client = server.AcceptTcpClient();
                networkStream = client.GetStream();
                networkStream.ReadTimeout = 50;
            }
            catch (Exception e)
            {
                _logger.LogError($"failed to connect to remote control client: {e}");
                continue;
            }

            _logger.LogDebug("connected to remote control client");

            var prefixPrompt = true;

            while (true)
            {
                if (!client.IsConnected())
                {
                    _logger.LogDebug("remote client has disconnected");
                    client.Close();
                    networkStream.Close();
                    break;
                }

                lock (_printResponseLock)
                {
                    while (_printResponse.Count > 0)
                    {
                        var first = $"{_printResponse.Peek()}\n";
                        var firstBytes = Encoding.ASCII.GetBytes(first);
                        try
                        {
                            _logger.LogDebug($"sending script print: {first}");
                            networkStream.Write(firstBytes, 0, firstBytes.Length);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"failed to send data to client: {e}");
                            Thread.Sleep(100);
                            continue;
                        }

                        _printResponse.Dequeue();
                    }
                }

                if (prefixPrompt)
                {
                    _logger.LogDebug("sending prefix for interpreter input");

                    var prefixBytes = ">> "u8.ToArray();
                    try
                    {
                        networkStream.Write(prefixBytes, 0, prefixBytes.Length);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"failed to send data to client: {e}");
                        Thread.Sleep(100);
                        continue;
                    }

                    prefixPrompt = false;
                }

                var bytes = 0;
                try
                {
                    bytes = networkStream.Read(data, 0, data.Length);
                }
                catch (Exception)
                {
                    //ignored
                }

                if (bytes == 0) continue;

                prefixPrompt = true;

                var response = Encoding.ASCII.GetString(data, 0, bytes);
                _logger.LogDebug($"remote got client data: {response}");

                try
                {
                    _script.DoString(response);
                }
                catch (ScriptRuntimeException e)
                {
                    var msg = e.Message;
                    const string searchKey = "type ";
                    var i = msg.IndexOf(searchKey, StringComparison.Ordinal);
                    if (i < 0)
                    {
                        _printResponse.Enqueue(e.ToString());
                        continue;
                    }

                    var typeRaw = msg.Substring(i + searchKey.Length);
                    var type = AccessTools.TypeByName(typeRaw);
                    if (type == null)
                    {
                        _printResponse.Enqueue(e.ToString());
                        continue;
                    }

                    if (UserData.IsTypeRegistered(type))
                    {
                        _printResponse.Enqueue(e.ToString());
                        continue;
                    }

                    try
                    {
                        UserData.RegisterType(type);
                    }
                    catch (Exception e2)
                    {
                        var err = $"failed to register type to MoonSharp: {e2}";
                        _logger.LogError(err);
                        _printResponse.Enqueue(err);
                        _printResponse.Enqueue(e.ToString());
                        continue;
                    }

                    // rerun
                    try
                    {
                        _script.DoString(response);
                    }
                    catch (Exception e2)
                    {
                        _printResponse.Enqueue(e2.ToString());
                    }
                }
                catch (Exception e)
                {
                    _printResponse.Enqueue(e.ToString());
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}