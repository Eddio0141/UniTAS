using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
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
        _logger.LogDebug("connecting...");

        TcpListener server = new(ipAddr, port);

        NetworkStream networkStream;
        try
        {
            server.Start();
            var client = server.AcceptTcpClient();
            networkStream = client.GetStream();
            networkStream.ReadTimeout = 10;
        }
        catch (Exception e)
        {
            _logger.LogError($"failed to enable remote control: {e}");
            return;
        }

        _logger.LogDebug("connected!");

        var data = new byte[256];
        while (true)
        {
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
                        break;
                    }

                    _printResponse.Dequeue();
                }
            }

            int bytes;
            try
            {
                bytes = networkStream.Read(data, 0, data.Length);
            }
            catch (Exception)
            {
                continue;
            }

            var response = Encoding.ASCII.GetString(data, 0, bytes);
            _logger.LogDebug($"remote got client data: {response}");

            try
            {
                _script.DoString(response);
            }
            catch (Exception e)
            {
                _printResponse.Enqueue(e.ToString());
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}