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
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations;

[Singleton(RegisterPriority.RemoteControl)]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class RemoteControl
{
    private readonly Script _script;

    private readonly Queue<string> _doStrings;
    private readonly Queue<string> _printResponse = new();

    private readonly ILogger _logger;

    public RemoteControl(IConfig config, ILogger logger, ILiveScripting liveScripting, TerminalCmd[] commands,
        IUpdateEvents updateEvents)
    {
        var conf = config.BepInExConfigFile;

        var enableConf = conf.Bind(Config.Sections.Remote.SectionName, Config.Sections.Remote.Enable, false,
            "If enabled, starts a TCP/IP connection for remote controlling UniTAS");
        var ipAddrConf = conf.Bind(Config.Sections.Remote.SectionName, Config.Sections.Remote.Address, "127.0.0.1",
            "IP address to use for remote connection");
        var portConf = conf.Bind(Config.Sections.Remote.SectionName, Config.Sections.Remote.Port, 8080,
            "Port to use for remote connection");

        if (!enableConf.Value) return;

        _logger = logger;

        _doStrings = new();
        _printResponse = new();
        _script = liveScripting.NewScript();
        _script.Options.DebugPrint = msg =>
        {
            lock (_printResponse)
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

        updateEvents.OnUpdateUnconditional += UpdateUnconditional;
    }

    private void UpdateUnconditional()
    {
        lock (_doStrings)
        {
            while (_doStrings.Count > 0)
            {
                try
                {
                    _script.DoString(_doStrings.Dequeue());
                }
                catch (Exception e)
                {
                    _printResponse.Enqueue(e.ToString());
                }
            }
        }
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
            catch (ThreadAbortException)
            {
                // no error logging for those
                return;
            }
            catch (Exception e)
            {
                _logger.LogError($"failed to connect to remote control client: {e}");
                continue;
            }

            _logger.LogDebug("connected to remote control client");

            var prefixPrompt = true;
            var whoIsClient = WhoIsClient.Unknown;

            while (true)
            {
                if (!client.IsConnected())
                {
                    _logger.LogDebug("remote client has disconnected");
                    client.Close();
                    networkStream.Close();
                    break;
                }

                lock (_printResponse)
                {
                    while (_printResponse.Count > 0)
                    {
                        var first = $"{_printResponse.Peek()}\n";
                        try
                        {
                            _logger.LogDebug($"sending script print: {first}");
                            Send(networkStream, first, whoIsClient);
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
                        if (whoIsClient is WhoIsClient.Script)
                        {
                            _buffer[0] = (byte)ScriptSendTypePrefix.Prefix;
                            networkStream.Write(_buffer, 0, sizeof(byte));
                        }
                        else
                        {
                            var msgRaw = ">> "u8.ToArray();
                            networkStream.Write(msgRaw, 0, msgRaw.Length);
                        }
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
                    bytes = Receive(networkStream, out response, ref whoIsClient);
                }
                catch (Exception)
                {
                    //ignored
                }

                if (bytes == 0) continue;

                prefixPrompt = true;

                _logger.LogDebug($"remote got client data: {response}");

                lock (_doStrings)
                {
                    _doStrings.Enqueue(response);
                }
            }
        }
    }

    private enum ScriptSendTypePrefix : byte
    {
        Prefix = 0,
        Stdout = 1,
    }

    private static void Send(NetworkStream stream, string msg, WhoIsClient whoIsClient)
    {
        var msgRaw = Encoding.UTF8.GetBytes(msg);
        if (whoIsClient is not WhoIsClient.Script)
        {
            // just send msg
            stream.Write(msgRaw, 0, msgRaw.Length);
            return;
        }

        var lengthRaw = BitConverter.GetBytes((ulong)msgRaw.Length);
        // fuck you c# I hate you
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(lengthRaw);

        var prefix = new[] { (byte)ScriptSendTypePrefix.Stdout };
        var content = prefix.Concat(lengthRaw).Concat(msgRaw).ToArray();
        stream.Write(content, 0, content.Length);
    }

    private byte[] _buffer = new byte[sizeof(ulong)];

    private int Receive(NetworkStream stream, out string content, ref WhoIsClient whoIsClient)
    {
        switch (whoIsClient)
        {
            case WhoIsClient.Unknown:
            {
                // try figure out who is the client
                const int scriptInitialSendSize = 1;
                var bytes = stream.Read(_buffer, 0, scriptInitialSendSize);
                if (bytes != scriptInitialSendSize)
                {
                    content = null;
                    return bytes;
                }

                if (_buffer[0] == 0)
                {
                    whoIsClient = WhoIsClient.Script;
                    _logger.LogDebug("detected remote control client to be a script");
                    content = null;
                    return 0;
                }

                whoIsClient = WhoIsClient.Human;
                _logger.LogDebug("detected remote control client to be a human");

                // resize buffer to be big enough (probably)
                var bufferCopy = new byte[_buffer.Length];
                _buffer.CopyTo(bufferCopy, 0);
                _buffer = new byte[2048];
                bufferCopy.CopyTo(_buffer, 0);

                bytes = stream.Read(_buffer, scriptInitialSendSize, _buffer.Length - scriptInitialSendSize);
                content = Encoding.UTF8.GetString(_buffer, 0, bytes + scriptInitialSendSize);
                return bytes;
            }
            case WhoIsClient.Human:
            {
                var bytes = stream.Read(_buffer, 0, _buffer.Length);
                content = Encoding.UTF8.GetString(_buffer, 0, bytes);
                return bytes;
            }
            case WhoIsClient.Script:
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
            default:
                throw new ArgumentOutOfRangeException(nameof(whoIsClient), whoIsClient, null);
        }
    }

    private enum WhoIsClient
    {
        Unknown,
        Human,
        Script
    }
}