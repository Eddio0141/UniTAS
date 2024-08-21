using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton(RegisterPriority.RemoteControl)]
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
                        try
                        {
                            _logger.LogDebug($"sending script print: {first}");
                            Send(networkStream, first);
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

                    try
                    {
                        Send(networkStream, ">> ");
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
                string response = null;
                try
                {
                    bytes = Receive(networkStream, out response);
                }
                catch (Exception)
                {
                    //ignored
                }

                if (bytes == 0) continue;

                prefixPrompt = true;

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
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void Send(NetworkStream stream, string msg)
    {
        var msgRaw = Encoding.UTF8.GetBytes(msg);
        var lengthRaw = BitConverter.GetBytes((ulong)msgRaw.Length);
        // fuck you c# I hate you
        if (!BitConverter.IsLittleEndian)
            lengthRaw = lengthRaw.Reverse().ToArray();

        var content = lengthRaw.Concat(msgRaw).ToArray();
        stream.Write(content, 0, content.Length);
    }

    private byte[] _buffer = new byte[sizeof(ulong)];

    private int Receive(NetworkStream stream, out string content)
    {
        var bytes = stream.Read(_buffer, 0, sizeof(ulong));
        if (bytes == 0)
        {
            content = null;
            return bytes;
        }

        // fucking c#
        var length = (int)BitConverter.ToInt64(
            BitConverter.IsLittleEndian ? _buffer : _buffer.Take(4).Reverse().ToArray(),
            0);

        // resize as required
        if (_buffer.Length < length)
        {
            _buffer = new byte[length];
        }

        bytes = stream.Read(_buffer, 0, length);

        content = Encoding.UTF8.GetString(_buffer, 0, length);
        return bytes;
    }
}