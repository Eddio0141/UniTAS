using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.UnitySafeWrappers;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;
using UniTAS.Plugin.UnitySafeWrappers.Wrappers.Unity.Collections;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

/// <summary>
/// A renderer that uses the native audio renderer to record audio
/// </summary>
public class GameNativeAudioRenderer : AudioRenderer
{
    private FileStream _audioFileStream;

    private Thread _audioProcessingThread;
    private readonly Queue<float[]> _audioProcessingQueue = new();

    private readonly int _channels = AudioSettings.speakerMode switch
    {
        AudioSpeakerMode.Raw => 1,
        AudioSpeakerMode.Mono => 1,
        AudioSpeakerMode.Stereo => 2,
        AudioSpeakerMode.Quad => 4,
        AudioSpeakerMode.Surround => 4,
        AudioSpeakerMode.Mode5point1 => 6,
        AudioSpeakerMode.Mode7point1 => 8,
        AudioSpeakerMode.Prologic => 2,
        _ => 1
    };

    private readonly IAudioRendererWrapper _audioRendererWrapper;
    private readonly IUnityInstanceWrapFactory _unityInstanceWrapFactory;
    private readonly ILogger _logger;

    public override bool Available => _audioRendererWrapper.Available;

    private readonly object[] _nativeArrayArgCache = new object[2];

    private readonly Dictionary<int, NativeArrayWrapper<float>> _nativeArrayCache = new();

    public GameNativeAudioRenderer(IAudioRendererWrapper audioRendererWrapper,
        IUnityInstanceWrapFactory unityInstanceWrapFactory, ILogger logger)
    {
        _audioRendererWrapper = audioRendererWrapper;
        _unityInstanceWrapFactory = unityInstanceWrapFactory;
        _logger = logger;
        _nativeArrayArgCache[1] = Allocator.Persistent;
    }

    public override void Start()
    {
        base.Start();

        _audioFileStream = new(OutputPath, FileMode.Create);
        _audioProcessingQueue.Clear();

        if (!_audioRendererWrapper.Start())
        {
            _logger.LogWarning("Audio renderer is already recording audio");
        }

        _audioProcessingThread = new(AudioProcessingThread);
        _audioProcessingThread.Start();

        _logger.LogInfo("Audio capture started");
    }

    public override void Stop()
    {
        base.Stop();

        _audioRendererWrapper.Stop();
        _audioProcessingThread.Join();
        WriteHeader();
        _audioFileStream.Flush();
        _audioFileStream.Close();

        foreach (var cache in _nativeArrayArgCache)
        {
            if (cache is NativeArrayWrapper<float> nativeArrayWrapper)
            {
                nativeArrayWrapper.Dispose();
            }
        }

        _nativeArrayCache.Clear();

        _logger.LogInfo("Audio capture complete");
    }

    public override void Update()
    {
        var sampleCount = _audioRendererWrapper.GetSampleCountForCaptureFrame;
        var len = sampleCount * _channels;

        if (!_nativeArrayCache.TryGetValue(len, out var nativeArray))
        {
            Trace.Write($"Caching native array, len: {len}");
            _nativeArrayArgCache[0] = len;
            nativeArray = _unityInstanceWrapFactory.CreateNew<NativeArrayWrapper<float>>(_nativeArrayArgCache);
            _nativeArrayCache.Add(len, nativeArray);
        }

        if (!_audioRendererWrapper.Render(nativeArray))
        {
            _logger.LogWarning("Something went wrong trying to grab audio data");
        }

        _audioProcessingQueue.Enqueue(nativeArray.ToArray());
    }

    private void AudioProcessingThread()
    {
        while (Recording || _audioProcessingQueue.Count > 0)
        {
            if (_audioProcessingQueue.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            // audio data is interleaved, e.g. [L, R, L, R, L, R, L, R]
            var data = _audioProcessingQueue.Dequeue();

            foreach (var sample in data)
            {
                var intData = (short)(sample * 32767);
                _audioFileStream.WriteByte((byte)(intData & 0xff));
                _audioFileStream.WriteByte((byte)((intData >> 8) & 0xff));
            }
        }
    }

    private void WriteHeader()
    {
        var header = new List<byte>();

        // RIFF header
        header.AddRange(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 8));
        header.AddRange(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });

        // fmt chunk
        header.AddRange(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
        header.AddRange(BitConverter.GetBytes(16));
        header.AddRange(BitConverter.GetBytes((short)1));
        header.AddRange(BitConverter.GetBytes((short)_channels));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate * _channels * 2));
        header.AddRange(BitConverter.GetBytes((short)(_channels * 2)));
        header.AddRange(BitConverter.GetBytes((short)16));

        // data chunk
        header.AddRange(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 44));

        _audioFileStream.Position = 0;
        _audioFileStream.Write(header.ToArray(), 0, header.Count);
    }
}