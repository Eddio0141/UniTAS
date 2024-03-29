using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.TASRenderer;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.TASRenderer;

/// <summary>
/// A renderer that uses unity's AudioRenderer to record audio
/// </summary>
[ExcludeRegisterIfTesting]
public class NativeAudioRenderer : AudioRenderer
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

    public NativeAudioRenderer(IAudioRendererWrapper audioRendererWrapper,
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

        _audioFileStream = new(OUTPUT_PATH, FileMode.Create);
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
            _logger.LogDebug($"Caching native array, len: {len}");
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
        header.AddRange("RIFF"u8.ToArray());
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 8));
        header.AddRange("WAVE"u8.ToArray());

        // fmt chunk
        header.AddRange("fmt "u8.ToArray());
        header.AddRange(BitConverter.GetBytes(16));
        header.AddRange(BitConverter.GetBytes((short)1));
        header.AddRange(BitConverter.GetBytes((short)_channels));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate));
        header.AddRange(BitConverter.GetBytes(AudioSettings.outputSampleRate * _channels * 2));
        header.AddRange(BitConverter.GetBytes((short)(_channels * 2)));
        header.AddRange(BitConverter.GetBytes((short)16));

        // data chunk
        header.AddRange("data"u8.ToArray());
        header.AddRange(BitConverter.GetBytes((int)_audioFileStream.Length - 44));

        _audioFileStream.Position = 0;
        _audioFileStream.Write(header.ToArray(), 0, header.Count);
    }
}